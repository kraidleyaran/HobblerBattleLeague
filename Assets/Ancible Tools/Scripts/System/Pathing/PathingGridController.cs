using System.Collections.Generic;
using System.Linq;
using CreativeSpore.SuperTilemapEditor;
using RogueSharp;
using UnityEngine;
using Path = DG.Tweening.Plugins.Core.PathCore.Path;

namespace Assets.Resources.Ancible_Tools.Scripts.System.Pathing
{
    public class PathingGridController : MonoBehaviour
    {
        public Vector2Int Size { get; private set; } = Vector2Int.zero;
        public Vector2Int Offset => _offset;
        public Vector2 CellSize => _tilemap.CellSize;
        public STETilemap Tilemap => _tilemap;

        [SerializeField] private STETilemap _tilemap;

        private Vector2Int _offset = Vector2Int.zero;

        private Map _pathingMap = null;

        private Dictionary<Vector2Int, MapTile> _tiles = new Dictionary<Vector2Int, MapTile>();

        public void Setup()
        {
            _pathingMap = new Map(_tilemap.GridWidth, _tilemap.GridHeight);
            Size = new Vector2Int(_tilemap.GridWidth, _tilemap.GridHeight);
            _offset = new Vector2Int(_tilemap.MinGridX, _tilemap.MinGridY);
            if (_offset.x < 0)
            {
                _offset.x *= -1;
            }

            if (_offset.y < 0)
            {
                _offset.y *= -1;
            }

            for (var x = _tilemap.MinGridX; x <= _tilemap.MaxGridX; x++)
            {
                for (var y = _tilemap.MinGridY; y <= _tilemap.MaxGridY; y++)
                {
                    if (_tilemap.GetTileData(x, y) == 0)
                    {
                        var cell = _pathingMap.GetCell(_offset.x + x, _offset.y + y);
                        _pathingMap.SetCellProperties(cell.X, cell.Y, true, true, true);
                        var pos = new Vector2Int(x, y);
                        _tiles.Add(pos, new MapTile
                        {
                            Position = pos,
                            Cell = cell,
                            World = TilemapUtils.GetGridWorldPos(_tilemap, x, y).ToVector2().ToPixelPerfect()
                        });
                    }
                }
            }
        }

        public void Setup(MapTile[] mapTiles, Vector2Int size, Vector2Int offset)
        {
            _offset = offset;
            _pathingMap = new Map(size.x, size.y);
            Size = size;
            for (var i = 0; i < mapTiles.Length; i++)
            {
                if (!_tiles.ContainsKey(mapTiles[i].Position))
                {
                    var tile = mapTiles[i];
                    tile.Cell = _pathingMap.GetCell(_offset.x + tile.Position.x, _offset.y + tile.Position.y);
                    _pathingMap.SetCellProperties(tile.Cell.X, tile.Cell.Y, true, true, true);
                    _tiles.Add(mapTiles[i].Position, tile);
                    _tilemap.SetTile(tile.Position.x, tile.Position.y, 0);
                }
            }
            _tilemap.Refresh(true,true,true,true);
        }



        public MapTile GetTileByPosition(Vector2Int pos)
        {
            if (_tiles.TryGetValue(pos, out var tile))
            {
                return tile;
            }

            return null;
        }

        public MapTile[] GetMapTilesInArea(Vector2Int pos, int area)
        {
            if (_tiles.TryGetValue(pos, out var tile))
            {
                return _pathingMap.GetCellsInSquare(tile.Cell.X, tile.Cell.Y, area).Select(GetTileFromCell).Where(t => t != null).ToArray();
            }
            return new MapTile[0];
        }

        public MapTile GetTileFromCell(ICell cell)
        {
            var pos = new Vector2Int(cell.X - _offset.x, cell.Y - _offset.y);
            return GetTileByPosition(pos);
        }

