using System.Collections.Generic;
using System.Linq;
using Assets.Resources.Ancible_Tools.Scripts.System;
using Assets.Resources.Ancible_Tools.Scripts.System.BattleLeague;
using Assets.Resources.Ancible_Tools.Scripts.System.Pathing;
using MessageBusLib;
using UnityEngine;

namespace Assets.Ancible_Tools.Scripts.Traits
{
    [CreateAssetMenu(fileName = "Battle Aggro Trait", menuName = "Ancible Tools/Traits/Battle/Battle Aggro")]
    public class BattleAggroTrait : Trait
    {
        [SerializeField] private Vector2 _flagOffset = Vector2.zero;
        [SerializeField] private float _diagonalCost = -1;

        private BattleAlignment _alignment = BattleAlignment.None;
        private Dictionary<GameObject, int> _enemyUnits = new Dictionary<GameObject, int>();
        private List<GameObject> _allies = new List<GameObject>();
        private List<GameObject> _enemies = new List<GameObject>();
        private GameObject _currentTarget = null;
        private MapTile _currentTile = null;
        private UnitBattleState _battleState = UnitBattleState.Active;
        private Vector2Int _direction = Vector2Int.zero;

        private bool _global = false;

        private List<MapTile> _currentPath = new List<MapTile>();
        

        public override void SetupController(TraitController controller)
        {
            base.SetupController(controller);
            SubscribeToMessages();
        }

        private bool CanPerformAction(MapTile targetTile)
        {
            var attacked = false;
            var distance = _currentTile.Position.DistanceTo(targetTile.Position);
            var battleAbilityCheckMsg = MessageFactory.GenerateBattleAbilityCheckMsg();
            battleAbilityCheckMsg.Distance = distance;
            battleAbilityCheckMsg.Origin = _currentTile;
            battleAbilityCheckMsg.Target = _currentTarget;
            battleAbilityCheckMsg.Allies = _allies.ToArray();
            battleAbilityCheckMsg.DoAfter = () => { attacked = true;};
            _controller.gameObject.SendMessageTo(battleAbilityCheckMsg, _controller.transform.parent.gameObject);
            MessageFactory.CacheMessage(battleAbilityCheckMsg);

            if (!attacked)
            {
                var basicAttackCheckMsg = MessageFactory.GenerateBasicAttackCheckMsg();
                basicAttackCheckMsg.Origin = _currentTile;
                basicAttackCheckMsg.Target = _currentTarget;
                basicAttackCheckMsg.TargetTile = targetTile;
                basicAttackCheckMsg.DoAfter = () => { attacked = true; };
                basicAttackCheckMsg.Distance = distance;
                _controller.gameObject.SendMessageTo(basicAttackCheckMsg, _controller.transform.parent.gameObject);
                MessageFactory.CacheMessage(basicAttackCheckMsg);
            }

            return attacked;
            
        }

        private void SubscribeToMessages()
        {
            _controller.transform.parent.gameObject.SubscribeWithFilter<UpdateMapTileMessage>(UpdateMapTile, _instanceId);
            _controller.transform.parent.gameObject.SubscribeWithFilter<UpdateUnitBattleStateMessage>(UpdateUnitBattleState, _instanceId);
            _controller.transform.parent.gameObject.SubscribeWithFilter<SetBattleAlignmentMessage>(SetBattleAlignment, _instanceId);
            _controller.transform.parent.gameObject.SubscribeWithFilter<QueryBattleAlignmentMessage>(QueryBattleAlignment, _instanceId);
            _controller.transform.parent.gameObject.SubscribeWithFilter<SetEnemyUnitsMessage>(SetEnemyUnits, _instanceId);
            _controller.transform.parent.gameObject.SubscribeWithFilter<ObstacleMessage>(Obstacle, _instanceId);
            _controller.transform.parent.gameObject.SubscribeWithFilter<RemoveEnemyUnitMessage>(RemoveEnemyUnit, _instanceId);
            _controller.transform.parent.gameObject.SubscribeWithFilter<ReportDamageMessage>(ReportDamage, _instanceId);
            _controller.transform.parent.gameObject.SubscribeWithFilter<SetAlliesMessage>(SetAllies, _instanceId);
            _controller.transform.parent.gameObject.SubscribeWithFilter<RemoveAllyMessage>(RemoveAlly, _instanceId);
        }

