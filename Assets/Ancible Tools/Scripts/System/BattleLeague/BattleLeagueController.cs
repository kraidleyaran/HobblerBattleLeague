using System;
using System.Collections.Generic;
using System.Linq;
using Assets.Ancible_Tools.Scripts.Traits;
using Assets.Resources.Ancible_Tools.Scripts.System.Combat;
using Assets.Resources.Ancible_Tools.Scripts.System.Items;
using Assets.Resources.Ancible_Tools.Scripts.System.Pathing;
using Assets.Resources.Ancible_Tools.Scripts.System.Templates;
using Assets.Resources.Ancible_Tools.Scripts.System.TickTimers;
using Assets.Resources.Ancible_Tools.Scripts.System.UI.BattleLeague;
using Assets.Resources.Ancible_Tools.Scripts.System.UI.BattleLeague.Status;
using CreativeSpore.SuperTilemapEditor;
using MessageBusLib;
using UnityEditor;
using UnityEngine;

namespace Assets.Resources.Ancible_Tools.Scripts.System.BattleLeague
{
    public class BattleLeagueController : MonoBehaviour
    {
        public static BattleState State { get; private set; }

        public static PathingGridController PathingGrid => _instance._pathingGrid;
        public static BattleBenchController LeftBench => _instance._leftBenchController;
        public static BattleBenchController RightBench => _instance._rightBenchController;
        public static int DefaultMoveJumpPower => _instance._defaultMoveJumpPower;
        public static float MoveSpeedModifier => _instance._moveSpeedModifier;
        public static float AttackSpeedModifier => _instance._attackSpeedModifier;
        public static int GlobalCooldownTicks => _instance._globalCooldownTicks;
        public static float CellSize => _instance._pathingGrid.CellSize.x;
        public static Sprite LeftSideFlagSprite => _instance._leftSideFlagSprite;
        public static Sprite RightSideFlagSprite => _instance._rightSideFlagSprite;
        public static UnitTemplate GamePieceTemplate => _instance._gamePieceTemplate;
        public static Transform Transform => _instance.transform;

        private static BattleLeagueController _instance = null;

        public string MatchId { get; private set; }

        [SerializeField] private UnitTemplate _battleUnitTemplate;
        [SerializeField] private PathingGridController _pathingGrid = null;
        [SerializeField] private BattleBenchController _leftBenchController = null;
        [SerializeField] private BattleBenchController _rightBenchController = null;
        [SerializeField] private BattleEncounterBenchController _encounterBenchController = null;
        [SerializeField] private UnitTemplate _gamePieceTemplate = null;
        [SerializeField] private Vector2Int _leftSideStart = Vector2Int.zero;
        [SerializeField] private Vector2Int _leftSideEnd = new Vector2Int(3,7);
        [SerializeField] private Vector2Int _rightSideStart = new Vector2Int(4,0);
        [SerializeField] private Vector2Int _rightSideEnd = new Vector2Int(7, 7);
        [SerializeField] private int _spawnCountPerSide = 4;
        [SerializeField] private int _defaultMoveJumpPower = 2;
        [SerializeField] private float _moveSpeedModifier = 1.2f;
        [SerializeField] private float _attackSpeedModifier = .5f;
        [SerializeField] private int _globalCooldownTicks = 1;
        [SerializeField] private int _prepPhaseTicks = 100;
        [SerializeField] private int _endofRoundTicks = 100;
        [SerializeField] private int _preBattleTicks = 300;
        [SerializeField] private float _aggroPerDamage = 1f;
        [SerializeField] private Sprite _leftSideFlagSprite = null;
        [SerializeField] private Sprite _rightSideFlagSprite = null;
        [SerializeField] private Trait[] _applyOnLeftDeath = null;
        [SerializeField] private Trait[] _applyOnRightDeath = null;


        private MapTile[] _leftSideTiles = new MapTile[0];
        private MapTile[] _rightSideTiles = new MapTile[0];
        
