using System.Collections.Generic;
using System.Linq;
using Assets.Ancible_Tools.Scripts.System.SaveData;
using Assets.Resources.Ancible_Tools.Scripts.System.Pathing;
using Assets.Resources.Ancible_Tools.Scripts.System.UI.BattleLeague;
using MessageBusLib;
using UnityEngine;

namespace Assets.Resources.Ancible_Tools.Scripts.System.BattleLeague
{
    public class BattleBenchController : MonoBehaviour
    {
        [SerializeField] private BattleAlignment _alignment = BattleAlignment.None;

        private Dictionary<MapTile, GameObject> _placedPieces = new Dictionary<MapTile, GameObject>();

        private BattleBenchSlotController[] _benchSlots = new BattleBenchSlotController[0];
        private List<MapTile> _availableTiles = new List<MapTile>();
        private List<GameObject> _benchedPieces = new List<GameObject>();
        private Dictionary<GameObject, BattleUnitData> _units = new Dictionary<GameObject, BattleUnitData>();

        public void WakeUp(MapTile[] tiles)
        {
            _benchSlots = gameObject.GetComponentsInChildren<BattleBenchSlotController>();
            for (var i = 0; i < _benchSlots.Length; i++)
            {
                _benchSlots[i].Setup(_alignment);
            }
            _availableTiles = tiles.ToList();
        }

        public void Setup(BattleUnitData[] units)
        {
            var addTraitToUnitMsg = MessageFactory.GenerateAddTraitToUnitMsg();
            var setGamePieceDataMsg = MessageFactory.GenerateSetGamePieceDataMsg();
            var setFacingDirecionMsg = MessageFactory.GenerateSetFaceDirectionMsg();
            switch (_alignment)
            {
                case BattleAlignment.Left:
                    setFacingDirecionMsg.Direction = Vector2.right;
                    break;
                case BattleAlignment.Right:
                    setFacingDirecionMsg.Direction = Vector2.left;
                    break;
            }
            for (var i = 0; i < units.Length && i < _benchSlots.Length; i++)
            {
                var pos = _benchSlots[i].transform.position.ToVector2();
                var unitController = BattleLeagueController.GamePieceTemplate.GenerateUnit(transform, pos);

                addTraitToUnitMsg.Trait = units[i].Sprite;
                gameObject.SendMessageTo(addTraitToUnitMsg, unitController.gameObject);

                setGamePieceDataMsg.Alignment = _alignment;
                setGamePieceDataMsg.Data = units[i];
                gameObject.SendMessageTo(setGamePieceDataMsg, unitController.gameObject);

                gameObject.SendMessageTo(setFacingDirecionMsg, unitController.gameObject);

                _benchSlots[i].SetCurrentPiece(unitController.gameObject);
                _benchedPieces.Add(unitController.gameObject);
                _units.Add(unitController.gameObject, units[i]);
            }
            MessageFactory.CacheMessage(addTraitToUnitMsg);
            MessageFactory.CacheMessage(setGamePieceDataMsg);
            MessageFactory.CacheMessage(setFacingDirecionMsg);
            UiBattleLeagueScoreController.SetReadyButtonInteractable(_placedPieces.Count > 0);
        }

        public void PlacePieceAtTileFromBench(GameObject piece, MapTile tile, BattleBenchSlotController fromBench)
        {
            var availableTile = _availableTiles.FirstOrDefault(t => t == tile);
            if (availableTile == null)
            {
                if (_placedPieces.TryGetValue(tile, out var existingPiece) && existingPiece != piece)
                {
                    if (!fromBench)
                    {
                        fromBench = _benchSlots.FirstOrDefault(b => !b.CurrentPiece);
                    }

                    fromBench?.SetCurrentPiece(existingPiece);
                    _benchedPieces.Add(existingPiece);
                }

                _placedPieces[tile] = piece;

                availableTile = tile;
            }
            else
            {
                _availableTiles.Remove(availableTile);
                fromBench?.Clear();
                piece.transform.SetParent(transform);
                if (_placedPieces.ContainsKey(availableTile))
                {
                    _placedPieces[availableTile] = piece;
                }
                else
                {
                    _placedPieces.Add(availableTile, piece);
                }
                
            }

            _benchedPieces.Remove(piece);
            var setGamePieceTileMsg = MessageFactory.GenerateSetGamePieceMapTileMsg();
            setGamePieceTileMsg.Tile = availableTile;
            gameObject.SendMessageTo(setGamePieceTileMsg, piece);
            MessageFactory.CacheMessage(setGamePieceTileMsg);
            if (_alignment == BattleAlignment.Left)
            {
                UiBattleLeagueScoreController.SetReadyButtonInteractable(_placedPieces.Count > 0);
            }

        }

        public void PlacePieceAtTileFromPlay(GameObject piece, MapTile toTile, MapTile fromTile)
        {
            var availableTile = _availableTiles.FirstOrDefault(t => t == toTile);
            var setGamePieceTileMsg = MessageFactory.GenerateSetGamePieceMapTileMsg();
            if (availableTile == null)
            {
                _placedPieces[fromTile] = _placedPieces[toTile];

                setGamePieceTileMsg.Tile = fromTile;
                gameObject.SendMessageTo(setGamePieceTileMsg, _placedPieces[fromTile]);
            }
            else
            {
                if (_placedPieces.Remove(fromTile))
                {
                    _availableTiles.Add(fromTile);
                }


                if (_availableTiles.Remove(toTile))
                {
                    _placedPieces.Add(toTile, null);
                }
            }
            _placedPieces[toTile] = piece;
            setGamePieceTileMsg.Tile = toTile;
            gameObject.SendMessageTo(setGamePieceTileMsg, piece);
            MessageFactory.CacheMessage(setGamePieceTileMsg);

            if (_alignment == BattleAlignment.Left)
            {
                UiBattleLeagueScoreController.SetReadyButtonInteractable(_placedPieces.Count > 0);
            }
        }

