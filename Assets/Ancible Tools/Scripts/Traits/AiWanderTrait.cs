using System.Collections.Generic;
using System.Linq;
using Assets.Resources.Ancible_Tools.Scripts.System;
using Assets.Resources.Ancible_Tools.Scripts.System.Pathing;
using MessageBusLib;
using UnityEngine;

namespace Assets.Ancible_Tools.Scripts.Traits
{
    [CreateAssetMenu(fileName = "Ai Wander Trait", menuName = "Ancible Tools/Traits/Ai/Ai Wander")]
    public class AiWanderTrait : Trait
    {
        [SerializeField] private int _wanderRange = 4;
        [SerializeField] private bool _diagonal = false;

        private MapTile _currentTile = null;
        private List<MapTile> _wanderPath = new List<MapTile>();
        private MonsterState _monsterState = MonsterState.Idle;

        public override void SetupController(TraitController controller)
        {
            base.SetupController(controller);
            SubscribeToMessages();
        }

        private void SubscribeToMessages()
        {
            _controller.gameObject.Subscribe<UpdateTickMessage>(UpdateTick);
            _controller.transform.parent.gameObject.SubscribeWithFilter<UpdateMapTileMessage>(UpdateMapTile, _instanceId);
            _controller.transform.parent.gameObject.SubscribeWithFilter<UpdateMonsterStateMessage>(UpdateMonsterState, _instanceId);
            _controller.transform.parent.gameObject.SubscribeWithFilter<ObstacleMessage>(Obstacle, _instanceId);
        }

        private void UpdateTick(UpdateTickMessage msg)
        {
            if (_monsterState == MonsterState.Idle && _currentTile != null && _wanderPath.Count <= 0)
            {
                var areaTiles = WorldController.Pathing.GetMapTilesInArea(_currentTile.Position, _wanderRange).Where(t => t != _currentTile).ToArray();
                if (areaTiles.Length > 0)
                {
                    var tile = areaTiles.Length > 1 ? areaTiles[Random.Range(0, areaTiles.Length)] : areaTiles[0];
                    var path = WorldController.Pathing.GetPath(_currentTile.Position, tile.Position, _diagonal);
                    if (path.Length > 0)
                    {
                        _wanderPath = path.ToList();
                        var setPathMsg = MessageFactory.GenerateSetPathMsg();
                        setPathMsg.Path = _wanderPath.ToArray();
                        _controller.gameObject.SendMessageTo(setPathMsg, _controller.transform.parent.gameObject);
                        MessageFactory.CacheMessage(setPathMsg);
                    }
                }
            }
        }

        private void UpdateMapTile(UpdateMapTileMessage msg)
        {
            _currentTile = msg.Tile;
            if (_wanderPath.Count > 0)
            {
                if (_wanderPath[0] == _currentTile)
                {
                    _wanderPath.RemoveAt(0);
                }
            }
        }

        private void UpdateMonsterState(UpdateMonsterStateMessage msg)
        {
            _monsterState = msg.State;
            if (_monsterState != MonsterState.Idle && _wanderPath.Count > 0)
            {
                _wanderPath.Clear();
            }
        }

        private void Obstacle(ObstacleMessage msg)
        {
            if (_monsterState == MonsterState.Idle && _wanderPath.Count > 0)
            {
                _wanderPath.Clear();
            }
        }
    }
    
}