        private Dictionary<GameObject, BattleAlignment> _allUnits = new Dictionary<GameObject, BattleAlignment>();
        private Dictionary<GameObject, BattleAlignment> _deadUnits = new Dictionary<GameObject, BattleAlignment>();

        private UpdateBattleLeagueScoreMessage _updateBattleLeagueScoreMsg = new UpdateBattleLeagueScoreMessage();
        private UpdateBattleStateMessage _updatebattleStateMsg = new UpdateBattleStateMessage();
        
        

        private int _leftPoints = 0;
        private int _rightPoints = 0;
        private int _goalPoints = 0;
        private int _round = 0;
        private int _maxRounds = 0;

        private TickTimer _prepPhaseTimer = null;
        private TickTimer _victoryTimer = null;
        private TickTimer _preBattleTimer = null;


        private BattleType _type = BattleType.Encounter;
        private BattleEncounter _encounter = null;

        private ItemStack[] _loot = new ItemStack[0];

        public void WakeUp()
        {
            if (_instance)
            {
                Destroy(_instance.gameObject);
            }

            _instance = this;
            _pathingGrid.Setup();
            var battleTiles = _pathingGrid.GetAllMapTiles();
            _leftSideTiles = battleTiles.Where(t => t.Position.x >= _leftSideStart.x && t.Position.x <= _leftSideEnd.x && t.Position.y >= _leftSideStart.y && t.Position.y <= _leftSideEnd.y).ToArray();
            _rightSideTiles = battleTiles.Where(t => t.Position.x >= _rightSideStart.x && t.Position.x <= _rightSideEnd.x && t.Position.y >= _rightSideStart.y && t.Position.y <= _rightSideEnd.y).ToArray();
            _leftBenchController.WakeUp(_leftSideTiles);
            _rightBenchController.WakeUp(_rightSideTiles);
            _encounterBenchController.WakeUp(_rightSideTiles, _rightSideStart);
            SubscribeToMessages();
            gameObject.SetActive(false);
        }

        public void Setup(BattleUnitData[] left, BattleUnitData[] right, int requiredPoints = 100, int maxRounds = 100)
        {
            _type = BattleType.Player;
            _encounterBenchController.gameObject.SetActive(false);
            _goalPoints = requiredPoints;
            _maxRounds = maxRounds;
            var updateBattlePointRequirementMsg = MessageFactory.GenerateUpdateBattlePointRequirementMsg();
            updateBattlePointRequirementMsg.Requirement = _goalPoints;
            updateBattlePointRequirementMsg.Rounds = _maxRounds;
            gameObject.SendMessage(updateBattlePointRequirementMsg);
            MessageFactory.CacheMessage(updateBattlePointRequirementMsg);

            gameObject.SetActive(true);
            LeftBench.Setup(left);
            LeftBench.SetUnitsFromPositionData(BattleLeagueManager.GetSavedBattlePositionData());
            RightBench.Setup(right);
            MatchId = GUID.Generate().ToString();
            StartPrepTimer();
        }

        public void Setup(BattleUnitData[] player, BattleEncounter encounter)
        {
            _type = BattleType.Encounter;
            _rightBenchController.gameObject.SetActive(false);
            _goalPoints = encounter.RequiredPoints;
            _maxRounds = encounter.MaximumRounds;
            _encounter = encounter;
            var updateBattlePointRequirementMsg = MessageFactory.GenerateUpdateBattlePointRequirementMsg();
            updateBattlePointRequirementMsg.Requirement = _goalPoints;
            updateBattlePointRequirementMsg.Rounds = _maxRounds;
            gameObject.SendMessage(updateBattlePointRequirementMsg);
            MessageFactory.CacheMessage(updateBattlePointRequirementMsg);

            gameObject.SetActive(true);

            LeftBench.Setup(player);
            LeftBench.SetUnitsFromPositionData(BattleLeagueManager.GetSavedBattlePositionData());
            _encounterBenchController.Setup(encounter);
            MatchId = GUID.Generate().ToString();
            StartPrepTimer();
        }

