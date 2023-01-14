using System.Collections.Generic;
using System.Linq;
using Assets.Resources.Ancible_Tools.Scripts.System;
using Assets.Resources.Ancible_Tools.Scripts.System.Combat;
using Assets.Resources.Ancible_Tools.Scripts.System.Minigame;
using Assets.Resources.Ancible_Tools.Scripts.System.Minigame.AI;
using Assets.Resources.Ancible_Tools.Scripts.System.Pathing;
using MessageBusLib;
using UnityEngine;
using MessageFactory = Assets.Resources.Ancible_Tools.Scripts.System.MessageFactory;

namespace Assets.Ancible_Tools.Scripts.Traits
{
    [CreateAssetMenu(fileName = "Minigame Ai Aggro Trait", menuName = "Ancible Tools/Traits/Minigame/Ai/Minigame Ai Aggro")]
    public class MinigameAiAggroTrait : Trait
    {
        [SerializeField] private int _aggroRange = 1;
        [SerializeField] private bool _diagonal = false;

        private MapTile _currentTile = null;
        private MapTile[] _aggroTiles = new MapTile[0];
        private List<GameObject> _aggrodUnits = new List<GameObject>();
        private CombatAlignment _alignment = CombatAlignment.Neutral;
        private MinigameAiState _aiState = MinigameAiState.Wander;
        private MinigameUnitState _unitState = MinigameUnitState.Idle;
        private GameObject _target = null;
        private List<MapTile> _currentPath = new List<MapTile>();

        public override void SetupController(TraitController controller)
        {
            base.SetupController(controller);
            SubscribeToMessages();

        }

        private bool IsAggroable(QueryCombatAlignmentMessage queryAlignmentMsg, QueryMinigameUnitStateMessage queryMinigameUnitStateMsg, GameObject obj)
        {
            if (_alignment == CombatAlignment.Neutral)
            {
                return false;
            }

            var objAlignment = CombatAlignment.Neutral;
            queryAlignmentMsg.DoAfter = alignment => objAlignment = alignment;
            _controller.gameObject.SendMessageTo(queryAlignmentMsg, obj);

            var active = false;
            queryMinigameUnitStateMsg.DoAfter = state => active = state != MinigameUnitState.Disabled;
            _controller.gameObject.SendMessageTo(queryMinigameUnitStateMsg, obj);

            return objAlignment != CombatAlignment.Neutral && objAlignment != _alignment && active;
        }

        private bool CanPerformAction(GameObject target, MapTile targetTile)
        {
            var actionPerformed = false;

            var battleAbilityCheckMsg = MessageFactory.GenerateBattleAbilityCheckMsg();
            battleAbilityCheckMsg.Distance = _currentTile.Position.DistanceTo(targetTile.Position);
            battleAbilityCheckMsg.Target = target;
            battleAbilityCheckMsg.Allies = new GameObject[0];
            battleAbilityCheckMsg.Origin = _currentTile;
            battleAbilityCheckMsg.DoAfter = () => actionPerformed = true;
            _controller.gameObject.SendMessageTo(battleAbilityCheckMsg, _controller.transform.parent.gameObject);
            MessageFactory.CacheMessage(battleAbilityCheckMsg);

            if (!actionPerformed)
            {
                var queryBasicAttackMsg = MessageFactory.GenerateBasicAttackCheckMsg();
                queryBasicAttackMsg.DoAfter = () => { actionPerformed = true; };
                queryBasicAttackMsg.Origin = _currentTile; ;
                queryBasicAttackMsg.TargetTile = targetTile;
                queryBasicAttackMsg.Target = target;
                _controller.gameObject.SendMessageTo(queryBasicAttackMsg, _controller.transform.parent.gameObject);
                MessageFactory.CacheMessage(queryBasicAttackMsg);
            }

            return actionPerformed;
        }

        private void SubscribeToMessages()
        {
            _controller.transform.parent.gameObject.SubscribeWithFilter<UpdateMapTileMessage>(UpdateMapTile, _instanceId);
            _controller.transform.parent.gameObject.SubscribeWithFilter<UpdateMinigameAiStateMessage>(UpdateMinigameAiState, _instanceId);
            _controller.transform.parent.gameObject.SubscribeWithFilter<UpdateMinigameUnitStateMessage>(UpdateMinigameUnitState, _instanceId);
            _controller.transform.parent.gameObject.SubscribeWithFilter<UpdateCombatAlignmentMessage>(UpdateCombatAlignment, _instanceId);
            _controller.transform.parent.gameObject.SubscribeWithFilter<ObstacleMessage>(Obstacle, _instanceId);
        }

