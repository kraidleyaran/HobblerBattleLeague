using System.Collections.Generic;
using System.Linq;
using Assets.Resources.Ancible_Tools.Scripts.System;
using Assets.Resources.Ancible_Tools.Scripts.System.Adventure;
using Assets.Resources.Ancible_Tools.Scripts.System.Pathing;
using Assets.Resources.Ancible_Tools.Scripts.System.TickTimers;
using MessageBusLib;
using UnityEngine;

namespace Assets.Ancible_Tools.Scripts.Traits
{
    [CreateAssetMenu(fileName = "Adventure Ai Wander Trait", menuName = "Ancible Tools/Traits/Adventure/Ai/Wander")]
    public class AdventureAiWanderTrait : Trait
    {
        [SerializeField] private int _area = 1;
        [SerializeField] private bool _isMonster = false;
        [SerializeField] private IntNumberRange _idleTicks = IntNumberRange.One;
        [SerializeField] [Range(0f, 1f)] private float _chanceToIdle = 0f;

        private MapTile _currentTile = null;

        private List<MapTile> _path = new List<MapTile>();

        private AdventureUnitState _unitState = AdventureUnitState.Idle;
        private AdventureAiState _adventureAiState = AdventureAiState.Wander;
        private TickTimer _idleTimer = null;

        public override void SetupController(TraitController controller)
        {
            base.SetupController(controller);
            SubscribeToMessages();
        }

        private bool IsIdle()
        {
            var idle = _chanceToIdle >= Random.Range(0f, 1f);
            if (idle)
            {
                if (_idleTimer != null)
                {
                    _idleTimer.Destroy();
                    _idleTimer = null;
                }

                _idleTimer = new TickTimer(_idleTicks.Roll(), 0, IdleFinished, null);
                return true;
            }

            return false;
        }

        private void IdleFinished()
        {
            _idleTimer.Destroy();
            _idleTimer = null;
        }

        private void SubscribeToMessages()
        {
            _controller.transform.parent.gameObject.SubscribeWithFilter<UpdateMapTileMessage>(UpdateMapTile, _instanceId);
            _controller.transform.parent.gameObject.SubscribeWithFilter<UpdateAdventureUnitStateMessage>(UpdateAdventureUnitState, _instanceId);
            _controller.transform.parent.gameObject.SubscribeWithFilter<ObstacleMessage>(Obstacle, _instanceId);
            _controller.transform.parent.gameObject.SubscribeWithFilter<UpdateAdventureAiStateMessage>(UpdateAdventureAiState, _instanceId);
        }

        private void UpdateMapTile(UpdateMapTileMessage msg)
        {
            if (_currentTile == null)
            {
                _controller.gameObject.Subscribe<UpdateTickMessage>(UpdateTick);
            }

            _currentTile = msg.Tile;
            if (_path.Count > 0)
            {
                if (_path[0] == _currentTile)
                {
                    _path.RemoveAt(0);
                }

                var setDirectionMsg = MessageFactory.GenerateSetDirectionMsg();
                if (_path.Count > 0)
                {
                    var direction = _path[0].Position - _currentTile.Position;
                    setDirectionMsg.Direction = direction;
                }
                else
                {
                    setDirectionMsg.Direction = Vector2.zero;
                }
                _controller.gameObject.SendMessageTo(setDirectionMsg, _controller.transform.parent.gameObject);
                MessageFactory.CacheMessage(setDirectionMsg);
            }
        }

        private void UpdateTick(UpdateTickMessage msg)
        {
            if (_unitState == AdventureUnitState.Idle && _path.Count <= 0 && _idleTimer == null && _adventureAiState == AdventureAiState.Wander)
            {
                var idle = IsIdle();
                if (!idle)
                {
                    var pathingGrid = _isMonster ? WorldAdventureController.MapController.MonsterPathing : WorldAdventureController.MapController.PlayerPathing;
                    var mapTiles = pathingGrid.GetMapTilesInArea(_currentTile.Position, _area);
                    if (mapTiles.Length > 0)
                    {
                        var tile = mapTiles.GetRandom();
                        var path = pathingGrid.GetPath(_currentTile.Position, tile.Position);
                        if (path.Length > 0)
                        {
                            _path = path.ToList();
                            var nextTile = _path[0];
                            var setDirectionMsg = MessageFactory.GenerateSetDirectionMsg();
                            setDirectionMsg.Direction = nextTile.Position - _currentTile.Position;
                            _controller.gameObject.SendMessageTo(setDirectionMsg, _controller.transform.parent.gameObject);
                            MessageFactory.CacheMessage(setDirectionMsg);
                        }
                    }
                    else
                    {
                        _controller.gameObject.SendMessageTo(ActivateGlobalCooldownMessage.INSTANCE, _controller.transform.parent.gameObject);
                    }
                }

            }
        }

        private void UpdateAdventureUnitState(UpdateAdventureUnitStateMessage msg)
        {
            _unitState = msg.State;
        }

        private void UpdateAdventureAiState(UpdateAdventureAiStateMessage msg)
        {
            _adventureAiState = msg.State;
            if (_adventureAiState != AdventureAiState.Wander)
            {
                _path.Clear();
                _idleTimer?.Destroy();
                _idleTimer = null;
            }
        }

        private void Obstacle(ObstacleMessage msg)
        {
            if (_adventureAiState == AdventureAiState.Wander)
            {
                _path.Clear();
                var setDirectionMsg = MessageFactory.GenerateSetDirectionMsg();
                setDirectionMsg.Direction = Vector2.zero;
                _controller.gameObject.SendMessageTo(setDirectionMsg, _controller.transform.parent.gameObject);
                MessageFactory.CacheMessage(setDirectionMsg);
            }

        }

        

        private void OnDestroy()
        {
            if (_idleTimer != null)
            {
                _idleTimer.Destroy();
                _idleTimer = null;
            }
        }
    }
}