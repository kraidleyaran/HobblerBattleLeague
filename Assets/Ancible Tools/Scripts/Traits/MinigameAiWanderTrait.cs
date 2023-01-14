using System.Collections.Generic;
using System.Linq;
using Assets.Resources.Ancible_Tools.Scripts.System;
using Assets.Resources.Ancible_Tools.Scripts.System.Minigame;
using Assets.Resources.Ancible_Tools.Scripts.System.Minigame.AI;
using Assets.Resources.Ancible_Tools.Scripts.System.Pathing;
using MessageBusLib;
using UnityEngine;

namespace Assets.Ancible_Tools.Scripts.Traits
{
    [CreateAssetMenu(fileName = "Minigame Ai Wander Trait", menuName = "Ancible Tools/Traits/Minigame/Ai/Minigame Ai Wander")]
    public class MinigameAiWanderTrait : Trait
    {
        [SerializeField] private int _wanderArea = 1;
        [SerializeField] private bool _anchor = true;
        [SerializeField] private bool _diagonal = false;

        private MapTile _anchorTile = null;
        private MapTile[] _anchorArea = new MapTile[0];

        private MapTile _currentTile = null;

        private List<MapTile> _currentPath = new List<MapTile>();

        private MinigameAiState _minigameAiState = MinigameAiState.Wander;
        private MinigameUnitState _unitState = MinigameUnitState.Idle;

        public override void SetupController(TraitController controller)
        {
            base.SetupController(controller);
            SubscribeToMessages();
        }

        private void SubscribeToMessages()
        {
            _controller.transform.parent.gameObject.SubscribeWithFilter<UpdateMapTileMessage>(UpdateMapTile, _instanceId);
            _controller.transform.parent.gameObject.SubscribeWithFilter<UpdateMinigameAiStateMessage>(UpdateMinigameAiState, _instanceId);
            _controller.transform.parent.gameObject.SubscribeWithFilter<UpdateMinigameUnitStateMessage>(UpdateMinigameUnitState, _instanceId);
            _controller.transform.parent.gameObject.SubscribeWithFilter<ObstacleMessage>(Obstacle, _instanceId);
        }

        private void UpdateTick(UpdateTickMessage msg)
        {
            if (_minigameAiState == MinigameAiState.Wander && _currentPath.Count <= 0 && _unitState == MinigameUnitState.Idle)
            {
                var originTile = _anchor ? _anchorTile : _currentTile;
                var pathableTiles = MinigameController.Pathing.GetTilesInPov(originTile.Position, _wanderArea);
                if (pathableTiles.Length > 0)
                {
                    var tile = pathableTiles.Length > 1 ? pathableTiles[Random.Range(0, pathableTiles.Length)] : pathableTiles[0];
                    var path = MinigameController.Pathing.GetPath(_currentTile.Position, tile.Position, _diagonal);
                    if (path.Length > 0)
                    {
                        _currentPath = path.ToList();
                        var direction = (path[0].Position - _currentTile.Position).Normalize();
                        var setDirectionMsg = MessageFactory.GenerateSetDirectionMsg();
                        setDirectionMsg.Direction = direction;
                        _controller.gameObject.SendMessageTo(setDirectionMsg, _controller.transform.parent.gameObject);
                        MessageFactory.CacheMessage(setDirectionMsg);
                    }

                }
                
            }
        }

        private void UpdateMapTile(UpdateMapTileMessage msg)
        {
            if (_currentTile == null)
            {
                _controller.gameObject.Subscribe<UpdateTickMessage>(UpdateTick);
            }

            if (_anchor && _anchorTile == null)
            {
                _anchorTile = msg.Tile;
            }
            _currentTile = msg.Tile;
            if (_currentPath.Count > 0)
            {
                if (_currentPath[0] == _currentTile)
                {
                    _currentPath.RemoveAt(0);
                    if (_currentPath.Count > 0)
                    {
                        var direction = (_currentPath[0].Position - _currentTile.Position).Normalize();
                        var setDirectionMsg = MessageFactory.GenerateSetDirectionMsg();
                        setDirectionMsg.Direction = direction;
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
        }

        private void UpdateMinigameAiState(UpdateMinigameAiStateMessage msg)
        {
            if (msg.State != MinigameAiState.Wander && _currentPath.Count > 0)
            {
                _currentPath.Clear();
            }
            _minigameAiState = msg.State;
        }

        private void UpdateMinigameUnitState(UpdateMinigameUnitStateMessage msg)
        {
            _unitState = msg.State;
        }

        private void Obstacle(ObstacleMessage msg)
        {
            if (_minigameAiState == MinigameAiState.Wander)
            {
                _currentPath.Clear();
                var setDirectionMsg = MessageFactory.GenerateSetDirectionMsg();
                setDirectionMsg.Direction = Vector2.zero;
                _controller.gameObject.SendMessageTo(setDirectionMsg, _controller.transform.parent.gameObject);
                MessageFactory.CacheMessage(setDirectionMsg);
            }
        }

    }
}