        public void Clear()
        {
            _leftBenchController.Clear();
            _rightBenchController.Clear();
            _encounterBenchController.Clear();
            _goalPoints = 0;
            _leftPoints = 0;
            _rightPoints = 0;
            _round = 0;
            _maxRounds = 0;
            UiBattleUnitStatusManager.Clear();
            MatchId = string.Empty;
        }

        public static void RemoveUnit(GameObject unit, BattleAlignment alignment, int spirit = 0)
        {
            var enemyUnits = new GameObject[0];
            var allyUnits = new GameObject[0];
            var totalPoints = Mathf.Max(1, spirit);
            
            switch (alignment)
            {
                case BattleAlignment.Left:
                    _instance._rightPoints += totalPoints;
                    enemyUnits = _instance._allUnits.Where(kv => kv.Value == BattleAlignment.Right).Select(kv => kv.Key).ToArray();
                    allyUnits = _instance._allUnits.Where(kv => kv.Value == BattleAlignment.Left).Select(kv => kv.Key).ToArray();
                    _instance.gameObject.AddTraitsToUnit(_instance._applyOnLeftDeath, unit);
                    break;
                case BattleAlignment.Right:
                    _instance._leftPoints += totalPoints;
                    enemyUnits = _instance._allUnits.Where(kv => kv.Value == BattleAlignment.Left).Select(kv => kv.Key).ToArray();
                    allyUnits = _instance._allUnits.Where(kv => kv.Value == BattleAlignment.Right).Select(kv => kv.Key).ToArray();
                    _instance.gameObject.AddTraitsToUnit(_instance._applyOnRightDeath, unit);
                    break;
            }
            var removeEnemyUnitMsg = MessageFactory.GenerateRemoveEnemyUnitMsg();
            removeEnemyUnitMsg.Enemy = unit;
            for (var i = 0; i < enemyUnits.Length; i++)
            {
                _instance.gameObject.SendMessageTo(removeEnemyUnitMsg, enemyUnits[i]);
            }
            MessageFactory.CacheMessage(removeEnemyUnitMsg);

            var removeAllyUnitMsg = MessageFactory.GenerateRemoveAllyMsg();
            removeAllyUnitMsg.Ally = unit;
            for (var i = 0; i < allyUnits.Length; i++)
            {
                _instance.gameObject.SendMessageTo(removeAllyUnitMsg, allyUnits[i]);
            }
            MessageFactory.CacheMessage(removeAllyUnitMsg);

            _instance._allUnits.Remove(unit);
            _instance._deadUnits.Add(unit, alignment);

            _instance._updateBattleLeagueScoreMsg.LeftSide = _instance._leftPoints;
            _instance._updateBattleLeagueScoreMsg.RightSide = _instance._rightPoints;
            _instance.gameObject.SendMessage(_instance._updateBattleLeagueScoreMsg);

            var leftUnitCount = _instance._allUnits.Count(u => u.Value == BattleAlignment.Left);
            var rightUnitCount = _instance._allUnits.Count(u => u.Value == BattleAlignment.Right);
            var roundOver = leftUnitCount <= 0 || rightUnitCount <= 0;
            if (roundOver)
            {
                var victor = leftUnitCount > 0 ? BattleAlignment.Left : BattleAlignment.Right;
                _instance.FinishRound(victor);
            }
        }

        public static int GetAggroFromDamage(int damage)
        {
            var aggro = (int)(damage * _instance._aggroPerDamage);
            if (aggro <= 0)
            {
                aggro++;
            }

            return aggro;
        }

