using System.Collections.Generic;
using System.Linq;
using Assets.Resources.Ancible_Tools.Scripts.System.Pathing;
using MessageBusLib;
using UnityEngine;

namespace Assets.Resources.Ancible_Tools.Scripts.System.BattleLeague
{
    public class BattleEncounterBenchController : MonoBehaviour
    {
        private BattleBenchSlotController[] _benchControllers = new BattleBenchSlotController[0];

        private BattleEncounter _encounter = null;
        private BattleUnitData[] _dataInstances = new BattleUnitData[0];
        private Vector2Int _min = Vector2Int.zero;
        private MapTile[] _availableTiles = new MapTile[0];

        private Dictionary<int, GameObject> _encounterPieces = new Dictionary<int, GameObject>();

        public void WakeUp(MapTile[] tiles, Vector2Int min)
        {
            _benchControllers = gameObject.GetComponentsInChildren<BattleBenchSlotController>();
            for (var i = 0; i < _benchControllers.Length; i++)
            {
                _benchControllers[i].Setup(BattleAlignment.Right);
            }
            _availableTiles = tiles.ToArray();
            _min = min;
        }

        public void Setup(BattleEncounter encounter)
        {
            _encounter = encounter;
            _dataInstances = _encounter.GenerateInstances();
            var addTraitToUnitMsg = MessageFactory.GenerateAddTraitToUnitMsg();
            var setGamePieceDataMsg = MessageFactory.GenerateSetGamePieceDataMsg();
            var setFacingDirectionMsg = MessageFactory.GenerateSetFaceDirectionMsg();
            setFacingDirectionMsg.Direction = Vector2.left;
            setGamePieceDataMsg.Alignment = BattleAlignment.Right;
            for (var i = 0; i < _dataInstances.Length && i < _benchControllers.Length; i++)
            {
                var bench = _benchControllers[i];
                var unitController = BattleLeagueController.GamePieceTemplate.GenerateUnit(transform, bench.transform.position.ToVector2());
                setGamePieceDataMsg.Data = _dataInstances[i];
                gameObject.SendMessageTo(setGamePieceDataMsg, unitController.gameObject);

                addTraitToUnitMsg.Trait = _dataInstances[i].Sprite;
                gameObject.SendMessageTo(addTraitToUnitMsg, unitController.gameObject);

                gameObject.SendMessageTo(setFacingDirectionMsg, unitController.gameObject);

                bench.SetCurrentPiece(unitController.gameObject);
                _encounterPieces.Add(i, unitController.gameObject);
            }
            MessageFactory.CacheMessage(setGamePieceDataMsg);
            MessageFactory.CacheMessage(addTraitToUnitMsg);
        }

        public KeyValuePair<MapTile, BattleUnitData>[] GetUnitsForBattle()
        {
            var returnUnits = new Dictionary<MapTile, BattleUnitData>();
            var encounters = _encounter.GetBattleUnits(_min, _availableTiles);
            var totalUnits = _encounter.TotalUnits;
            for (var i = 0; i < encounters.Length; i++)
            {
                if (totalUnits > encounters[i].Value)
                {
                    returnUnits.Add(encounters[i].Key, _dataInstances[i]);
                    gameObject.SendMessageTo(EnterBattleMessage.INSTANCE, _encounterPieces[encounters[i].Value]);
                }
                
            }

            return returnUnits.ToArray();
        }

        public void Prepare()
        {
            var objs = _encounterPieces.Values.ToArray();
            for (var i = 0; i < objs.Length; i++)
            {
                objs[i].gameObject.SetActive(true);
            }
        }

        public KeyValuePair<BattleUnitData, BattleAlignment>[] GetAllUnits()
        {
            var returnData = new Dictionary<BattleUnitData, BattleAlignment>();
            for (var i = 0; i < _dataInstances.Length; i++)
            {
                returnData.Add(_dataInstances[i], BattleAlignment.Right);
            }

            return returnData.ToArray();
        }

        public void Clear()
        {
            var pieces = _encounterPieces.Values.ToArray();
            for (var i = 0; i < pieces.Length; i++)
            {
                Destroy(pieces[i]);
            }
            _encounterPieces.Clear();
            _encounter = null;
            for (var i = 0; i < _dataInstances.Length; i++)
            {
                _dataInstances[i].Dispose();
            }
            _dataInstances = new BattleUnitData[0];
        }
    }
}