        private void BattleAggroCheck(BattleAggroCheckMessage msg)
        {
            if (_enemyUnits.Count > 0)
            {
                if (!_currentTarget)
                {
                    var pos = _controller.transform.position.ToVector2();
                    var orderedThreat = _enemyUnits.OrderByDescending(e => e.Value).ThenBy(e => (e.Key.transform.position.ToVector2() - pos).sqrMagnitude).ToArray();
                    var equalThreat = orderedThreat.Where(e => e.Value == orderedThreat[0].Value).Select(e => e.Key).ToArray();
                    _currentTarget = equalThreat.GetRandom();
                    _currentTarget.gameObject.SubscribeWithFilter<UpdateMapTileMessage>(UpdateTargetMapTile, _instanceId);
                    _currentPath.Clear();
                }

                MapTile targetTile = null;
                var queryTargetTileMsg = MessageFactory.GenerateQueryMapTileMsg();
                queryTargetTileMsg.DoAfter = mapTile => targetTile = mapTile;
                _controller.gameObject.SendMessageTo(queryTargetTileMsg, _currentTarget);
                MessageFactory.CacheMessage(queryTargetTileMsg);


                if (_battleState == UnitBattleState.Active && targetTile != null)
                {
                    if (!CanPerformAction(targetTile))
                    {
                        if (_currentPath.Contains(targetTile))
                        {
                            var index = _currentPath.IndexOf(targetTile);
                            if (index > 1)
                            {
                                _currentPath.RemoveRange(index, _currentPath.Count - index);
                            }
                        }

                        if (_currentPath.Count <= 0)
                        {
                            var availableSurroundingTiles = BattleLeagueController.PathingGrid.GetMapTilesInArea(targetTile.Position, 1);
                            if (availableSurroundingTiles.Length > 0)
                            {
                                if (!availableSurroundingTiles.Contains(_currentTile))
                                {
                                    var goToTile = availableSurroundingTiles.GetRandom();
                                    var path = BattleLeagueController.PathingGrid.GetPath(_currentTile.Position, goToTile.Position, _diagonalCost);
                                    if (path.Length > 0)
                                    {
                                        _currentPath = path.ToList();
                                        var setDirectionMsg = MessageFactory.GenerateSetDirectionMsg();
                                        setDirectionMsg.Direction = (_currentPath[0].Position - _currentTile.Position).Normalize();
                                        _controller.gameObject.SendMessageTo(setDirectionMsg, _controller.transform.parent.gameObject);
                                        MessageFactory.CacheMessage(setDirectionMsg);
                                    }
                                    else
                                    {
                                        Debug.Log("No path to target");
                                        _enemyUnits[_currentTarget] -= 1;
                                        _currentTarget.UnsubscribeFromAllMessagesWithFilter(_instanceId);
                                        _currentTarget = null;
                                    }
                                }
                            }
                            else
                            {
                                Debug.Log("No available tile");
                                _enemyUnits[_currentTarget] -= 1;
                                _currentTarget.UnsubscribeFromAllMessagesWithFilter(_instanceId);
                                _currentTarget = null;
                            }
                        }
                        
                    }
                }
            }
            else
            {
                Debug.Log("No enemies");
            }
        }

        private void SetBattleAlignment(SetBattleAlignmentMessage msg)
        {
            _alignment = msg.Alignment;

            var updateBattleAlignmentMsg = MessageFactory.GenerateUpdateBattleAlignmentMsg();
            updateBattleAlignmentMsg.Alignment = _alignment;
            _controller.gameObject.SendMessageTo(updateBattleAlignmentMsg, _controller.transform.parent.gameObject);
            MessageFactory.CacheMessage(updateBattleAlignmentMsg);
        }

        private void QueryBattleAlignment(QueryBattleAlignmentMessage msg)
        {
            msg.DoAfter.Invoke(_alignment);
        }

        private void SetEnemyUnits(SetEnemyUnitsMessage msg)
        {
            var pairs = msg.Units.ToArray();
            foreach (var pair in pairs)
            {
                if (!_enemyUnits.ContainsKey(pair.Key))
                {

                    var distance = _currentTile.Position.DistanceTo(pair.Value.Position);

                    _enemyUnits.Add(pair.Key, distance * -1);
                    _enemies.Add(pair.Key);
                }
            }
        }