        public static MapTile GetMapTileByWorldPositionAlignment(Vector2 worldPos, BattleAlignment alignment)
        {
            var tilePos = TilemapUtils.GetGridPositionInt(_instance._pathingGrid.Tilemap, _instance._pathingGrid.transform.InverseTransformPoint(worldPos).ToVector2());
            switch (alignment)
            {
                case BattleAlignment.Left:
                    return _instance._leftSideTiles.FirstOrDefault(t => t.Position == tilePos);
                case BattleAlignment.Right:
                    return _instance._rightSideTiles.FirstOrDefault(t => t.Position == tilePos);
                default:
                    return null;
            }
        }

        private void SetupRound()
        {
            _pathingGrid.Clear();
            var positionData = LeftBench.GetBattlePositionData();
            if (positionData.Length > 0)
            {
                BattleLeagueManager.SetSavedBattlePositions(positionData);
            }
            var addTraitToUnitMsg = MessageFactory.GenerateAddTraitToUnitMsg();
            var setMapTileMsg = MessageFactory.GenerateSetMapTileMsg();
            var setAbilitiesMsg = MessageFactory.GenerateSetAbilitiesMsg();
            var setCombatStatsMsg = MessageFactory.GenerateSetCombatStatsMsg();
            var setBasicAttackSetupMsg = MessageFactory.GenerateSetBasicAttackSetupMsg();
            var setSpriteMsg = MessageFactory.GenerateSetSpriteMsg();
            var setBattleAlignmentMsg = MessageFactory.GenerateSetBattleAlignmentMsg();
            var setGamePieceDataMsg = MessageFactory.GenerateSetGamePieceDataMsg();
            var setFacingDirectionMsg = MessageFactory.GenerateSetFaceDirectionMsg();


            setBattleAlignmentMsg.Alignment = BattleAlignment.Left;
            setGamePieceDataMsg.Alignment = BattleAlignment.Left;
            setFacingDirectionMsg.Direction = Vector2.right;
            var leftSideUnits = LeftBench.GetUnitsForBattle();
            var leftUnits = new Dictionary<GameObject, MapTile>();
            for (var i = 0; i < leftSideUnits.Length; i++)
            {
                var tile = leftSideUnits[i].Key;
                var unitController = GenerateUnit(leftSideUnits[i].Value, tile, setSpriteMsg, setMapTileMsg, setAbilitiesMsg, setCombatStatsMsg, setBasicAttackSetupMsg, setGamePieceDataMsg, setFacingDirectionMsg);
                gameObject.SendMessageTo(setBattleAlignmentMsg, unitController.gameObject);
                _allUnits.Add(unitController.gameObject, BattleAlignment.Left);
                leftUnits.Add(unitController.gameObject, tile);
            }

            setBattleAlignmentMsg.Alignment = BattleAlignment.Right;
            setGamePieceDataMsg.Alignment = BattleAlignment.Right;
            setFacingDirectionMsg.Direction = Vector2.left;

            var rightSideUnits = _type == BattleType.Player ? RightBench.GetUnitsForBattle() : _encounterBenchController.GetUnitsForBattle();
            var rightUnits = new Dictionary<GameObject, MapTile>();
            for (var i = 0; i < rightSideUnits.Length; i++)
            {
                var tile = rightSideUnits[i].Key;
                var unitController = GenerateUnit(rightSideUnits[i].Value,tile, setSpriteMsg, setMapTileMsg, setAbilitiesMsg, setCombatStatsMsg, setBasicAttackSetupMsg, setGamePieceDataMsg, setFacingDirectionMsg);
                gameObject.SendMessageTo(setBattleAlignmentMsg, unitController.gameObject);
                _allUnits.Add(unitController.gameObject, BattleAlignment.Right);
                rightUnits.Add(unitController.gameObject, tile);
            }
            
            MessageFactory.CacheMessage(setMapTileMsg);
            MessageFactory.CacheMessage(setBattleAlignmentMsg);
            MessageFactory.CacheMessage(addTraitToUnitMsg);
            MessageFactory.CacheMessage(setAbilitiesMsg);
            MessageFactory.CacheMessage(setCombatStatsMsg);
            MessageFactory.CacheMessage(setBasicAttackSetupMsg);
            MessageFactory.CacheMessage(setGamePieceDataMsg);

            var setEnemyUnitsMsg = MessageFactory.GenerateSetEnemyUnitsMsg();
            var setAllyUnitsMsg = MessageFactory.GenerateSetAlliesMsg();
            //var leftUnits = _allUnits.Where(kv => kv.Value == BattleAlignment.Left).Select(kv => kv.Key).ToArray();
            //var rightUnits = _allUnits.Where(kv => kv.Value == BattleAlignment.Right).Select(kv => kv.Key).ToArray();

            setEnemyUnitsMsg.Units = rightUnits;
            setAllyUnitsMsg.Allies = _allUnits.Where(kv => kv.Value == BattleAlignment.Left).Select(kv => kv.Key).ToArray();

            var left = leftUnits.Keys.ToArray();
            foreach (var unit in left)
            {
                gameObject.SendMessageTo(setEnemyUnitsMsg, unit);
                gameObject.SendMessageTo(setAllyUnitsMsg, unit);
            }

            setEnemyUnitsMsg.Units = leftUnits;
            setAllyUnitsMsg.Allies = _allUnits.Where(kv => kv.Value == BattleAlignment.Left).Select(kv => kv.Key).ToArray(); ;

            var right = rightUnits.Keys.ToArray();
            foreach (var unit in right)
            {
                gameObject.SendMessageTo(setEnemyUnitsMsg, unit);
                gameObject.SendMessageTo(setAllyUnitsMsg, unit);
            }
            MessageFactory.CacheMessage(setEnemyUnitsMsg);
            StartCountdownTimer();
        }

