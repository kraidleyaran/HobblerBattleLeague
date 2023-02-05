using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Assets.Ancible_Tools.Scripts.Traits;
using Assets.Resources.Ancible_Tools.Scripts.System.Abilities;
using Assets.Resources.Ancible_Tools.Scripts.System.Combat;
using Assets.Resources.Ancible_Tools.Scripts.System.Items;
using Assets.Resources.Ancible_Tools.Scripts.System.Minigame;
using Assets.Resources.Ancible_Tools.Scripts.System.Pathing;
using Assets.Resources.Ancible_Tools.Scripts.System.Templates;
using Assets.Resources.Ancible_Tools.Scripts.System.UI.MInigame;
using CreativeSpore.SuperTilemapEditor;
using MessageBusLib;
using ProceduralToolkit;
using RogueSharp;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Assets.Resources.Ancible_Tools.Scripts.System.Maze
{
    public class WorldMazeController : MonoBehaviour
    {
        [SerializeField] private STETilemap _tilemap;
        [SerializeField] private STETilemap _terrain;
        [SerializeField] private uint _deadEndId = 1;
        [SerializeField] private uint _possibleEndId = 0;
        [SerializeField] private int _mazeSteps = 100;
        [SerializeField] private PathingGridController _pathingGrid = null;
        [SerializeField] private MinigameFogOfWarController _fogofWar = null;
        [SerializeField] private UnitTemplate _playerTemplate = null;
        [SerializeField] private SpriteTrait _defaultPlayerSprite = null;
        
        

        private ProceduralToolkit.Samples.Maze _maze = null;
        private Dictionary<Vector2Int, MapTile> _tiles = new Dictionary<Vector2Int, MapTile>();
        private List<Vector2Int> _spawnableTiles = new List<Vector2Int>();
        private List<MazeDoor> _possibleEnds = new List<MazeDoor>();
        private MazeRoom[] _rooms = new MazeRoom[0];
        private Vector2Int _playerSpawn = Vector2Int.zero;

        private Vector2Int _offset = Vector2Int.zero;
        private MazeMinigameSettings _settings = null;
        

        public void Setup(MazeMinigameSettings settings, GameObject proxy)
        {
            _settings = settings;
            _maze = new ProceduralToolkit.Samples.Maze(settings.Size.x, settings.Size.y);
            var startPoint = new Vector2Int(Random.Range(0, _settings.Size.x), Random.Range(0, _settings.Size.y));
            var edges = _maze.GetPossibleConnections(new ProceduralToolkit.Samples.Maze.Vertex(startPoint, Directions.None, 0));
            _tilemap.Tileset = _settings.Ground;
            _terrain.Tileset = _settings.Terrain;
            Debug.Log($"Player Spawn: {_playerSpawn}");
            var draw = new List<ProceduralToolkit.Samples.Maze.Edge>();
            while (Generate(edges, draw, _mazeSteps))
            {
            }
            var doors = new Dictionary<Vector2Int, MazeDoor>();
            var deadEnds = new List<Vector2Int>();
            var existingTiles = new List<Vector2Int>();
            _rooms = draw.Select(e => GenerateRoom(e, deadEnds, doors, existingTiles)).ToArray();
            DrawRooms(_rooms);
            var rooms = DrawWalls(deadEnds, doors);
            var chests = SpawnChests(_settings, rooms);
            var monsters = SpawnMonsters(_settings, rooms);
            var playerSpawnRoom = _rooms.FirstOrDefault(r => r.RoomId == startPoint);
            if (playerSpawnRoom != null && playerSpawnRoom.SpawnableTiles.Count > 0)
            {
                _playerSpawn = playerSpawnRoom.SpawnableTiles.GetRandom();
            }
            if (_tiles.TryGetValue(_playerSpawn, out var spawnTile))
            {

                var unitController = _playerTemplate.GenerateUnit(transform, spawnTile.World);
                //unitController.transform.SetLocalPosition(spawnTile.World);
                unitController.transform.gameObject.layer = transform.gameObject.layer;

                var addTraitToUnitMsg = MessageFactory.GenerateAddTraitToUnitMsg();
                //for (var i = 0; i < _playerTraits.Length; i++)
                //{
                //    addTraitToUnitMsg.Trait = _playerTraits[i];
                //    gameObject.SendMessageTo(addTraitToUnitMsg, unitController.gameObject);
                //}
                

                var setMapTileMsg = MessageFactory.GenerateSetMapTileMsg();
                setMapTileMsg.Tile = spawnTile;
                gameObject.SendMessageTo(setMapTileMsg, unitController.gameObject);
                MessageFactory.CacheMessage(setMapTileMsg);

                if (proxy)
                {
                    CombatStats stats = CombatStats.Zero;
                    var queryCombatStatsMsg = MessageFactory.GenerateQueryCombatStatsMsg();
                    queryCombatStatsMsg.DoAfter = (baseStats, bonusStats, genetics) =>
                    {
                        stats += baseStats + bonusStats + genetics;
                    };
                    gameObject.SendMessageTo(queryCombatStatsMsg, proxy);
                    MessageFactory.CacheMessage(queryCombatStatsMsg);

                    var setCombatStatsMsg = MessageFactory.GenerateSetCombatStatsMsg();
                    setCombatStatsMsg.Stats = stats;
                    setCombatStatsMsg.Accumulated = GeneticCombatStats.Zero;
                    gameObject.SendMessageTo(setCombatStatsMsg, unitController.gameObject);
                    MessageFactory.CacheMessage(setCombatStatsMsg);

                    SpriteTrait sprite = null;
                    var querySpriteMsg = MessageFactory.GenerateQuerySpriteMsg();
                    querySpriteMsg.DoAfter = trait => sprite = trait;
                    gameObject.SendMessageTo(querySpriteMsg, proxy);
                    MessageFactory.CacheMessage(querySpriteMsg);
                    if (sprite)
                    {
                        addTraitToUnitMsg.Trait = sprite;
                        gameObject.SendMessageTo(addTraitToUnitMsg, unitController.gameObject);
                    }

                    BasicAttackSetup attackSetup = null;
                    var queryBasicAttackMsg = MessageFactory.GenerateQueryBasicAttackSetupMsg();
                    queryBasicAttackMsg.DoAfter = attack => attackSetup = attack;
                    gameObject.SendMessageTo(queryBasicAttackMsg, proxy);
                    MessageFactory.CacheMessage(queryBasicAttackMsg);

                    if (attackSetup != null)
                    {
                        var setBasicAttackMsg = MessageFactory.GenerateSetBasicAttackSetupMsg();
                        setBasicAttackMsg.Setup = attackSetup;
                        gameObject.SendMessageTo(setBasicAttackMsg, unitController.gameObject);
                        MessageFactory.CacheMessage(setBasicAttackMsg);
                    }



                    var armor = new EquippableInstance[0];
                    var trinkets = new EquippableInstance[0];
                    EquippableInstance weapon = null;
                    var queryHobbleryEquipmentMsg = MessageFactory.GenerateQueryHobblerEquipmentMsg();
                    queryHobbleryEquipmentMsg.DoAfter = (equippedArmor, equippedTrinkets, equippedWeapon) =>
                    {
                        armor = equippedArmor;
                        trinkets = equippedTrinkets;
                        weapon = equippedWeapon;
                    };
                    gameObject.SendMessageTo(queryHobbleryEquipmentMsg, proxy);
                    MessageFactory.CacheMessage(queryHobbleryEquipmentMsg);

                    var equipment = new List<EquippableInstance>();
                    equipment.AddRange(armor.Where(a => a != null));
                    equipment.AddRange(trinkets.Where(t => t != null));
                    if (weapon != null)
                    {
                        equipment.Add(weapon);
                    }
                    var setMinigameEquipmentMsg = MessageFactory.GenerateSetMinigameEquipmentMsg();
                    setMinigameEquipmentMsg.Equipment = equipment.ToArray();
                    gameObject.SendMessageTo(setMinigameEquipmentMsg, unitController.gameObject);
                    MessageFactory.CacheMessage(setMinigameEquipmentMsg);

                    var abilities = new KeyValuePair<int, WorldAbility>[0];
                    var queryAbilitiesMsg = MessageFactory.GenerateQueryAbilitiesMsg();
                    queryAbilitiesMsg.DoAfter = hobblerAbilities => abilities = hobblerAbilities;
                    gameObject.SendMessageTo(queryAbilitiesMsg, proxy);
                    MessageFactory.CacheMessage(queryAbilitiesMsg);

                    var availableAbilities = abilities.Select(kv => kv.Value).ToArray();
                    var setAbilitiesMsg = MessageFactory.GenerateSetAbilitiesMsg();
                    setAbilitiesMsg.Abilities = availableAbilities;
                    gameObject.SendMessageTo(setAbilitiesMsg, unitController.gameObject);
                    MessageFactory.CacheMessage(setAbilitiesMsg);


                }
                else
                {
                    addTraitToUnitMsg.Trait = _defaultPlayerSprite;
                    gameObject.SendMessageTo(addTraitToUnitMsg, unitController.gameObject);
                }

                var setProxyHobblerMsg = MessageFactory.GenerateSetProxyHobblerMsg();
                setProxyHobblerMsg.Hobbler = proxy;
                setProxyHobblerMsg.MaxMonsters = monsters;
                setProxyHobblerMsg.MaxChests = chests;
                gameObject.SendMessageTo(setProxyHobblerMsg, unitController.gameObject);
                MessageFactory.CacheMessage(setProxyHobblerMsg);

                MessageFactory.CacheMessage(addTraitToUnitMsg);
                unitController.gameObject.name = "Player";

                MinigameController.CullingCheck();
                UiMazeActionBarController.SetPlayer(unitController.gameObject);
            }
            else
            {
                Debug.LogWarning($"Tile does not exist:{_playerSpawn}");
            }
        }

        private bool Generate(List<ProceduralToolkit.Samples.Maze.Edge> edges, List<ProceduralToolkit.Samples.Maze.Edge> draw, int steps = 0)
        {
            var changed = false;
            for (var i = 0; edges.Count > 0 && (steps == 0 || i < steps); i++)
            {
                var edge = edges.Count > 1 ? edges[Random.Range(0, edges.Count)] : edges[0];
                edges.Remove(edge);
                if (_maze.IsUnconnected(edge.exit.position))
                {
                    _maze.AddEdge(edge);
                    edges.AddRange(_maze.GetPossibleConnections(edge.exit));
                    draw.Add(edge);
                }

                changed = true;
            }

            return changed;
        }

        private MazeRoom GenerateRoom(ProceduralToolkit.Samples.Maze.Edge edge, List<Vector2Int> deadEnds, Dictionary<Vector2Int,MazeDoor> doors, List<Vector2Int> existingTiles)
        {
            var room = new MazeRoom{RoomId = edge.origin.position};
            var min = new Vector2Int(_settings.WallSize + Mathf.Min(edge.origin.position.x, edge.exit.position.x) * (_settings.RoomSize + _settings.WallSize), _settings.WallSize + Mathf.Min(edge.origin.position.y, edge.exit.position.y) * (_settings.RoomSize + _settings.WallSize));
            var max = new Vector2Int(_settings.WallSize + Mathf.Max(edge.origin.position.x, edge.exit.position.x) * (_settings.RoomSize + _settings.WallSize), _settings.WallSize + Mathf.Max(edge.origin.position.y, edge.exit.position.y) * (_settings.RoomSize + _settings.WallSize));
            var size = Vector2Int.zero;
            if (edge.exit.position.y - edge.origin.position.y == 0)
            {
                size.x = _settings.RoomSize * 2 + _settings.WallSize;
                size.y = _settings.RoomSize;
            }
            else
            {
                size.x = _settings.RoomSize;
                size.y = _settings.RoomSize * 2 + _settings.WallSize;
            }

            room.Min = min;
            room.Max = max;
            room.Depth = edge.origin.depth;
            var entryTile = new Vector2Int(_settings.WallSize + edge.origin.position.x * (_settings.WallSize + _settings.RoomSize), _settings.WallSize + edge.origin.position.y * (_settings.WallSize + _settings.RoomSize));
            var exitTile = new Vector2Int(_settings.WallSize + edge.exit.position.x * (_settings.WallSize + _settings.RoomSize), _settings.WallSize + edge.exit.position.y * (_settings.WallSize + _settings.RoomSize));
            if (edge.exit.position != edge.origin.position)
            {
                if (edge.exit.connections != Directions.None)
                {
                    deadEnds.Remove(exitTile);
                    if (!doors.ContainsKey(exitTile))
                    {
                        var range = new IntNumberRange { Minimum = 1, Maximum = _settings.RoomSize - 2 };
                        var space = range.Roll();
                        var doorTile = ApplyDirectionalOffset(exitTile, edge.exit.connections, space);
                        doors.Add(exitTile, new MazeDoor(doorTile, edge.exit.connections, GetDoorWalls(doorTile, space, _settings.RoomSize, edge.exit.connections.ToDoorRotation())));
                    }
                }
                //else if (!_doors.ContainsKey(exitTile))
                //{
                //    _deadEnds.Add(ApplyDirectionalOffset(edge.origin.position, edge.origin.connections, 0));
                //}

                if (edge.origin.connections != Directions.None)
                {
                    deadEnds.Remove(entryTile);

                    if (!doors.ContainsKey(entryTile))
                    {
                        var range = new IntNumberRange { Minimum = 1, Maximum = _settings.RoomSize - 2 };
                        var space = range.Roll();
                        var doorTile = ApplyDirectionalOffset(entryTile, edge.origin.connections, space);
                        doors.Add(entryTile, new MazeDoor(doorTile, edge.origin.connections, GetDoorWalls(doorTile, space, _settings.RoomSize, edge.origin.connections.ToDoorRotation())));
                    }
                }

                if (edge.origin.connections == edge.exit.connections)
                {
                    var debugMsg = string.Empty;
                    debugMsg = $"Not Added: {exitTile} - {entryTile} - {edge.exit.connections} - {edge.origin.connections}";
                    Debug.Log($"{debugMsg}");
                    if (!doors.ContainsKey(exitTile))
                    {
                        deadEnds.Add(exitTile);
                    }
                    if (!doors.ContainsKey(entryTile))
                    {
                        deadEnds.Add(entryTile);
                    }
                    //_deadEnds.Add(entryTile);
                }
            }
            else
            {
                if (edge.origin.connections != Directions.None)
                {
                    deadEnds.Remove(entryTile);

                    if (!doors.ContainsKey(entryTile))
                    {
                        var range = new IntNumberRange { Minimum = 1, Maximum = _settings.RoomSize - 2 };
                        var space = range.Roll();
                        var doorTile = ApplyDirectionalOffset(entryTile, edge.origin.connections, space);
                        doors.Add(entryTile, new MazeDoor(doorTile, edge.origin.connections, GetDoorWalls(doorTile, space, _settings.RoomSize, edge.origin.connections.ToDoorRotation())));
                    }
                }
                if (edge.exit.connections != Directions.None)
                {
                    var debugMsg = string.Empty;
                    debugMsg = $"Dead End: {exitTile} - {entryTile} - {edge.exit.connections} - {edge.origin.connections}";
                    Debug.Log($"{debugMsg}");
                    deadEnds.Add(entryTile);
                }

                //if (edge.exit.connections != Directions.None)
                //{
                //    var doorTile = ApplyDirectionalOffset(exitTile, edge.exit.connections, 0);
                //    _possibleEnds.Add(new MazeDoor(doorTile, edge.exit.connections, GetDoorWalls(doorTile, 0, _roomSize, edge.exit.connections.ToDoorRotation())));
                //}
            }
            var pos = Vector2Int.zero;
            var pathableTiles = new List<Vector2Int>();
            var spawnableTiles = new List<Vector2Int>();
            while (pos.y < size.y)
            {
                var tilePos = pos + min;
                if (!existingTiles.Contains(tilePos))
                {
                    pathableTiles.Add(tilePos);
                    spawnableTiles.Add(tilePos);
                    existingTiles.Add(tilePos);
                }
                //if (pos.y > 1 && pos.y < size.y - 2 && pos.x > 1 && pos.x < size.x - 2)
                //{
                    
                //}
                pos.x++;
                
                if (pos.x >= size.x)
                {
                    pos.x = 0;
                    pos.y++;
                }


            }

            room.PathableTiles = pathableTiles.ToArray();
            room.SpawnableTiles = spawnableTiles.ToList();
            return room;
        }

        private void DrawRooms(MazeRoom[] rooms)
        {
            var currentGroundTileIndex = 0;
            
            var orderedRooms = rooms.OrderBy(r => (r.RoomId - Vector2Int.zero).sqrMagnitude).ToArray();
            for (var i = 0; i < orderedRooms.Length; i++)
            {
                var room = orderedRooms[i];
                for (var t = 0; t < room.PathableTiles.Length; t++)
                {
                    var tile = room.PathableTiles[t];
                    _tilemap.SetTile(tile.x, tile.y, 0, _settings.GroundTiles[currentGroundTileIndex], eTileFlags.Updated);
                }
                currentGroundTileIndex++;
                if (currentGroundTileIndex >= _settings.GroundTiles.Length)
                {
                    currentGroundTileIndex = 0;
                }
            }

            //var totalExits = exits.Count * _doorPercent;
            //for (var i = 0; i < totalExits && exits.Count > 0; i++)
            //{
            //    var exit = exits.Count > 1 ? exits[Random.Range(0, exits.Count)] : exits[0];
            //    exits.Remove(exit);
            //    _terrain.SetTileData(exit.x, exit.y, _roomExitId);
            //}

            //for (var i = 0; i < deadEnds.Count; i++)
            //{
            //    _terrain.SetTileData(deadEnds[i].x, deadEnds[i].y, _deadEndId);
            //}
            //_terrain.Refresh(true,true,true,true);
            _tilemap.Refresh(true, true, true, true);
        }

        private MazeRoom[] DrawWalls(List<Vector2Int> deadEnds, Dictionary<Vector2Int, MazeDoor> doors)
        {
            var min = new Vector2Int(_tilemap.MinGridX - _settings.EdgeBuffer, _tilemap.MinGridY - _settings.EdgeBuffer);
            var max = new Vector2Int(_tilemap.MaxGridX + (_settings.EdgeBuffer + _settings.WallSize) + 1, _tilemap.MaxGridY + _settings.EdgeBuffer + _settings.WallSize + 1);
            var pos = min;
            _offset = min;
            if (_offset.x < 0)
            {
                _offset.x *= -1;
            }

            if (_offset.y < 0)
            {
                _offset.y *= -1;
            }
            _offset += Vector2Int.one;
            while (pos.y <= max.y)
            {
                var brush = _tilemap.GetBrush(pos.x, pos.y);
                if (brush == null)
                {
                    var door = doors.FirstOrDefault(kv => kv.Value.Position == pos);
                    if (door.Value != null)
                    {
                        doors.Remove(door.Key);
                        //_deadEnds.Add(door.Key);
                        //_possibleEnds.Add(door.Value);
                    }
                    _terrain.SetTile(pos.x, pos.y, 0, _settings.WallTileId, eTileFlags.Updated);
                    if (_settings.PaintGroundForWall)
                    {
                        _tilemap.SetTile(pos.x, pos.y,0, _settings.GroundTiles[0]);
                    }
                }
                else if (!_tiles.ContainsKey(pos))
                {
                    var mapTile = new MapTile
                    {
                        Position = pos,
                        World = TilemapUtils.GetGridWorldPos(_tilemap, pos.x, pos.y).ToVector2().ToPixelPerfect(),
                    };
                    _tiles.Add(pos, mapTile);
                }

                pos.x++;
                if (pos.x >= max.x + 1)
                {
                    pos.x = min.x;
                    pos.y++;
                }
            }
            _spawnableTiles = _tiles.Keys.Where(IsSpawnableTile).ToList();
            var doorCount = doors.Count * _settings.DoorPerecent;
            var drawDoors = doors.Values.ToList();
            var doorPositions = new List<Vector2Int>();
            for (var i = 0; i < doorCount && i < drawDoors.Count; i++)
            {
                var door = drawDoors.Count > 1 ? drawDoors[Random.Range(0, drawDoors.Count)] : drawDoors[0];
                drawDoors.Remove(door);
                doorPositions.AddRange(door.Walls);
                MinigameUnitSpawnController spawnController = null;
                switch (door.Rotation.ToDoorRotation())
                {
                    case DoorRotation.Horizontal:
                        spawnController = _settings.HorizontalDoorSpawn;
                        break;
                    case DoorRotation.Vertical:
                        spawnController = _settings.VerticalDoorSpawn;
                        break;
                }

                if (_tiles.TryGetValue(door.Position, out var tile))
                {
                    var doorUnit = Instantiate(spawnController, MinigameController.Transform);
                    doorUnit.transform.SetTransformPosition(tile.World);
                    switch (door.Rotation.ToDoorRotation())
                    {
                        case DoorRotation.Horizontal:
                            _spawnableTiles.Remove(door.Position + Vector2Int.left);
                            _spawnableTiles.Remove(door.Position + Vector2Int.right);

                            break;
                        case DoorRotation.Vertical:
                            _spawnableTiles.Remove(door.Position + Vector2Int.up);
                            _spawnableTiles.Remove(door.Position + Vector2Int.down);
                            break;
                    }
                    _spawnableTiles.Remove(door.Position + Vector2Int.up + Vector2Int.left);
                    _spawnableTiles.Remove(door.Position + Vector2Int.up + Vector2Int.right);
                    _spawnableTiles.Remove(door.Position + Vector2Int.down + Vector2Int.left);
                    _spawnableTiles.Remove(door.Position + Vector2Int.down + Vector2Int.right);
                }

            }

            for (var i = 0; i < doorPositions.Count; i++)
            {
                var wallPos = doorPositions[i];
                _tiles.Remove(wallPos);
                _spawnableTiles.Remove(wallPos);
                _spawnableTiles.Remove(wallPos + Vector2Int.up);
                _spawnableTiles.Remove(wallPos + Vector2Int.down);
                _spawnableTiles.Remove(wallPos + Vector2Int.left);
                _spawnableTiles.Remove(wallPos + Vector2Int.right);

                _tilemap.SetTile(wallPos.x, wallPos.y, 0, _settings.WallTileId, eTileFlags.Updated);
                
            }
            
            _pathingGrid.Setup(_tiles.Values.ToArray(), (max - min) + Vector2Int.one, _offset);
            for (var i = 0; i < drawDoors.Count; i++)
            {
                doors.Remove(drawDoors[i].Position);
            }

            var playerQuadrant = GetQuadrant(_playerSpawn);
            
            _possibleEnds = _tiles.Keys.Where(t => IsEndTile(t,playerQuadrant, GetDirectionFromTilePos(t))).Select(t => new MazeDoor(GetEndTileOffsetFromDirection(t, GetDirectionFromTilePos(t)), GetDirectionFromTilePos(t), new Vector2Int[0])).Where(d => !doorPositions.Contains(d.Position)).ToList();
            Debug.Log($"Possible Exits: {_possibleEnds.Count}");
            if (_possibleEnds.Count > 0)
            {
                var end = _possibleEnds.Count > 1 ? _possibleEnds[Random.Range(0, _possibleEnds.Count)] : _possibleEnds[0];
                UnitTemplate endTemplate = null;
                switch (end.Rotation)
                {
                    case Directions.Left:
                        endTemplate = _settings.RightEndTemplate;
                        break;
                    case Directions.Right:
                        endTemplate = _settings.LeftEndTemplate;
                        break;
                    case Directions.Down:
                        endTemplate = _settings.UpEndTemplate;
                        break;
                    case Directions.Up:
                        endTemplate = _settings.DownEndTemplate;
                        break;
                }

                var endTile = _pathingGrid.AddMapTile(end.Position);
                if (endTile != null && endTemplate)
                {
                    var endController = endTemplate.GenerateUnit(MinigameController.Transform, endTile.World);
                    var setMapTileMsg = MessageFactory.GenerateSetMapTileMsg();
                    setMapTileMsg.Tile = endTile;
                    gameObject.SendMessageTo(setMapTileMsg, endController.gameObject);
                    MessageFactory.CacheMessage(setMapTileMsg);
                }
            }

            //for (var i = 0; i < _possibleEnds.Count; i++)
            //{
            //    var door = _possibleEnds[i];
            //    if (!doorPositions.Contains(door.Position) && IsDeadEndExit(door.Position, door.Rotation))
            //    {
            //        var rotationFlag = eTileFlags.Updated;
            //        switch (door.Rotation.ToDoorRotation())
            //        {
            //            case DoorRotation.Horizontal:
            //                rotationFlag = rotationFlag | eTileFlags.Rot90;
            //                if (door.Rotation == Directions.Right)
            //                {
            //                    rotationFlag = rotationFlag | eTileFlags.FlipH;
            //                }
            //                break;
            //        }
            //        _terrain.SetTile(door.Position.x, door.Position.y, (int)_possibleEndId, 0, rotationFlag);
            //    }
            //}

            //for (var i = 0; i < _deadEnds.Count; i++)
            //{
            //    _terrain.SetTileData(_deadEnds[i].x, _deadEnds[i].y, _deadEndId);
            //}
            Debug.Log($"{deadEnds.Count} Dead Ends");



            MinigameController.SetPathingGrid(_pathingGrid);
            _fogofWar.Setup(_pathingGrid.GetAllMapTiles(), _pathingGrid.Size, _pathingGrid.Offset, new Vector2Int(_tilemap.MinGridX - _settings.EdgeBuffer, _tilemap.MinGridY - _settings.EdgeBuffer),  new Vector2Int(_tilemap.MaxGridX + _settings.EdgeBuffer, _tilemap.MaxGridY + _settings.EdgeBuffer));
            

            _terrain.Refresh(true, true, true, true);
            _tilemap.Refresh(true, true, true, true);
            Debug.Log($"{_tiles.Count} Pathing Tiles");
            return _rooms.Where(r => r.Depth > 2).ToArray();
        }

        private Vector2Int ApplyDirectionalOffset(Vector2Int vector, Directions direction, int space)
        {
            var pos = vector;
            //if (DataController.Roll())
            //{
            //    space *= -1;
            //}
            if (direction.HasFlag(Directions.Left) || direction.HasFlag(Directions.Right))
            {
                pos.x -= 1;
                pos.y += space;
            }
            else if (direction.HasFlag(Directions.Up) || direction.HasFlag(Directions.Down))
            {
                pos.x += space;
                pos.y -= 1;
            }
            return pos;
        }

        private Vector2Int[] GetDoorWalls(Vector2Int doorPos, int space, int roomSize, DoorRotation rotation)
        {
            var returnValues = new List<Vector2Int>();
            var firstWallSpace = roomSize - space;
            var secondWallSpace = roomSize - firstWallSpace;
            switch (rotation)
            {
                case DoorRotation.Horizontal:
                    for (var t = 1; t <= firstWallSpace; t++)
                    {
                        returnValues.Add(doorPos + t * Vector2Int.up);
                    }

                    if (secondWallSpace <= 0)
                    {
                        returnValues.Add(doorPos + Vector2Int.down);
                    }
                    else
                    {
                        for (var t = 1; t <= secondWallSpace; t++)
                        {
                            returnValues.Add(doorPos + t * Vector2Int.down);
                        }
                    }


                    break;
                case DoorRotation.Vertical:
                    for (var t = 1; t <= firstWallSpace; t++)
                    {
                        returnValues.Add(doorPos + t * Vector2Int.right);
                    }
                    if (space > 0)
                    {
                        for (var t = 1; t <= secondWallSpace; t++)
                        {
                            returnValues.Add(doorPos + t * Vector2Int.left);
                        }
                    }
                    break;
            }

            return returnValues.ToArray();
        }

        private bool IsSpawnableTile(Vector2Int tile)
        {
            return _tiles.ContainsKey(tile + Vector2Int.up) && _tiles.ContainsKey(tile + Vector2Int.down) &&
                   _tiles.ContainsKey(tile + Vector2Int.left) && _tiles.ContainsKey(tile + Vector2Int.right);
        }

        private bool IsEndTile(Vector2Int pos, Vector2Int playerQuadrant, Directions direction)
        {
            var quadrant = GetQuadrant(pos);
            var space = _settings.RoomSize;
            if (quadrant.x != playerQuadrant.x && quadrant.y != playerQuadrant.y && !IsSpawnableTile(pos) && !IsCornerTile(pos))
            {
                switch (direction)
                {
                    case Directions.Left:
                        return !(_tiles.ContainsKey(pos + Vector2Int.up * space + Vector2Int.right) || _tiles.ContainsKey(pos + Vector2Int.down * space + Vector2Int.right));
                    case Directions.Right:
                        return !(_tiles.ContainsKey(pos + Vector2Int.up * space + Vector2Int.left) || _tiles.ContainsKey(pos + Vector2Int.down * space + Vector2Int.left));
                    case Directions.Down:
                        return !(_tiles.ContainsKey(pos + Vector2Int.up + Vector2Int.right * space) || _tiles.ContainsKey(pos + Vector2Int.up + Vector2Int.left * space));
                    case Directions.Up:
                        return !(_tiles.ContainsKey(pos + Vector2Int.down + Vector2Int.right * space) || _tiles.ContainsKey(pos + Vector2Int.down + Vector2Int.left * space));
                }
            }
            return false;
        }

        public Vector2Int GetEndTileOffsetFromDirection(Vector2Int pos, Directions direction)
        {
            switch (direction)
            {
                case Directions.None:
                    return pos;
                case Directions.Left:
                    return pos + Vector2Int.right;
                case Directions.Right:
                    return pos + Vector2Int.left;
                case Directions.Down:
                    return pos + Vector2Int.up;
                case Directions.Up:
                    return pos + Vector2Int.down;
            }
            return pos;
        }

        private Vector2Int GetQuadrant(Vector2Int pos)
        {
            var quadrant = Vector2Int.zero;
            if (_tiles.TryGetValue(pos, out var tile))
            {
                if (tile.Cell.X < _pathingGrid.Size.x / 2)
                {
                    quadrant.x = -1;
                }
                else
                {
                    quadrant.x = 1;
                }

                if (tile.Cell.Y < _pathingGrid.Size.y / 2)
                {
                    quadrant.y = -1;
                }
                else
                {
                    quadrant.y = 1;
                }
            }

            return quadrant;
        }

        private bool IsCornerTile(Vector2Int pos)
        {
            var tileCount = 0;
            if (!_tiles.ContainsKey(pos + Vector2Int.up))
            {
                tileCount++;
            }
            else if (!_tiles.ContainsKey(pos + Vector2Int.down))
            {
                tileCount++;
            }
            if (!_tiles.ContainsKey(pos + Vector2Int.left))
            {
                tileCount++;
            }
            else if (!_tiles.ContainsKey(pos + Vector2Int.right))
            {
                tileCount++;
            }
            return tileCount > 1;
        }

        private Directions GetDirectionFromTilePos(Vector2Int pos)
        {
            if (_tiles.TryGetValue(pos, out var tile))
            {
                if (!_tiles.ContainsKey(tile.Position + Vector2Int.up))
                {
                    return Directions.Down;
                }

                if (!_tiles.ContainsKey(tile.Position + Vector2Int.down))
                {
                    return Directions.Up;
                }

                if (!_tiles.ContainsKey(tile.Position + Vector2Int.left))
                {
                    return Directions.Right;
                }

                if (!_tiles.ContainsKey(tile.Position + Vector2Int.right))
                {
                    return Directions.Left;
                }
            }
            return Directions.None;
        }

        private bool IsDeadEndExit(Vector2Int pos, Directions direction)
        {
            var doorPos = pos;
            var space = _settings.RoomSize / 2 + 1;
            var firstCheck = doorPos;
            var secondCheck = doorPos;
            switch (direction)
            {
                case Directions.Left:
                    pos.x -= 1;
                    firstCheck = pos + (Vector2Int.up * space);
                    secondCheck = pos + (Vector2Int.down * space);
                    break;
                case Directions.Right:
                    pos.x += 1;
                    firstCheck = pos + (Vector2Int.up * space);
                    secondCheck = pos + (Vector2Int.down * space);
                    break;
                case Directions.Down:
                    pos.y -= 1;
                    firstCheck = pos + (Vector2Int.left * space);
                    secondCheck = pos + (Vector2Int.right * space);
                    break;
                case Directions.Up:
                    pos.y += 1;
                    firstCheck = pos + (Vector2Int.left * space);
                    secondCheck = pos + (Vector2Int.right * space);
                    break;
            }

            return !_tiles.ContainsKey(firstCheck) && !_tiles.ContainsKey(secondCheck);
        }

        private Vector2Int[] GetSurrounding(Vector2Int pos)
        {
            return new[]
            {
                pos + Vector2Int.up,
                pos + Vector2Int.down,
                pos + Vector2Int.left,
                pos + Vector2Int.right,
                pos + Vector2Int.one,
                pos + new Vector2Int(-1,-1),
                pos + new Vector2Int(1,-1),
                pos + new Vector2Int(-1,1),
            };
        }

        private int SpawnMonsters(MazeMinigameSettings settings, MazeRoom[] rooms)
        {
            var setMapTileMsg = MessageFactory.GenerateSetMapTileMsg();
            var setCombatAlignmentMsg = MessageFactory.GenerateSetCombatAlignmentMsg();
            var totalMonsters = 0;
            for (var i = 0; i < rooms.Length; i++)
            {
                var monsterCount = settings.MonstersPerRoom.Roll();
                var availableTiles = rooms[i].SpawnableTiles.Select(MinigameController.Pathing.GetTileByPosition).Where(t => t != null && !t.Block && IsSpawnableTile(t.Position)).ToList();

                setCombatAlignmentMsg.Alignment = CombatAlignment.Enemy;
                for (var m = 0; m < monsterCount && availableTiles.Count > 0; m++)
                {
                    var tile = availableTiles.Count > 1 ? availableTiles[Random.Range(0, availableTiles.Count)] : availableTiles[0];
                    availableTiles.Remove(tile);
                    var template = settings.MonsterTemplates.Length > 1 ? settings.MonsterTemplates[Random.Range(0, settings.MonsterTemplates.Length)] : settings.MonsterTemplates[0];
                    var controller = template.GenerateUnit(MinigameController.Transform, tile.World);
                    setMapTileMsg.Tile = tile;
                    gameObject.SendMessageTo(setMapTileMsg, controller.gameObject);
                    gameObject.SendMessageTo(setCombatAlignmentMsg, controller.gameObject);
                    MinigameController.RegisterMinigameObject(controller.gameObject);
                    totalMonsters++;
                }
            }
            MessageFactory.CacheMessage(setCombatAlignmentMsg);
            MessageFactory.CacheMessage(setMapTileMsg);
            return totalMonsters;
        }

        private int SpawnChests(MazeMinigameSettings settings, MazeRoom[] rooms)
        {
            var createdCount = 0;
            var maxChests = settings.MaximumChestsPerRoom * rooms.Length;

            var availableRooms = rooms.ToList();
            var chestTiles = new List<MapTile>();
            var unavailblePos = new List<Vector2Int>();
            for (var c = 0; c < maxChests && availableRooms.Count > 0; c++)
            {
                var room = availableRooms.Count > 1 ? availableRooms[Random.Range(0, availableRooms.Count)] : availableRooms[0];
                var spawnableTiles = room.SpawnableTiles.Select(MinigameController.Pathing.GetTileByPosition).Where(t => t != null && !unavailblePos.Contains(t.Position) && _spawnableTiles.Contains(t.Position) && !t.Block && IsSpawnableTile(t.Position)).ToList();
                for (var i = 0; i < settings.MaximumChestsPerRoom && spawnableTiles.Count > 0; i++)
                {
                    var tile = spawnableTiles.Count > 1 ? spawnableTiles[Random.Range(0, spawnableTiles.Count)] : spawnableTiles[0];
                    var surroundingPositions = GetSurrounding(tile.Position);
                    spawnableTiles.RemoveAll(t => surroundingPositions.Contains(t.Position));
                    spawnableTiles.Remove(tile);
                    unavailblePos.AddRange(surroundingPositions.Where(p => !unavailblePos.Contains(p)));
                    unavailblePos.Add(tile.Position);
                    spawnableTiles.Remove(tile);
                    chestTiles.Add(tile);
                }
                availableRooms.Remove(room);
            }
            var setMapTileMsg = MessageFactory.GenerateSetMapTileMsg();
            var setLootTableMsg = MessageFactory.GenerateSetLootTableMsg();
            var chestCount = (int) (maxChests * settings.ChestSpawnPercent);
            while (chestTiles.Count > 0 && chestCount > 0)
            {
                var tile = chestTiles.Count > 1 ? chestTiles[Random.Range(0, chestTiles.Count)] : chestTiles[0];
                createdCount++;
                var lootTable = settings.ChestLootTables.Length > 1 ? settings.ChestLootTables[Random.Range(0, settings.ChestLootTables.Length)] : settings.ChestLootTables[0];
                var template = settings.ChestTemplates[Random.Range(0, settings.ChestTemplates.Length)];
                var unitController = template.GenerateUnit(MinigameController.Transform, tile.World);
                setMapTileMsg.Tile = tile;
                gameObject.SendMessageTo(setMapTileMsg, unitController.gameObject);

                setLootTableMsg.Table = lootTable;
                gameObject.SendMessageTo(setLootTableMsg, unitController.gameObject);
                chestCount--;
                chestTiles.Remove(tile);
            }

            Debug.Log($"Created {createdCount} out of {maxChests} Chests");
            return createdCount;
        }
    }
}