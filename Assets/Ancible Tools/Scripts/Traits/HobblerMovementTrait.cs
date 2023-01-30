using System.Linq;
using Assets.Resources.Ancible_Tools.Scripts.System;
using Assets.Resources.Ancible_Tools.Scripts.System.Pathing;
using DG.Tweening;
using MessageBusLib;
using UnityEngine;

namespace Assets.Ancible_Tools.Scripts.Traits
{
    [CreateAssetMenu(fileName = "Hobbler Movement Trait", menuName = "Ancible Tools/Traits/Hobbler/Hobbler Movement")]
    public class HobblerMovementTrait : Trait
    {
        [SerializeField] private int _moveSpeed = 1;

        private MapTile _currentTile = null;

        private MapTile[] _path = new MapTile[0];
        private MonsterState _monsterState = MonsterState.Idle;
        private Rigidbody2D _rigidBody = null;
        private Tween _moveTween = null;
        private Vector2 _position = Vector2.zero;

        public override void SetupController(TraitController controller)
        {
            base.SetupController(controller);
            _rigidBody = _controller.transform.parent.gameObject.GetComponent<Rigidbody2D>();
            SubscribeToMessages();
        }

        private void SubscribeToMessages()
        {
            _controller.transform.parent.gameObject.SubscribeWithFilter<SetMapTileMessage>(SetMapTile, _instanceId);
            _controller.transform.parent.gameObject.SubscribeWithFilter<QueryMapTileMessage>(QueryMapTile, _instanceId);
            _controller.transform.parent.gameObject.SubscribeWithFilter<SetPathMessage>(SetPath, _instanceId);
            _controller.transform.parent.gameObject.SubscribeWithFilter<UpdateMonsterStateMessage>(UpdateMonsterState, _instanceId);
            _controller.transform.parent.gameObject.SubscribeWithFilter<SetDirectionMessage>(SetDirection, _instanceId);
        }

        private void UpdateTick(UpdateTickMessage msg)
        {
            if (_position != _rigidBody.position)
            {
                //TODO: Update position?
            }
        }

        private void SetPath(SetPathMessage msg)
        {
            if (_moveTween != null)
            {
                if (_moveTween.IsActive())
                {
                    _moveTween.Kill();
                }

                _moveTween = null;
            }
            _path = msg.Path.ToArray();
            if (_currentTile.World != _rigidBody.position)
            {
                //Move back to origin tile - this is to prevent odd looking pathing when a hobbler is between tiles
                var addPath = msg.Path.ToList();
                addPath.Insert(0, _currentTile);
                _path = addPath.ToArray();
            }
            var path = _path.Select(t => t.World).ToArray();
            if (path.Length > 0)
            {
                var moveSpeed = (_moveSpeed * DataController.Interpolation) / TickController.OneSecond;
                {
                    var direction = _path[0].Position - _currentTile.Position;
                    var updateDirectionMsg = MessageFactory.GenerateUpdateDirectionMsg();
                    updateDirectionMsg.Direction = direction;
                    _controller.gameObject.SendMessageTo(updateDirectionMsg, _controller.transform.parent.gameObject);
                    MessageFactory.CacheMessage(updateDirectionMsg);
                }
                _moveTween = _rigidBody.DOPath(path, moveSpeed).SetEase(Ease.Linear).SetSpeedBased(true).OnWaypointChange(
                    waypoint =>
                    {
                        if (waypoint < _path.Length)
                        {
                            var direction = _path[waypoint].Position - _currentTile.Position;
                            var updateDirectionMsg = MessageFactory.GenerateUpdateDirectionMsg();
                            updateDirectionMsg.Direction = direction;
                            _controller.gameObject.SendMessageTo(updateDirectionMsg, _controller.transform.parent.gameObject);
                            MessageFactory.CacheMessage(updateDirectionMsg);
                        }
                        if (waypoint > 0)
                        {
                            _currentTile = _path[waypoint - 1];
                            var updateMapTileMsg = MessageFactory.GenerateUpdateMapTileMsg();
                            updateMapTileMsg.Tile = _currentTile;
                            _controller.gameObject.SendMessageTo(updateMapTileMsg, _controller.transform.parent.gameObject);
                            MessageFactory.CacheMessage(updateMapTileMsg);

                            if (waypoint < _path.Length)
                            {
                                var nextTile = _path[waypoint];
                                if (nextTile.Block)
                                {
                                    var obstacleMsg = MessageFactory.GenerateObstacleMsg();
                                    obstacleMsg.Direction = (nextTile.Position - _currentTile.Position).Normalize();
                                    _controller.gameObject.SendMessageTo(obstacleMsg, _controller.transform.parent.gameObject);
                                    MessageFactory.CacheMessage(obstacleMsg);

                                    _path = new MapTile[0];
                                    if (_moveTween != null)
                                    {
                                        _moveTween.Kill();
                                        _moveTween = null;
                                    }
                                }
                            }
                        }


                    }).OnComplete(
                    () =>
                    {
                        _path = new MapTile[0];
                        _moveTween = null;
                    });
            }
            else
            {
                var updateMapTileMsg = MessageFactory.GenerateUpdateMapTileMsg();
                updateMapTileMsg.Tile = _currentTile;
                _controller.gameObject.SendMessageTo(updateMapTileMsg, _controller.transform.parent.gameObject);
                MessageFactory.CacheMessage(updateMapTileMsg);
            }
            
        }

        private void SetMapTile(SetMapTileMessage msg)
        {
            _currentTile = msg.Tile;
            var updateMapTileMsg = MessageFactory.GenerateUpdateMapTileMsg();
            updateMapTileMsg.Tile = _currentTile;
            _controller.gameObject.SendMessageTo(updateMapTileMsg, _controller.transform.parent.gameObject);
            MessageFactory.CacheMessage(updateMapTileMsg);
        }

        private void QueryMapTile(QueryMapTileMessage msg)
        {
            msg.DoAfter.Invoke(_currentTile);
        }

        private void UpdateMonsterState(UpdateMonsterStateMessage msg)
        {
            _monsterState = msg.State;
        }

        private void SetDirection(SetDirectionMessage msg)
        {
            var updateDirectionMsg = MessageFactory.GenerateUpdateDirectionMsg();
            updateDirectionMsg.Direction = msg.Direction;
            _controller.gameObject.SendMessageTo(updateDirectionMsg, _controller.transform.parent.gameObject);
            MessageFactory.CacheMessage(updateDirectionMsg);
        }

        public override void Destroy()
        {
            if (_moveTween != null)
            {
                if (_moveTween.IsActive())
                {
                    _moveTween.Kill();
                }

                _moveTween = null;
            }
            base.Destroy();
        }
    }
}