using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using Assets.Ancible_Tools.Scripts.System.SaveData.Building;
using Assets.Ancible_Tools.Scripts.Traits;
using Assets.Resources.Ancible_Tools.Scripts.System.Items;
using Assets.Resources.Ancible_Tools.Scripts.System.Pathing;
using Assets.Resources.Ancible_Tools.Scripts.System.Templates;
using Assets.Resources.Ancible_Tools.Scripts.System.UI;
using Assets.Resources.Ancible_Tools.Scripts.System.UI.Alerts;
using CreativeSpore.SuperTilemapEditor;
using MessageBusLib;
using UnityEngine;

namespace Assets.Resources.Ancible_Tools.Scripts.System.Building
{
    public class WorldBuildingManager : MonoBehaviour
    {
        public static WorldBuilding[] Available => _instance._availableBuildings.ToArray();
        public static bool Active => _instance._buildingController.gameObject.activeSelf;
        public static float SellBackPerecent => _instance._sellBackPercentage;

        private static WorldBuildingManager _instance = null;

        [SerializeField] private BuildingTemplateController _buildingController = null;
        [SerializeField] private STETilemap _validBuildingTilemap;
        [SerializeField] private STETilemap _invalidBuildingTilemap;
        [SerializeField] private WorldBuilding[] _startingBuildings = new WorldBuilding[0];
        [SerializeField] private string _buildingFolder = string.Empty;
        [SerializeField] private float _sellBackPercentage = 0f;
        [SerializeField] private Trait[] _applyOnSell = new Trait[0];

        private Vector2 _mousePos = Vector2.zero;
        private MapTile _currentMapTile = null;
        private bool _validBuild = false;
        private List<WorldBuilding> _availableBuildings = new List<WorldBuilding>();

        private SetupBuildingMessage _setupBuildingMsg = new SetupBuildingMessage();

        private Dictionary<string, WorldBuilding> _allBuildings = new Dictionary<string, WorldBuilding>();
        

        private Dictionary<BuildingData, GameObject> _currentBuildings = new Dictionary<BuildingData, GameObject>();
        private List<Vector2Int> _passiveBuildingTiles = new List<Vector2Int>();

        private GameObject _movingBuilding = null;
        private MapTile[] _movingOriginTiles = new MapTile[0];
        private WorldBuilding _movingTemplate = null;

        void Awake()
        {
            if (_instance)
            {
                Destroy(gameObject);
                return;
            }

            _availableBuildings = _startingBuildings.ToList();
            _instance = this;
            _buildingController.Clear();
            _allBuildings = UnityEngine.Resources.LoadAll<WorldBuilding>(_buildingFolder).ToDictionary(b => b.name, b => b);
            Debug.Log($"Loaded {_allBuildings.Count} World Buildings");
            SubscribeToMessages();
        }

        public static void SetupBuilding(WorldBuilding building)
        {
            _instance._buildingController.Setup(building);
            _instance.UpdateBuildingTile();
            _instance.gameObject.SendMessage(WorldBuildingActiveMessage.INSTANCE);
        }

        public static void ClearBuilding()
        {
            if (_instance._buildingController.gameObject.activeSelf)
            {
                _instance._buildingController.Clear();
                _instance._currentMapTile = null;
                _instance.gameObject.SendMessage(WorldBuildingStoppedMessage.INSTANCE);
                _instance._validBuildingTilemap.ClearMap();
                _instance._invalidBuildingTilemap.ClearMap();
                _instance._validBuildingTilemap.Refresh();
                _instance._invalidBuildingTilemap.Refresh();
            }

        }

        public static BuildingData[] GetData()
        {
            var buildings = _instance._currentBuildings.ToArray();
            var queryMapTileMsg = MessageFactory.GenerateQueryMapTileMsg();
            var queryBuildingParameterDataMsg = MessageFactory.GenerateQueryBuildingParamterDataMsg();
            foreach (var building in buildings)
            {
                queryMapTileMsg.DoAfter = tile => building.Key.Position = tile.Position.ToData();
                _instance.gameObject.SendMessageTo(queryMapTileMsg, building.Value);

                queryBuildingParameterDataMsg.DoAfter = data => building.Key.Parameter = data;
                _instance.gameObject.SendMessageTo(queryBuildingParameterDataMsg, building.Value);
            }
            MessageFactory.CacheMessage(queryMapTileMsg);

            
            

            return buildings.Select(kv => kv.Key).ToArray();
        }

