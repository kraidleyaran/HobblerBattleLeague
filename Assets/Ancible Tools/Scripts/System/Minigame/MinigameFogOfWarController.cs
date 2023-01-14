using System.Collections.Generic;
using System.Linq;
using Assets.Resources.Ancible_Tools.Scripts.System.Pathing;
using CreativeSpore.SuperTilemapEditor;
using MessageBusLib;
using RogueSharp;
using UnityEngine;

namespace Assets.Resources.Ancible_Tools.Scripts.System.Minigame
{
    public class MinigameFogOfWarController : MonoBehaviour
    {
        private static MinigameFogOfWarController _instance = null;

        [SerializeField] private STETilemap _unexploredTilemap;
        [SerializeField] private STETilemap _fogTilemap;

        private Map _fogOfWarMap = null;
        private Dictionary<Vector2Int, MapTile> _exploredTiles = new Dictionary<Vector2Int, MapTile>();
        private List<MapTile> _currentPlayerPov = new List<MapTile>();
        private Dictionary<Vector2Int, MapTile> _mapTiles = new Dictionary<Vector2Int, MapTile>();
        private List<Vector2Int> _visionBlocks = new List<Vector2Int>();
        private Vector2Int _offset = Vector2Int.zero;
        private ICell[] _light = new ICell[0];
        private MapTile _currentTile = null;
        private int _visionArea = 1;

        public void Setup(MapTile[] tiles, Vector2Int size, Vector2Int offset, Vector2Int minFog, Vector2Int maxFog)
        {
            if (_instance)
            {
                Destroy(_instance.gameObject);
            }

            _instance = this;
            _offset = offset;
            _fogOfWarMap = new Map(size.x, size.y);
            _mapTiles = tiles.ToDictionary(t => t.Position, t => t);
            for (var i = 0; i < tiles.Length; i++)
            {
                var tile = tiles[i];
                _fogOfWarMap.SetCellProperties(tile.Cell.X, tile.Cell.Y, true, true, false);
            }

            var pos = minFog;
            while (pos.y <= maxFog.y)
            {
                _unexploredTilemap.SetTile(pos.x, pos.y, 0);
                _fogTilemap.SetTile(pos.x, pos.y, 0);
                pos.x++;
                if (pos.x > maxFog.x)
                {
                    pos.x = minFog.x;
                    pos.y++;
                }
            }
            _unexploredTilemap.Refresh(true, true, true, true);
        }

        public static MapTile[] SetPlayerPov(MapTile tile, int visionArea)
        {
            var returnTiles = new MapTile[0];
            if (_instance)
            {
                _instance._currentTile = tile;
                _instance._visionArea = visionArea;
                //var oldPov = _instance._currentPlayerPov.ToArray();
                var prevLight = _instance._light.ToList();
                _instance._light = _instance._fogOfWarMap.ComputeFov(tile.Cell.X, tile.Cell.Y, visionArea, true).ToArray();
                var exploreTiles = _instance._light.Select(GetMapTileByCell).Where(t => t != null).ToArray();
                for (var i = 0; i < exploreTiles.Length; i++)
                {
                    if (!_instance._exploredTiles.ContainsKey(exploreTiles[i].Position))
                    {
                        _instance._exploredTiles.Add(exploreTiles[i].Position, exploreTiles[i]);
                        var block = _instance._visionBlocks.Contains(exploreTiles[i].Position);
                        _instance._fogOfWarMap.SetCellProperties(exploreTiles[i].Cell.X, exploreTiles[i].Cell.Y, !block, !block, true);
                        _instance._unexploredTilemap.SetTileData(exploreTiles[i].Position.x, exploreTiles[i].Position.y, Tileset.k_TileData_Empty);
                    }
                }


                _instance._currentPlayerPov = exploreTiles.ToList();
                
                returnTiles = _instance._currentPlayerPov.ToArray();

                var clearCells = prevLight.Where(c => !_instance._light.Contains(c)).ToArray();
                for (var i = 0; i < clearCells.Length; i++)
                {
                    var cell = clearCells[i];
                    var fogTile = new Vector2Int(cell.X - _instance._offset.x, cell.Y - _instance._offset.y);
                    _instance._fogTilemap.SetTile(fogTile.x, fogTile.y, 0);
                }
                
                for (var i = 0; i < _instance._light.Length; i++)
                {
                    var cell = _instance._light[i];
                    var fogTile = new Vector2Int(cell.X - _instance._offset.x, cell.Y - _instance._offset.y);
                    _instance._fogTilemap.SetTileData(fogTile.x, fogTile.y, Tileset.k_TileData_Empty);
                    _instance._unexploredTilemap.SetTileData(fogTile.x, fogTile.y, Tileset.k_TileData_Empty);
                }

                _instance._fogTilemap.Refresh(true, true, true,true);
                _instance._unexploredTilemap.Refresh(true, true, true, true);
                _instance.gameObject.SendMessage(MinigamePlayerPovUpdatedMessage.INSTANCE);
            }

            return returnTiles;
        }

        public static void SetBlockOnTile(Vector2Int pos)
        {
            if (_instance)
            {
                if (_instance._mapTiles.TryGetValue(pos, out var mapTile) && !_instance._visionBlocks.Contains(pos))
                {
                    _instance._visionBlocks.Add(pos);
                    _instance._fogOfWarMap.SetCellProperties(mapTile.Cell.X, mapTile.Cell.Y, false, false, _instance._exploredTiles.ContainsKey(pos));
                    if (_instance._currentTile != null)
                    {
                        SetPlayerPov(_instance._currentTile, _instance._visionArea);
                    }
                }
            }
        }

        public static void RemoveBlockOnTile(Vector2Int pos)
        {
            if (_instance)
            {
                if (_instance._mapTiles.TryGetValue(pos, out var mapTile))
                {
                    _instance._visionBlocks.Remove(pos);
                    _instance._fogOfWarMap.SetCellProperties(mapTile.Cell.X, mapTile.Cell.Y, true, true, _instance._exploredTiles.ContainsKey(pos));
                    if (_instance._currentTile != null)
                    {
                        SetPlayerPov(_instance._currentTile, _instance._visionArea);
                    }
                }
            }
        }

        public static bool IsVisible(MapTile tile)
        {
            if (_instance)
            {
                return _instance._currentPlayerPov.Contains(tile);
            }

            return true;
        }

        private static MapTile GetMapTileByCell(ICell cell)
        {

            if (_instance)
            {
                var pos = new Vector2Int(cell.X - _instance._offset.x, cell.Y - _instance._offset.y);
                if (_instance._mapTiles.TryGetValue(pos, out var mapTile))
                {
                    return mapTile;
                }

                return null;
            }

            return null;

        }


        public void Destroy()
        {
            if (_instance && _instance == this)
            {
                _instance = null;
            }
        }
    }
}