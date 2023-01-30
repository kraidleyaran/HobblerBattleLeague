using System;
using System.Collections.Generic;
using System.Linq;
using Assets.Ancible_Tools.Scripts.System.SaveData.Buildings;
using Assets.Resources.Ancible_Tools.Scripts.System.Pathing;
using Assets.Resources.Ancible_Tools.Scripts.System.Templates;
using CreativeSpore.SuperTilemapEditor;
using MessageBusLib;
using UnityEngine;

namespace Assets.Resources.Ancible_Tools.Scripts.System.Building
{
    public class WorldBuildingManager : MonoBehaviour
    {
        public static WorldBuilding[] Available => _instance._availableBuildings.ToArray();
        public static bool Active => _instance._buildingController.gameObject.activeSelf;

        private static WorldBuildingManager _instance = null;

        [SerializeField] private BuildingTemplateController _buildingController = null;
        [SerializeField] private STETilemap _validBuildingTilemap;
        [SerializeField] private STETilemap _invalidBuildingTilemap;
        [SerializeField] private WorldBuilding[] _startingBuildings = new WorldBuilding[0];
        [SerializeField] private string _buildingFolder = string.Empty;

        private Vector2 _mousePos = Vector2.zero;
        private MapTile _currentMapTile = null;
        private bool _validBuild = false;
        private List<WorldBuilding> _availableBuildings = new List<WorldBuilding>();

        private SetupBuildingMessage _setupBuildingMsg = new SetupBuildingMessage();

        private Dictionary<string, WorldBuilding> _allBuildings = new Dictionary<string, WorldBuilding>();
        

        private Dictionary<BuildingData, GameObject> _currentBuildings = new Dictionary<BuildingData, GameObject>();
        private List<Vector2Int> _passiveBuildingTiles = new List<Vector2Int>();

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
            foreach (var building in buildings)
            {
                queryMapTileMsg.DoAfter = tile => building.Key.Position = tile.Position.ToData();
                _instance.gameObject.SendMessageTo(queryMapTileMsg, building.Value);
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
            setMapTileMsg.Tile = tile ;
            _instance.gameObject.SendMessageTo(setMapTileMsg, buildingUnit.gameObject);
            MessageFactory.CacheMessage(setMapTileMsg);

            _instance._currentBuildings.Add(new BuildingData { Building = building.name, Id = id, Position = tile.Position.ToData() }, buildingUnit.gameObject);
            var buildTiles = building.GetBlockingPositions(tile.Position);
            var passiveTiles = building.GetRequiredPositions(tile.Position).Where(t => !buildTiles.Contains(t)).ToList();

            if (!tile.Block)
            {
                passiveTiles.Add(tile.Position);
            }

            for (var i = 0; i < passiveTiles.Count; i++)
            {
                if (!_instance._passiveBuildingTiles.Contains(passiveTiles[i]))
                {
                    _instance._passiveBuildingTiles.Add(passiveTiles[i]);
                }
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
            if (_buildingController.gameObject.activeSelf && WorldController.State == WorldState.World)
            {
                UpdateBuildingTile();
                if (!msg.Previous.LeftClick && msg.Current.LeftClick)
                {
                    if (_currentMapTile != null && _validBuild)
                    {
                        GenerateBuildingAtPosition(_buildingController.Building, _currentMapTile);
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