        public static void Clear()
        {
            var buildings = _instance._currentBuildings.ToArray();
            foreach (var building in buildings)
            {
                Destroy(building.Value);
                building.Key.Dispose();
            }
            _instance._currentBuildings.Clear();
            _instance._buildingController.Clear();
            _instance._currentMapTile = null;
            _instance._validBuild = false;
            _instance._invalidBuildingTilemap.ClearMap();
            _instance._validBuildingTilemap.ClearMap();
            _instance._passiveBuildingTiles.Clear();
        }

        public static void SetFromBuildingsData(BuildingData[] buildings)
        {
            Clear();
            foreach (var building in buildings)
            {
                if (_instance._allBuildings.TryGetValue(building.Building, out var worldBuilding))
                {
                    var mapTile = WorldController.Pathing.GetTileByPosition(building.Position.ToVector());
                    if (mapTile != null)
                    {
                        GenerateBuildingAtPosition(worldBuilding, mapTile, building.Id);
                    }
                }
            }
        }

        public static void SetupMovingBuilding(GameObject obj, MapTile[] tiles, WorldBuilding worldBuilding)
        {
            _instance._movingTemplate = worldBuilding;
            _instance._movingBuilding = obj;
            _instance._movingOriginTiles = tiles;
            _instance.UpdateMovingBuildingTiles();

            var setSpriteAlphaMsg = MessageFactory.GenerateSetSpriteAlphaMsg();
            setSpriteAlphaMsg.Alpha = .5f;
            _instance.gameObject.SendMessageTo(setSpriteAlphaMsg, obj);
            MessageFactory.CacheMessage(setSpriteAlphaMsg);

            _instance.gameObject.SendMessage(WorldBuildingActiveMessage.INSTANCE);
        }

        public static void AddPassivePositions(Vector2Int[] positions)
        {
            _instance._passiveBuildingTiles.AddRange(positions.Where(p => !_instance._passiveBuildingTiles.Contains(p)));
        }

        public static void RemovePassivePositions(Vector2Int[] positions)
        {
            _instance._passiveBuildingTiles.RemoveAll(positions.Contains);
        }

        public static void SellBuilding(GameObject obj)
        {
            WorldBuilding building = null;
            var queryBuildingMsg = MessageFactory.GenerateQueryBuildingMsg();
            queryBuildingMsg.DoAfter = (worldBuilding, tile, id) =>
            {
                building = worldBuilding;
            };
            _instance.gameObject.SendMessageTo(queryBuildingMsg, obj);
            MessageFactory.CacheMessage(queryBuildingMsg);

            if (building)
            {
                var pair = _instance._currentBuildings.FirstOrDefault(kv => kv.Value == obj);
                if (pair.Value)
                {
                    _instance.gameObject.AddTraitsToUnit(_instance._applyOnSell, pair.Value);
                    WorldStashController.AddGold(CalculateSellbackValue(building.Cost));
                    pair.Key.Dispose();
                    _instance._currentBuildings.Remove(pair.Key);
                    Destroy(obj);
                }
            }
        }

        public static int CalculateSellbackValue(int gold)
        {
            return Mathf.RoundToInt(gold * SellBackPerecent);
        }

        public static void UpgradeBuilding(GameObject obj, MapTile tile, WorldBuilding baseBuilding, WorldBuilding upgrade)
        {
            if (baseBuilding.Upgrades.Buildings.Contains(upgrade))
            {
                var pair = _instance._currentBuildings.FirstOrDefault(kv => kv.Value == obj);
                if (pair.Value == obj)
                {
                    Destroy(pair.Value);
                    _instance._currentBuildings.Remove(pair.Key);
                    GenerateBuildingAtPosition(upgrade, tile);
                }
            }
        }

