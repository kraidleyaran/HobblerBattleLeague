using System.Collections.Generic;
using Assets.Resources.Ancible_Tools.Scripts.System;
using Assets.Resources.Ancible_Tools.Scripts.System.Adventure;
using Assets.Resources.Ancible_Tools.Scripts.System.Pathing;
using MessageBusLib;
using UnityEngine;

namespace Assets.Ancible_Tools.Scripts.Traits
{
    [CreateAssetMenu(fileName = "Adventure Ai Wander Trait", menuName = "Ancible Tools/Traits/Adventure/Adventure Ai Wander")]
    public class AdventureAiWanderTrait : Trait
    {
        [SerializeField] private int _area = 1;
        [SerializeField] private bool _isMonster = false;

        private MapTile _currentTile = null;

        private List<MapTile> _path = new List<MapTile>();

        private AdventureUnitState _unitState = AdventureUnitState.Idle;

        public override void SetupController(TraitController controller)
        {
            base.SetupController(controller);
            SubscribeToMessages();
        }

        private void SubscribeToMessages()
        {
            _controller.transform.parent.gameObject.SubscribeWithFilter<UpdateMapTileMessage>(UpdateMapTile, _instanceId);
            _controller.transform.parent.gameObject.SubscribeWithFilter<UpdateAdventureUnitStateMessage>(UpdateAdventureUnitState, _instanceId);
            _controller.transform.parent.gameObject.SubscribeWithFilter<ObstacleMessage>(Obstacle, _instanceId);
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
            if (_unitState == AdventureUnitState.Idle && _path.Count <= 0)
            {
                var pathingGrid = _isMonster ? WorldAdventureController.MapController.MonsterPathing : WorldAdventureController.MapController.PlayerPathing;
                var mapTiles = pathingGrid.GetMapTilesInArea(_currentTile.Position, _area);
                if (mapTiles.Length > 0)
                {
                    var tile = mapTiles.GetRandom();
                    var path = pathingGrid.GetPath(_currentTile.Position, tile.Position, false);
                    if (path.Length > 0)
                    {
                        var nextTile = path[0];

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

        private void UpdateAdventureUnitState(UpdateAdventureUnitStateMessage msg)
        {
            _unitState = msg.State;
        }

        private void Obstacle(ObstacleMessage msg)
        {
            _path.Clear();
            var setDirectionMsg = MessageFactory.GenerateSetDirectionMsg();
            setDirectionMsg.Direction = Vector2.zero;
            _controller.gameObject.SendMessageTo(setDirectionMsg, _controller.transform.parent.gameObject);
            MessageFactory.CacheMessage(setDirectionMsg);
        }


    }
}