        private void FinishRound(BattleAlignment roundVictor)
        {
            State = BattleState.Results;
            _updatebattleStateMsg.State = BattleState.Results;
            gameObject.SendMessage(_updatebattleStateMsg);

            UiBattleLeagueTimerController.UpdateTimer(1,1, BattleState.Results, _round, false);
            var allUnits = _allUnits.Values.ToArray();
            var setBattleUnitStateMsg = MessageFactory.GenerateSetUnitBattleStateMsg();
            setBattleUnitStateMsg.State = UnitBattleState.End;
            for (var i = 0; i < allUnits.Length; i++)
            {
                gameObject.SendMessageTo(setBattleUnitStateMsg, allUnits[i]);
            }
            MessageFactory.CacheMessage(setBattleUnitStateMsg);

            var victoryUnits = _allUnits.Where(kv => kv.Value == roundVictor).ToArray();
            for (var i = 0; i < victoryUnits.Length; i++)
            {
                gameObject.SendMessageTo(DoVictoryAnimationMessage.INSTANCE, victoryUnits[i].Key);
            }

            gameObject.Unsubscribe<UpdateTickMessage>();
            _victoryTimer = new TickTimer(_endofRoundTicks, 0, CleanUpRound, () =>
            {
                _victoryTimer.Destroy();
                _victoryTimer = null;
            });
        }