        private static void GenerateBuildingAtPosition(WorldBuilding building, MapTile tile, string id = "")
        {
            var template = building.Template;
            var buildingUnit = template.GenerateUnit(_instance.transform, tile.World);
            if (string.IsNullOrEmpty(id))
            {
                id = Guid.NewGuid().ToString();
            }
            _instance._setupBuildingMsg.Building = building;
            _instance._setupBuildingMsg.Id = id;
            _instance.gameObject.SendMessageTo(_instance._setupBuildingMsg, buildingUnit.gameObject);

            var setMapTileMsg = MessageFactory.GenerateSetMapTileMsg();
            setMapTileMsg.Tile = tile;
            _instance.gameObject.SendMessageTo(setMapTileMsg, buildingUnit.gameObject);
            MessageFactory.CacheMessage(setMapTileMsg);

            _instance._currentBuildings.Add(new BuildingData { Building = building.name, Id = id, Position = tile.Position.ToData() }, buildingUnit.gameObject);
        }

        private void UpdateBuildingTile()
        {
            var pos = WorldCameraController.Camera.ScreenToWorldPoint(_mousePos).ToVector2();
            var mapTile = WorldController.Pathing.GetMapTileByWorldPosition(pos);
            if (mapTile != null && (_currentMapTile == null || _currentMapTile != mapTile))
            {
                _validBuildingTilemap.ClearMap();
                _invalidBuildingTilemap.ClearMap();
                _currentMapTile = mapTile;
                _buildingController.transform.SetTransformPosition(_currentMapTile.World);
                var requiredTilePositions = _buildingController.Building.GetRequiredPositions(_currentMapTile.Position);
                var validTiles = requiredTilePositions.Select(WorldController.Pathing.GetTileByPosition).Where(t => t != null && !t.Block && !_passiveBuildingTiles.Contains(t.Position)).ToArray();
                var invalidTiles = requiredTilePositions.Where(t => validTiles.FirstOrDefault(v => v.Position == t) == null).ToArray();
                for (var i = 0; i < validTiles.Length; i++)
                {
                    var tile = validTiles[i];
                    _validBuildingTilemap.SetTile(tile.Position.x, tile.Position.y, 0);
                }

                for (var i = 0; i < invalidTiles.Length; i++)
                {
                    var tile = invalidTiles[i];
                    _invalidBuildingTilemap.SetTile(tile.x, tile.y, 0);
                }
                _validBuildingTilemap.Refresh();
                _invalidBuildingTilemap.Refresh();
                _validBuild = invalidTiles.Length <= 0;

            }
        }

        private void UpdateMovingBuildingTiles()
        {
            var pos = WorldCameraController.Camera.ScreenToWorldPoint(_mousePos).ToVector2();
            var mapTile = WorldController.Pathing.GetMapTileByWorldPosition(pos);
            if (mapTile != null && (_currentMapTile == null || _currentMapTile != mapTile))
            {
                _validBuildingTilemap.ClearMap();
                _invalidBuildingTilemap.ClearMap();
                _currentMapTile = mapTile;
                _movingBuilding.transform.SetTransformPosition(_currentMapTile.World);
                var requiredTilePositions = _movingTemplate.GetRequiredPositions(_currentMapTile.Position);
                var validTiles = requiredTilePositions.Select(WorldController.Pathing.GetTileByPosition).Where(t => t != null && (!t.Block || t.Block == _movingBuilding) && (!_passiveBuildingTiles.Contains(t.Position) || _movingOriginTiles.Contains(t))).ToArray();
                var invalidTiles = requiredTilePositions.Where(t => validTiles.FirstOrDefault(v => v.Position == t) == null).ToArray();
                foreach (var tile in validTiles)
                {
                    _validBuildingTilemap.SetTile(tile.Position.x, tile.Position.y, 0);
                }

                foreach (var tile in invalidTiles)
                {
                    _invalidBuildingTilemap.SetTile(tile.x, tile.y, 0);
                }
                _validBuildingTilemap.Refresh();
                _invalidBuildingTilemap.Refresh();
                _validBuild = invalidTiles.Length <= 0;

            }
        }





        private void SubscribeToMessages()
        {
            gameObject.Subscribe<UpdateInputStateMessage>(UpdateInputState);
            gameObject.Subscribe<ClearWorldMessage>(ClearWorld);
        }