        private void UpdateMapTile(UpdateMapTileMessage msg)
        {
            if (_currentTile == null)
            {
                _controller.gameObject.Subscribe<UpdateTickMessage>(UpdateTick);
            }
            _currentTile = msg.Tile;
            _aggroTiles = MinigameController.Pathing.GetTilesInPov(_currentTile.Position, _aggroRange).ToArray();
            if (_currentPath.Count > 0 && _currentPath[0] == _currentTile)
            {
                _currentPath.RemoveAt(0);
                if (_currentPath.Count > 0)
                {
                    var setDirectionMsg = MessageFactory.GenerateSetDirectionMsg();
                    setDirectionMsg.Direction = (_currentPath[0].Position - _currentTile.Position).Normalize();
                    _controller.gameObject.SendMessageTo(setDirectionMsg, _controller.transform.parent.gameObject);
                    MessageFactory.CacheMessage(setDirectionMsg);
                }
                else
                {
                    var setDirectionMsg = MessageFactory.GenerateSetDirectionMsg();
                    setDirectionMsg.Direction = Vector2.zero;
                    _controller.gameObject.SendMessageTo(setDirectionMsg, _controller.transform.parent.gameObject);
                    MessageFactory.CacheMessage(setDirectionMsg);
                }
            }
        }

        private void UpdateTick(UpdateTickMessage msg)
        {
            if (_unitState == MinigameUnitState.Idle)
            {
                if (_alignment != CombatAlignment.Neutral)
                {
                    var aggroableObjs = new List<GameObject>();
                    var queryAlignmentMsg = MessageFactory.GenerateQueryCombatAlignmentMsg();
                    var queryMinigameUnitStateMsg = MessageFactory.GenerateQueryMinigameUnitStateMsg();
                    aggroableObjs.AddRange(_aggroTiles.Select(t => t.Block).Where(o => IsAggroable(queryAlignmentMsg, queryMinigameUnitStateMsg, o)));
                    MessageFactory.CacheMessage(queryAlignmentMsg);
                    _aggrodUnits = aggroableObjs.ToList();
                }

                if (_aggrodUnits.Count > 0 && _aiState != MinigameAiState.Aggro)
                {
                    var setMinigameAiStateMsg = MessageFactory.GenerateSetMinigameAiStateMsg();
                    setMinigameAiStateMsg.State = MinigameAiState.Aggro;
                    _controller.gameObject.SendMessageTo(setMinigameAiStateMsg, _controller.transform.parent.gameObject);
                    MessageFactory.CacheMessage(setMinigameAiStateMsg);
                }
                else if (_aiState == MinigameAiState.Aggro && _aggrodUnits.Count <= 0)
                {
                    var setMinigameAiStateMsg = MessageFactory.GenerateSetMinigameAiStateMsg();
                    setMinigameAiStateMsg.State = MinigameAiState.Wander;
                    _controller.gameObject.SendMessageTo(setMinigameAiStateMsg, _controller.transform.parent.gameObject);
                    MessageFactory.CacheMessage(setMinigameAiStateMsg);
                }

                if (_target && !_aggrodUnits.Contains(_target))
                {
                    _target = null;
                    _target.UnsubscribeFromAllMessagesWithFilter(_instanceId);
                    _currentPath.Clear();
                }
                if (!_target && _aggrodUnits.Count > 0)
                {
                    _target = _aggrodUnits.OrderBy(t => (t.transform.position.ToVector2() - _currentTile.World).sqrMagnitude).FirstOrDefault();
                    _target.SubscribeWithFilter<UpdateMapTileMessage>(TargetUpdateMapTile, _instanceId);
                    _currentPath.Clear();
                }

                if (_target && _currentPath.Count <= 0)
                {
                    MapTile targetTile = null;
                    var queryMapTileMsg = MessageFactory.GenerateQueryMapTileMsg();
                    queryMapTileMsg.DoAfter = tile => targetTile = tile;
                    _controller.gameObject.SendMessageTo(queryMapTileMsg, _target);
                    MessageFactory.CacheMessage(queryMapTileMsg);

                    if (targetTile != null)
                    {
                        if (!CanPerformAction(_target, targetTile))
                        {
                            var targetTiles = MinigameController.Pathing.GetMapTilesInArea(targetTile.Position, 1);
                            if (targetTiles.Length > 0)
                            {
                                var goToTile = targetTiles.Length > 1 ? targetTiles[Random.Range(0, targetTiles.Length)] : targetTiles[0];
                                var path = MinigameController.Pathing.GetPath(_currentTile.Position, goToTile.Position, _diagonal);
                                if (path.Length > 0)
                                {
                                    _currentPath = path.ToList();
                                    var setDirectionMsg = MessageFactory.GenerateSetDirectionMsg();
                                    setDirectionMsg.Direction = (_currentPath[0].Position - _currentTile.Position).Normalize();
                                    _controller.gameObject.SendMessageTo(setDirectionMsg, _controller.transform.parent.gameObject);
                                    MessageFactory.CacheMessage(setDirectionMsg);
                                }
                            }
                        }
                    }

                }
            }
        }