        public void ChangeGamePieceBenchSlot(GameObject piece, BattleBenchSlotController toBench, BattleBenchSlotController fromBench)
        {
            if (toBench.CurrentPiece && toBench.CurrentPiece != piece)
            {
                fromBench.SetCurrentPiece(toBench.CurrentPiece);
                toBench.Clear();
            }
            toBench.SetCurrentPiece(piece);
        }

        public void BenchPieceFromPlay(GameObject piece, BattleBenchSlotController bench, MapTile tile)
        {
            if (bench)
            {
                if (bench.CurrentPiece)
                {
                    PlacePieceAtTileFromBench(bench.CurrentPiece, tile, bench);
                }
                else
                {
                    if (_placedPieces.TryGetValue(tile, out var currentPiece) && currentPiece == piece)
                    {
                        _placedPieces.Remove(tile);
                        _availableTiles.Add(tile);
                    }
                    bench.SetCurrentPiece(piece);
                    _benchedPieces.Add(piece);
                }
            }
            else
            {
                bench = _benchSlots.FirstOrDefault(b => !b.CurrentPiece);
                if (bench)
                {
                    if (tile != null && _placedPieces.TryGetValue(tile, out var currentPiece) && currentPiece == piece)
                    {
                        _placedPieces.Remove(tile);
                        _availableTiles.Add(tile);
                    }
                    bench.SetCurrentPiece(piece);
                    _benchedPieces.Add(piece);
                }
            }
            UiBattleLeagueScoreController.SetReadyButtonInteractable(_placedPieces.Count > 0);

        }

        public KeyValuePair<MapTile, BattleUnitData>[] GetUnitsForBattle()
        {
            var returnUnits = new Dictionary<MapTile, BattleUnitData>();
            var pairs = _placedPieces.ToArray();
            var queryBattlePieceMsg = MessageFactory.GenerateQueryBattlePieceMsg();
            for (var i = 0; i < pairs.Length; i++)
            {
                BattleUnitData unitData = null;
                queryBattlePieceMsg.DoAfter = (data, alignment) => unitData = data;
                gameObject.SendMessageTo(queryBattlePieceMsg, pairs[i].Value);
                if (unitData != null)
                {
                    returnUnits.Add(pairs[i].Key, unitData);
                    gameObject.SendMessageTo(EnterBattleMessage.INSTANCE, pairs[i].Value);
                }
            }

            return returnUnits.ToArray();
        }

        public KeyValuePair<BattleUnitData, BattleAlignment>[] GetAllUnits()
        {
            var returnUnits = new Dictionary<BattleUnitData, BattleAlignment>();
            var queryBattlePieceMsg = MessageFactory.GenerateQueryBattlePieceMsg();
            queryBattlePieceMsg.DoAfter = (data, alignment) =>
            {
                returnUnits.Add(data, alignment);
            };
            var allPieces = _benchedPieces.ToList();
            allPieces.AddRange(_placedPieces.Values.Where(p => !allPieces.Contains(p)).ToArray());
            for (var i = 0; i < allPieces.Count; i++)
            {
                gameObject.SendMessageTo(queryBattlePieceMsg, allPieces[i]);
            }
            MessageFactory.CacheMessage(queryBattlePieceMsg);

            return returnUnits.ToArray();
        }

        public void Prepare()
        {
            var objs = _placedPieces.Values.ToArray();
            for (var i = 0; i < objs.Length; i++)
            {
                objs[i].gameObject.SetActive(true);
            }
            UiBattleLeagueScoreController.SetReadyButtonInteractable(_placedPieces.Count > 0);
        }

        public BattlePositionData[] GetBattlePositionData()
        {
            var pieces = _placedPieces.ToArray();
            var positionData = new List<BattlePositionData>();
            foreach (var piece in pieces)
            {
                if (_units.TryGetValue(piece.Value, out var data))
                {
                    positionData.Add(new BattlePositionData{Id = data.Id, Position = piece.Key.Position.ToData()});
                }
            }

            return positionData.ToArray();
        }

        public void SetUnitsFromPositionData(BattlePositionData[] data)
        {
            foreach (var pieceData in data)
            {
                var existingUnit = _units.FirstOrDefault(u => u.Value.Id == pieceData.Id);
                if (existingUnit.Key)
                {
                    var tile = _availableTiles.FirstOrDefault(t => t.Position == pieceData.Position.ToVector());
                    if (tile != null)
                    {
                        PlacePieceAtTileFromBench(existingUnit.Key, tile, null);
                    }
                }
            }
        }

        public void Clear()
        {
            for (var i = 0; i < _benchSlots.Length; i++)
            {
                _benchSlots[i].Clear();
            }

            var allUnits = _benchedPieces.ToList();
            allUnits.AddRange(_placedPieces.Values.ToArray());
            for (var i = 0; i < allUnits.Count; i++)
            {
                Destroy(allUnits[i]);
            }

            var tiles = _placedPieces.Keys.ToArray();
            _availableTiles.AddRange(tiles);
            _placedPieces.Clear();
            _benchedPieces.Clear();
            _units.Clear();
            _placedPieces = new Dictionary<MapTile, GameObject>();
            _benchedPieces = new List<GameObject>();
            
        }

    }
}