        private void UpdateInputState(UpdateInputStateMessage msg)
        {
            _mousePos = msg.Current.MousePos;
            if (WorldController.State == WorldState.World)
            {
                if (_buildingController.gameObject.activeSelf)
                {
                    UpdateBuildingTile();
                    if (!msg.Previous.LeftClick && msg.Current.LeftClick)
                    {
                        if (WorldStashController.Gold >= _buildingController.Building.Cost)
                        {
                            if (_currentMapTile != null && _validBuild)
                            {
                                WorldStashController.RemoveGold(_buildingController.Building.Cost);
                                GenerateBuildingAtPosition(_buildingController.Building, _currentMapTile);
                                _buildingController.Clear();
                                _currentMapTile = null;
                                _validBuild = false;
                                _invalidBuildingTilemap.ClearMap();
                                _validBuildingTilemap.ClearMap();

                                gameObject.SendMessage(WorldBuildingStoppedMessage.INSTANCE);
                            }
                            else
                            {
                                UiOverlayTextManager.ShowOverlayAlert("Invalid building location", ColorFactoryController.ErrorAlertText);
                            }
                        }
                        else
                        {
                            UiOverlayTextManager.ShowOverlayAlert("Not Enough Gold", ColorFactoryController.ErrorAlertText);
                            _buildingController.Clear();
                            _currentMapTile = null;
                            _validBuild = false;
                            _invalidBuildingTilemap.ClearMap();
                            _validBuildingTilemap.ClearMap();
                            gameObject.SendMessage(WorldBuildingStoppedMessage.INSTANCE);
                        }
                    }
                    else if (!msg.Previous.RightClick && msg.Current.RightClick)
                    {
                        _buildingController.Clear();
                        _currentMapTile = null;
                        _validBuild = false;
                        _invalidBuildingTilemap.ClearMap();
                        _validBuildingTilemap.ClearMap();
                        gameObject.SendMessage(WorldBuildingStoppedMessage.INSTANCE);
                    }
                }
                else if (_movingBuilding)
                {
                    UpdateMovingBuildingTiles();
                    if (!msg.Previous.LeftClick && msg.Current.LeftClick)
                    {
                        if (_currentMapTile != null && _validBuild)
                        {
                            //GenerateBuildingAtPosition(_buildingController.Building, _currentMapTile);
                            //_buildingController.Clear();
                            _movingBuilding.transform.SetTransformPosition(_currentMapTile.World);
                            var setMapTileMsg = MessageFactory.GenerateSetMapTileMsg();
                            setMapTileMsg.Tile = _currentMapTile;
                            gameObject.SendMessageTo(setMapTileMsg, _movingBuilding);
                            MessageFactory.CacheMessage(setMapTileMsg);

                            var setSpriteAlphaMsg = MessageFactory.GenerateSetSpriteAlphaMsg();
                            setSpriteAlphaMsg.Alpha = 1f;
                            _instance.gameObject.SendMessageTo(setSpriteAlphaMsg, _movingBuilding);
                            MessageFactory.CacheMessage(setSpriteAlphaMsg);

                            _movingOriginTiles = new MapTile[0];
                            _movingBuilding = null;

                            _currentMapTile = null;
                            _validBuild = false;
                            _invalidBuildingTilemap.ClearMap();
                            _validBuildingTilemap.ClearMap();

                            gameObject.SendMessage(WorldBuildingStoppedMessage.INSTANCE);
                        }
                        else
                        {
                            UiOverlayTextManager.ShowOverlayAlert("Invalid building location", ColorFactoryController.ErrorAlertText);
                        }
                    }
                    else if (!msg.Previous.RightClick && msg.Current.RightClick)
                    {
                        _instance.gameObject.SendMessageTo(ResetPositionMessage.INSTANCE, _movingBuilding);

                        var setSpriteAlphaMsg = MessageFactory.GenerateSetSpriteAlphaMsg();
                        setSpriteAlphaMsg.Alpha = 1f;
                        _instance.gameObject.SendMessageTo(setSpriteAlphaMsg, _movingBuilding);
                        MessageFactory.CacheMessage(setSpriteAlphaMsg);

                        _movingBuilding = null;
                        _movingOriginTiles = new MapTile[0];
                        _currentMapTile = null;
                        _validBuild = false;
                        _invalidBuildingTilemap.ClearMap();
                        _validBuildingTilemap.ClearMap();
                        gameObject.SendMessage(WorldBuildingStoppedMessage.INSTANCE);
                    }
                }
            }
        }

        private void ClearWorld(ClearWorldMessage msg)
        {
            Clear();
        }

        void OnDestroy()
        {
            gameObject.UnsubscribeFromAllMessages();
        }
    }
}