        private void UpdateMinigameUnitState(UpdateMinigameUnitStateMessage msg)
        {
            if (msg.State == MinigameUnitState.Disabled)
            {
                _controller.gameObject.Unsubscribe<UpdateTickMessage>();
            }
            else if (_unitState == MinigameUnitState.Disabled)
            {
                _controller.gameObject.Subscribe<UpdateTickMessage>(UpdateTick);
            }
            _unitState = msg.State;
            
        }

        private void UpdateMinigameAiState(UpdateMinigameAiStateMessage msg)
        {
            _aiState = msg.State;
        }

        private void UpdateCombatAlignment(UpdateCombatAlignmentMessage msg)
        {
            _alignment = msg.Alignment;
        }

        private void TargetUpdateMapTile(UpdateMapTileMessage msg)
        {
            var tileIndex = _currentPath.IndexOf(msg.Tile);

            if (tileIndex > -1)
            {
                if (tileIndex < _currentPath.Count - 1)
                {
                    _currentPath.RemoveRange(tileIndex + 1, _currentPath.Count - 1 - tileIndex);
                }
                if (_currentPath.Count <= 0)
                {
                    var setDirectionMsg = MessageFactory.GenerateSetDirectionMsg();
                    setDirectionMsg.Direction = Vector2.zero;
                    _controller.gameObject.SendMessageTo(setDirectionMsg, _controller.transform.parent.gameObject);
                    MessageFactory.CacheMessage(setDirectionMsg);
                }
            }
            else
            {
                _currentPath.Clear();
                var setDirectionMsg = MessageFactory.GenerateSetDirectionMsg();
                setDirectionMsg.Direction = Vector2.zero;
                _controller.gameObject.SendMessageTo(setDirectionMsg, _controller.transform.parent.gameObject);
                MessageFactory.CacheMessage(setDirectionMsg);
            }
        }

        private void Obstacle(ObstacleMessage msg)
        {
            if (_aiState == MinigameAiState.Aggro)
            {
                _currentPath.Clear();
                var setDirectionMsg = MessageFactory.GenerateSetDirectionMsg();
                setDirectionMsg.Direction = Vector2.zero;
                _controller.gameObject.SendMessageTo(setDirectionMsg, _controller.transform.parent.gameObject);
                MessageFactory.CacheMessage(setDirectionMsg);
                if (_target && _target == msg.Obstacle || _aggrodUnits.Contains(msg.Obstacle))
                {
                    if (!_target || _target != msg.Obstacle)
                    {
                        if (_target)
                        {
                            _target.UnsubscribeFromAllMessagesWithFilter(_instanceId);
                        }
                        _target = msg.Obstacle;
                        _target.SubscribeWithFilter<UpdateMapTileMessage>(TargetUpdateMapTile, _instanceId);
                    }

                    var doBasicAttackMsg = MessageFactory.GenerateDoBasicAttackMsg();
                    doBasicAttackMsg.Direction = msg.Direction.ToVector2Int();
                    doBasicAttackMsg.Target = _target;
                    _controller.gameObject.SendMessageTo(doBasicAttackMsg, _controller.transform.parent.gameObject);
                    MessageFactory.CacheMessage(doBasicAttackMsg);
                }
            }
        }

        

        public override void Destroy()
        {
            if (_target)
            {
                _target.UnsubscribeFromAllMessagesWithFilter(_instanceId);
            }

            _target = null;
            _aggrodUnits.Clear();
            _currentPath.Clear();
            _currentTile = null;
            base.Destroy();
        }
    }
}