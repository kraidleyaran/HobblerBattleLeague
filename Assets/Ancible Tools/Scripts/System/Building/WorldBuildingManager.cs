using System.Collections.Generic;
using System.Linq;
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

        private Vector2 _mousePos = Vector2.zero;
        private MapTile _currentMapTile = null;
        private bool _validBuild = false;
        private List<WorldBuilding> _availableBuildings = new List<WorldBuilding>();

        private SetupBuildingMessage _setupBuildingMsg = new SetupBuildingMessage();

        private List<GameObject> _currentBuildings = new List<GameObject>();
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

        private void SubscribeToMessages()
        {
            gameObject.Subscribe<UpdateInputStateMessage>(UpdateInputState);
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
                        var template = _buildingController.Building.Template;
                        var buildingUnit = template.GenerateUnit(transform, _currentMapTile.World);
                        _setupBuildingMsg.Building = _buildingController.Building;
                        gameObject.SendMessageTo(_setupBuildingMsg, buildingUnit.gameObject);

                        var setMapTileMsg = MessageFactory.GenerateSetMapTileMsg();
                        setMapTileMsg.Tile = _currentMapTile;
                        gameObject.SendMessageTo(setMapTileMsg, buildingUnit.gameObject);
                        MessageFactory.CacheMessage(setMapTileMsg);

                        _currentBuildings.Add(buildingUnit.gameObject);
                        var buildTiles = _buildingController.Building.GetBlockingPositions(_currentMapTile.Position);
                        var passiveTiles = _buildingController.Building.GetRequiredPositions(_currentMapTile.Position).Where(t => !buildTiles.Contains(t)).ToList();
                        
                        if (!_currentMapTile.Block)
                        {
                            passiveTiles.Add(_currentMapTile.Position);
                        }

                        for (var i = 0; i < passiveTiles.Count; i++)
                        {
                            if (!_passiveBuildingTiles.Contains(passiveTiles[i]))
                            {
                                _passiveBuildingTiles.Add(passiveTiles[i]);
                            }
                        }
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
    }
}