        private void CleanUpRound()
        {
            var units = _allUnits.Keys.ToList();
            units.AddRange(_deadUnits.Keys);
            for (var i = 0; i < units.Count; i++)
            {
                Destroy(units[i]);
            }
            _allUnits.Clear();
            _deadUnits.Clear();
            _allUnits = new Dictionary<GameObject, BattleAlignment>();
            _deadUnits = new Dictionary<GameObject, BattleAlignment>();
            UiBattleUnitStatusManager.Clear();
            LeftBench.Prepare();
            switch (_type)
            {
                case BattleType.Encounter:
                    _encounterBenchController.Prepare();
                    break;
                case BattleType.Player:
                    RightBench.Prepare();
                    break;
            }
            // If one side has enough points to reach the goal or we're over on the max rounds AND one of the players is ahead of the other, then we finish - otherwise, keep going
            if ((_leftPoints >= _goalPoints || _rightPoints >= _goalPoints || _maxRounds > 0 && _round + 1 >= _maxRounds) && (_leftPoints > _rightPoints || _rightPoints > _leftPoints))
            {
                State = BattleState.End;
                _updatebattleStateMsg.State = State;
                gameObject.SendMessage(_updatebattleStateMsg);

                var result = _leftPoints > _rightPoints ? BattleResult.Victory : BattleResult.Defeat;
                var allUnits = _leftBenchController.GetAllUnits().ToList();
                switch (_type)
                {
                    case BattleType.Encounter:
                        allUnits.AddRange(_encounterBenchController.GetAllUnits());
                        break;
                    case BattleType.Player:
                        allUnits.AddRange(_rightBenchController.GetAllUnits());
                        break;
                }
                BattleLeagueManager.EncounterFinished(_leftPoints, _rightPoints, allUnits.ToArray(), _round + 1, result);
                //_showBattleResultsWindowMsg.Result = _leftPoints > _rightPoints ? BattleResult.Victory : BattleResult.Defeat;
                //_showBattleResultsWindowMsg.LeftScore = _leftPoints;
                //_showBattleResultsWindowMsg.RightScore = _rightPoints;
                //var allUnits = _leftBenchController.GetAllUnits().ToList();
                //switch (_type)
                //{
                //    case BattleType.Encounter:
                //        allUnits.AddRange(_encounterBenchController.GetAllUnits());
                //        break;
                //    case BattleType.Player:
                //        allUnits.AddRange(_rightBenchController.GetAllUnits());
                //        break;
                //}

                //_showBattleResultsWindowMsg.Units = allUnits.ToArray();
                //_showBattleResultsWindowMsg.TotalRounds = _round + 1;
                //gameObject.SendMessage(_showBattleResultsWindowMsg);
            }
            else
            {
                _round++;
                StartPrepTimer();
            }


        }

        private void StartPrepTimer()
        {
            State = BattleState.Prep;
            _updatebattleStateMsg.State = State;
            gameObject.SendMessage(_updatebattleStateMsg);
            if (_prepPhaseTimer != null)
            {
                _prepPhaseTimer.Destroy();
                _prepPhaseTimer = null;
            }
            UiBattleLeagueTimerController.UpdateTimer(0, _prepPhaseTicks, BattleState.Prep, _round, IsOvertime());
            UiBattleLeagueTimerController.SetActive(true);
            UiBattleLeagueScoreController.SetReadyButtonAvailable(true);
            _prepPhaseTimer = new TickTimer(_prepPhaseTicks, 0, SetupRound, () =>
            {
                _prepPhaseTimer.Destroy();
                _prepPhaseTimer = null;
            });
            _prepPhaseTimer.OnTickUpdate += (current, max) => { UiBattleLeagueTimerController.UpdateTimer(current, max, BattleState.Prep, _round, IsOvertime());};
        }

        private void StartCountdownTimer()
        {
            State = BattleState.Countdown;
            _updatebattleStateMsg.State = State;
            gameObject.SendMessage(_updatebattleStateMsg);
            if (_preBattleTimer != null)
            {
                _preBattleTimer.Destroy();
                _preBattleTimer = null;
            }
            UiBattleLeagueTimerController.UpdateTimer(0, _preBattleTicks, BattleState.Countdown, _round, IsOvertime());
            UiBattleLeagueTimerController.SetActive(true);
            UiBattleLeagueScoreController.SetReadyButtonAvailable(false);
            _preBattleTimer = new TickTimer(_preBattleTicks, 0, StartBattle, () =>
            {
                _preBattleTimer.Destroy();
                _preBattleTimer = null;
                UiBattleLeagueTimerController.UpdateTimer(1,1, BattleState.Battle, _round, IsOvertime());
            });
            _preBattleTimer.OnTickUpdate += (current, max) => { UiBattleLeagueTimerController.UpdateTimer(current,max, BattleState.Countdown, _round, IsOvertime());};
        }