        public MapTile[] GetPath(Vector2Int start, Vector2Int end, bool diagnoal = true)
        {
            if (_tiles.TryGetValue(start, out var startTile) && _tiles.TryGetValue(end, out var endTile))
            {
                var min = new Vector2Int(Mathf.Min(startTile.Cell.X, endTile.Cell.X), Mathf.Min(startTile.Cell.Y, endTile.Cell.Y));
                min.x = Mathf.Max(0, min.x - 2);
                min.y = Mathf.Max(0, min.y - 2);
                var max = new Vector2Int(Mathf.Max(startTile.Cell.X, endTile.Cell.X), Mathf.Max(startTile.Cell.Y, endTile.Cell.Y)) + Vector2Int.one;

                var size = (max - min) + Vector2Int.one;
                var pathMap = new Map(size.x, size.y);

                var pos = min;
                var offset = min;
                var startCellPos = new Vector2Int(startTile.Cell.X, startTile.Cell.Y);
                while (pos.y <= max.y)
                {
                    if (pos.x < _pathingMap.Width && pos.y < _pathingMap.Height)
                    {
                        var originCell = _pathingMap.GetCell(pos.x, pos.y);
                        if (originCell != null)
                        {
                            var walkable = originCell.IsWalkable || pos == startCellPos;
                            var cellOffset = pos - offset;
                            pathMap.SetCellProperties(cellOffset.x, cellOffset.y, originCell.IsTransparent, walkable);
                        }
                    }

                    pos.x++;
                    if (pos.x >= max.x)
                    {
                        pos.x = min.x;
                        pos.y++;
                    }
                }

                var pathFinder = diagnoal ? new PathFinder(pathMap, 1) : new PathFinder(pathMap);
                var startOffset =  pathMap.GetCell(startTile.Cell.X - offset.x, startTile.Cell.Y - offset.y);
                var endOffset = pathMap.GetCell(endTile.Cell.X - offset.x, endTile.Cell.Y - offset.y);
                var attempt = pathFinder.TryFindShortestPath(startOffset, endOffset);
                if (attempt != null)
                {
                    var path = attempt.Steps.Select(c => _pathingMap.GetCell(c.X + offset.x, c.Y + offset.y)).Select(GetTileFromCell).Where(t => t != null).ToList();
                    if (path.Count > 0 && path[0] == startTile)
                    {
                        path.Remove(startTile);
                    }
                    return path.ToArray();
                }
                return new MapTile[0];
            }

            return new MapTile[0];
        }

        public MapTile GetMapTileByWorldPosition(Vector2 worldPos)
        {
            var pos = TilemapUtils.GetGridPositionInt(_tilemap, _tilemap.transform.InverseTransformPoint(worldPos));
            return GetTileByPosition(pos);
        }

        public void SetTileBlock(GameObject obj, Vector2Int pos, bool blockPathing = true)
        {
            if (_tiles.TryGetValue(pos, out var tile) && !tile.Block)
            {
                tile.Block = obj;
                if (blockPathing)
                {
                    _pathingMap.SetCellProperties(tile.Cell.X, tile.Cell.Y, true, false, true);
                }
            }
        }

        public void RemoveTileBlock(GameObject obj, Vector2Int pos, bool unblockPathing = true)
        {
            if (_tiles.TryGetValue(pos, out var tile) && tile.Block && tile.Block == obj)
            {
                tile.Block = null;
                if (unblockPathing)
                {
                    _pathingMap.SetCellProperties(tile.Cell.X, tile.Cell.Y, true, true, true);
                }
                
            }
        }

        public Vector2 GetWorldPositionFromPos(Vector2Int pos)
        {
            if (_tiles.TryGetValue(pos, out var tile))
            {
                return tile.World;
            }
            return TilemapUtils.GetGridWorldPos(_tilemap, pos.x, pos.y).ToVector2().ToPixelPerfect();
        }

        public MapTile AddMapTile(Vector2Int pos)
        {
            if (!_tiles.ContainsKey(pos))
            {

                var cell = _pathingMap.GetCell(pos.x + _offset.x, pos.y + _offset.y);
                if (cell != null)
                {
                    _pathingMap.SetCellProperties(cell.X, cell.Y, true, true, true);
                    var mapTile = new MapTile
                    {
                        Position = pos,
                        World = TilemapUtils.GetGridWorldPos(_tilemap, pos.x, pos.y).ToVector2().ToPixelPerfect(),
                        Cell = cell,
                    };
                    _tiles.Add(pos, mapTile);
                    _tilemap.SetTile(pos.x, pos.y, 0);
                    _tilemap.Refresh(true,true,true,true);
                    return mapTile;
                }
            }

            return null;
        }

        public MapTile[] GetTilesInPov(Vector2Int position, int area)
        {
            if (_tiles.TryGetValue(position, out var origin))
            {
                return _pathingMap.ComputeFov(origin.Cell.X, origin.Cell.Y, area, false).Select(GetTileFromCell).Where(t => t != null && t != origin).ToArray();
            }
            return new MapTile[0];
        }

        public MapTile[] GetAllMapTiles()
        {
            return _tiles.Values.ToArray();
        }

        public Vector2 GetWorldPosition(Vector2Int worldPos)
        {
            return TilemapUtils.GetGridWorldPos(_tilemap, worldPos.x, worldPos.y).ToVector2();
        }

        public void Clear()
        {
            var tiles = _tiles.ToArray();
            foreach (var tile in tiles)
            {
                if (tile.Value.Block)
                {
                    var cell = tile.Value.Cell;
                    _pathingMap.SetCellProperties(cell.X, cell.Y, true, true, true);
                    tile.Value.Block = null;
                }
            }
        }

        void Destroy()
        {
            var tiles = _tiles.Values.ToArray();
            for (var i = 0; i < tiles.Length; i++)
            {
                tiles[i].Destroy();
            }

            _tiles.Clear();
            _tiles = null;
        }
    }
}