        private void SetAllies(SetAlliesMessage msg)
        {
            _allies = msg.Allies.ToList();
        }

        private void UpdateMapTile(UpdateMapTileMessage msg)
        {
            if (_currentTile == null)
            {
                _controller.transform.parent.gameObject.SubscribeWithFilter<BattleAggroCheckMessage>(BattleAggroCheck, _instanceId);
            }

            _currentTile = msg.Tile;

            if (_currentPath.Count > 0)
            {
                if (_currentPath[0] == _currentTile)
                {
                    _currentPath.RemoveAt(0);
                    var setDirectionMsg = MessageFactory.GenerateSetDirectionMsg();
                    if (_currentPath.Count > 0)
                    {
                        setDirectionMsg.Direction = _currentPath[0].Position - _currentTile.Position;
                    }
                    else
                    {
                        setDirectionMsg.Direction = Vector2.zero;
                    }
                    _controller.gameObject.SendMessageTo(setDirectionMsg, _controller.transform.parent.gameObject);
                    MessageFactory.CacheMessage(setDirectionMsg);
                }
            }
        }

        private void Obstacle(ObstacleMessage msg)
        {
            if (_currentTarget && _currentPath.Count > 0)
            {
                if (_currentTarget != msg.Obstacle)
                {
                    _enemyUnits[_currentTarget] -= 1;
                    if (_enemyUnits.ContainsKey(msg.Obstacle))
                    {
                        _enemyUnits[msg.Obstacle] += 1;
                    }
                    _currentTarget.UnsubscribeFromAllMessagesWithFilter(_instanceId);
                    _currentTarget = null;

                    _currentPath.Clear();
                }
                else
                {
                    _currentPath.Clear();
                }
            }
        }

        private void UpdateUnitBattleState(UpdateUnitBattleStateMessage msg)
        {
            _battleState = msg.State;
            if (_battleState == UnitBattleState.Dead)
            {
                _currentPath.Clear();
                if (_currentTarget)
                {
                    _currentTarget.UnsubscribeFromAllMessagesWithFilter(_instanceId);
                }
                _currentTarget = null;
                _enemyUnits.Clear();
                
            }
        }

        private void RemoveEnemyUnit(RemoveEnemyUnitMessage msg)
        {
            _enemyUnits.Remove(msg.Enemy);
            if (_currentTarget && _currentTarget == msg.Enemy)
            {
                _currentTarget.UnsubscribeFromAllMessagesWithFilter(_instanceId);
                _currentTarget = null;
                _currentPath.Clear();

                var setDirectionMsg = MessageFactory.GenerateSetDirectionMsg();
                setDirectionMsg.Direction = Vector2.zero;
                _controller.gameObject.SendMessageTo(setDirectionMsg, _controller.transform.parent.gameObject);
                MessageFactory.CacheMessage(setDirectionMsg);
            }
        }

        private void RemoveAlly(RemoveAllyMessage msg)
        {
            _allies.Remove(msg.Ally);
        }

        private void ReportDamage(ReportDamageMessage msg)
        {
            if (_enemyUnits.ContainsKey(msg.Owner))
            {
                _enemyUnits[msg.Owner] += BattleLeagueController.GetAggroFromDamage(msg.Amount);
            }
        }

        private void UpdateTargetMapTile(UpdateMapTileMessage msg)
        {
            var targetDirection = (msg.Tile.Position - _currentTile.Position).Normalize();
            if (targetDirection != _direction)
            {
                _currentPath.Clear();

                var setDirectionMsg = MessageFactory.GenerateSetDirectionMsg();
                setDirectionMsg.Direction = Vector2.zero;
                _controller.gameObject.SendMessageTo(setDirectionMsg, _controller.transform.parent.gameObject);
                MessageFactory.CacheMessage(setDirectionMsg);
            }
            else if (_currentPath.Contains(msg.Tile))
            {
                var index = _currentPath.IndexOf(msg.Tile);
                if (index > 1)
                {
                    _currentPath.RemoveRange(index + 1, _currentPath.Count - index);
                }
            }
            else
            {
                _currentPath.AddRange(BattleLeagueController.PathingGrid.GetPath(_currentPath[_currentPath.Count - 1].Position, msg.Tile.Position, _diagonalCost));
            }
        }
    }
}