        private void StartBattle()
        {
            State = BattleState.Battle;

            gameObject.Subscribe<UpdateTickMessage>(UpdateTick);
            gameObject.SendMessage(StartBattleMessage.INSTANCE);
            var leftUnits = _allUnits.Count(kv => kv.Value == BattleAlignment.Left);
            var rightUnits = _allUnits.Count(kv => kv.Value == BattleAlignment.Right);
            if (leftUnits <= 0 || rightUnits <= 0)
            {
                var points = leftUnits > 0 ? leftUnits : rightUnits;
                if (leftUnits <= 0)
                {
                    _rightPoints += points;
                    FinishRound(BattleAlignment.Right);
                }
                else
                {
                    _leftPoints += points;
                    FinishRound(BattleAlignment.Left);
                }

                _updateBattleLeagueScoreMsg.LeftSide = _leftPoints;
                _updateBattleLeagueScoreMsg.RightSide = _rightPoints;
                gameObject.SendMessage(_updateBattleLeagueScoreMsg);
            }
        }

        private UnitController GenerateUnit(BattleUnitData unitData, MapTile tile, SetSpriteMessage setSpriteMsg, SetMapTileMessage tileMsg, SetAbilitiesMessage setAbilitiesMsg, SetCombatStatsMessage setCombatStatsMsg, SetBasicAttackSetupMessage setBasicAttackSetupMsg, SetGamePieceDataMessage setGamePieceDataMsg, SetFaceDirectionMessage setFacingDirectionMsg)
        {
            var unitController = _battleUnitTemplate.GenerateUnit(transform, tile.World);

            tileMsg.Tile = tile;
            gameObject.SendMessageTo(tileMsg, unitController.gameObject);

            setAbilitiesMsg.Abilities = unitData.Abilities;
            gameObject.SendMessageTo(setAbilitiesMsg, unitController.gameObject);

            setCombatStatsMsg.Stats = unitData.Stats;
            gameObject.SendMessageTo(setCombatStatsMsg, unitController.gameObject);

            setSpriteMsg.Sprite = unitData.Sprite;
            gameObject.SendMessageTo(setSpriteMsg, unitController.gameObject);

            setBasicAttackSetupMsg.Setup = unitData.BasicAttack;
            gameObject.SendMessageTo(setBasicAttackSetupMsg, unitController.gameObject);

            setGamePieceDataMsg.Data = unitData;
            gameObject.SendMessageTo(setGamePieceDataMsg, unitController.gameObject);

            gameObject.SendMessageTo(setFacingDirectionMsg, unitController.gameObject);

            return unitController;
        }

        private bool IsOvertime()
        {
            return _maxRounds > 0 && _round >= _maxRounds || _leftPoints >= _goalPoints || _rightPoints >= _goalPoints;
        }

        private void SubscribeToMessages()
        {
            gameObject.Subscribe<PlayerReadyForBattleMessage>(PlayerReadyForBattle);
        }

        private void UpdateTick(UpdateTickMessage msg)
        {
            if (State == BattleState.Battle)
            {
                var allUnits = _allUnits.Keys.ToList();
                while (allUnits.Count > 0)
                {
                    var unit = allUnits.GetRandom();
                    gameObject.SendMessageTo(msg, unit);
                    allUnits.Remove(unit);
                }
            }
            else
            {
                gameObject.Unsubscribe<UpdateTickMessage>();
            }
        }

        private void PlayerReadyForBattle(PlayerReadyForBattleMessage msg)
        {
            if (State == BattleState.Prep)
            {
                _prepPhaseTimer.Stop(false);
                _prepPhaseTimer.Destroy();
                _prepPhaseTimer = null;
                SetupRound();
            }
        }

        void OnDestroy()
        {
            gameObject.UnsubscribeFromAllMessages();
        }
    }
}