using Assets.Resources.Ancible_Tools.Scripts.System;
using Assets.Resources.Ancible_Tools.Scripts.System.Minigame;
using Assets.Resources.Ancible_Tools.Scripts.System.Pathing;
using DG.Tweening;
using MessageBusLib;
using UnityEngine;

namespace Assets.Ancible_Tools.Scripts.Traits
{
    [CreateAssetMenu(fileName = "Minigame Grid Movement Trait", menuName = "Ancible Tools/Traits/Minigame/Movement/Minigame Grid Movement")]
    public class MinigameGridMovementTrait : Trait
    {
        [SerializeField] private int _moveSpeed = 1;
        [SerializeField] private bool _blockTile = false;
        [SerializeField] private int _movementCooldown = 0;
        [SerializeField] private bool _visibleInFog = true;
        [SerializeField] private bool _activateGlobalCooldown = false;

        private MapTile _mapTile = null;
        private Vector2Int _direction = Vector2Int.zero;
        private MinigameUnitState _unitState = MinigameUnitState.Idle;
        private Tween _moveTween = null;
        private Rigidbody2D _rigidBody = null;
        private Vector2 _position = Vector2.zero;
        private Sequence _cooldownSequence = null;
        

        public override void SetupController(TraitController controller)
        {
            base.SetupController(controller);
            _rigidBody = _controller.transform.parent.gameObject.GetComponent<Rigidbody2D>();
            _position = _rigidBody.position;
            SubscribeToMessages();
        }

        private void MoveCheck()
        {
            var canMove = true;
            var moveCheckMsg = MessageFactory.GenerateCanMoveCheckMsg();
            moveCheckMsg.DoAfter = () => canMove = false;
            _controller.gameObject.SendMessageTo(moveCheckMsg, _controller.transform.parent.gameObject);
            MessageFactory.CacheMessage(moveCheckMsg);
            if (canMove)
            {
                var nextTile = MinigameController.Pathing.GetTileByPosition(_direction + _mapTile.Position);
                if (nextTile != null)
                {
                    if (nextTile.Block && nextTile.Block != _controller.transform.parent.gameObject)
                    {
                        if (_unitState == MinigameUnitState.Move)
                        {
                            var setMinigameUnitStateMsg = MessageFactory.GenerateSetMinigameUnitStateMsg();
                            setMinigameUnitStateMsg.State = MinigameUnitState.Idle;
                            _controller.gameObject.SendMessageTo(setMinigameUnitStateMsg, _controller.transform.parent.gameObject);
                            MessageFactory.CacheMessage(setMinigameUnitStateMsg);
                        }
                        var obstacleMsg = MessageFactory.GenerateObstacleMsg();
                        obstacleMsg.Obstacle = nextTile.Block;
                        obstacleMsg.Direction = _direction;
                        _controller.gameObject.SendMessageTo(obstacleMsg, _controller.transform.parent.gameObject);
                        MessageFactory.CacheMessage(obstacleMsg);
                    }
                    else if (_cooldownSequence == null)
                    {
                        {
                            var setMinigameUnitStateMsg = MessageFactory.GenerateSetMinigameUnitStateMsg();
                            setMinigameUnitStateMsg.State = MinigameUnitState.Move;
                            _controller.gameObject.SendMessageTo(setMinigameUnitStateMsg, _controller.transform.parent.gameObject);
                            MessageFactory.CacheMessage(setMinigameUnitStateMsg);
                        }
                        var moveSpeed = (_moveSpeed * DataController.Interpolation) / TickController.OneSecond;
                        if (_blockTile)
                        {
                            MinigameController.Pathing.SetTileBlock(_controller.transform.parent.gameObject, nextTile.Position);
                        }
                        var updateDirectionMsg = MessageFactory.GenerateUpdateDirectionMsg();
                        updateDirectionMsg.Direction = _direction;
                        _controller.gameObject.SendMessageTo(updateDirectionMsg, _controller.transform.parent.gameObject);
                        MessageFactory.CacheMessage(updateDirectionMsg);

                        _moveTween = _rigidBody.DOMove(nextTile.World, moveSpeed).SetSpeedBased(true).SetEase(Ease.Linear).OnComplete(() =>
                        {
                            _moveTween = null;
                            if (_movementCooldown > 0)
                            {
                                _cooldownSequence = DOTween.Sequence().AppendInterval(TickController.TickRate * _movementCooldown).OnComplete(
                                    () =>
                                    {
                                        _cooldownSequence = null;
                                    });
                            }
                            if (_blockTile)
                            {
                                MinigameController.Pathing.RemoveTileBlock(_controller.transform.parent.gameObject, _mapTile.Position);
                            }

                            if (_activateGlobalCooldown)
                            {
                                _controller.gameObject.SendMessageTo(ActivateGlobalCooldownMessage.INSTANCE, _controller.transform.parent.gameObject);
                            }
                            _mapTile = nextTile;
                            _mapTile.ApplyEvent(_controller.transform.parent.gameObject);
                            var updateMapTileMsg = MessageFactory.GenerateUpdateMapTileMsg();
                            updateMapTileMsg.Tile = _mapTile;
                            _controller.gameObject.SendMessageTo(updateMapTileMsg, _controller.transform.parent.gameObject);
                            MessageFactory.CacheMessage(updateMapTileMsg);

                            var updatePositionMsg = MessageFactory.GenerateUpdatePositionMsg();
                            updatePositionMsg.Position = _position;
                            _controller.gameObject.SendMessageTo(updatePositionMsg, _controller.transform.parent.gameObject);
                            MessageFactory.CacheMessage(updatePositionMsg);
                            //Extra direction check - if the player has a direction held down, this will continue moving them without pause
                            if (_cooldownSequence == null && (_unitState == MinigameUnitState.Idle || _unitState == MinigameUnitState.Move) && _direction != Vector2Int.zero)
                            {
                                MoveCheck();
                            }
                            else if (_unitState != MinigameUnitState.Disabled && _unitState != MinigameUnitState.Interact)
                            {
                                var setMinigameUnitStateMsg = MessageFactory.GenerateSetMinigameUnitStateMsg();
                                setMinigameUnitStateMsg.State = MinigameUnitState.Idle;
                                _controller.gameObject.SendMessageTo(setMinigameUnitStateMsg, _controller.transform.parent.gameObject);
                                MessageFactory.CacheMessage(setMinigameUnitStateMsg);
                            }
                        });
                    }

                }
            }
        }

        private void InterruptMovement()
        {
            if (_moveTween != null)
            {
                if (_moveTween.IsActive())
                {
                    _moveTween.Kill();
                }

                _moveTween = null;
                _rigidBody.position = _mapTile.World;
            }

            if (_unitState == MinigameUnitState.Move)
            {
                var setUnitStateMsg = MessageFactory.GenerateSetMinigameUnitStateMsg();
                setUnitStateMsg.State = MinigameUnitState.Idle;
                _controller.gameObject.SendMessageTo(setUnitStateMsg, _controller.transform.parent.gameObject);
                MessageFactory.CacheMessage(setUnitStateMsg);
            }
        }

        private void SubscribeToMessages()
        {
            _controller.gameObject.Subscribe<UpdateTickMessage>(UpdateTick);
            _controller.transform.parent.gameObject.SubscribeWithFilter<SetDirectionMessage>(SetDirection, _instanceId);
            _controller.transform.parent.gameObject.SubscribeWithFilter<QueryDirectionMessage>(QueryDirection, _instanceId);
            _controller.transform.parent.gameObject.SubscribeWithFilter<SetMapTileMessage>(SetMapTile, _instanceId);
            _controller.transform.parent.gameObject.SubscribeWithFilter<QueryMapTileMessage>(QueryMapTile, _instanceId);
            _controller.transform.parent.gameObject.SubscribeWithFilter<UpdateMinigameUnitStateMessage>(UpdateMinigameUnitState, _instanceId);
            _controller.transform.parent.gameObject.SubscribeWithFilter<StunMessage>(Stun, _instanceId);
            _controller.transform.parent.gameObject.SubscribeWithFilter<RootMessage>(Root, _instanceId);
        }

        private void UpdateTick(UpdateTickMessage msg)
        {
            if (_direction != Vector2Int.zero && _moveTween == null && (_unitState == MinigameUnitState.Idle || _unitState == MinigameUnitState.Move))
            {
                MoveCheck();
            }
            if (_position != _rigidBody.position)
            {
                _position = _rigidBody.position;
                var updatePositionMsg = MessageFactory.GenerateUpdatePositionMsg();
                updatePositionMsg.Position = _position;
                _controller.gameObject.SendMessageTo(updatePositionMsg, _controller.transform.parent.gameObject);
                MessageFactory.CacheMessage(updatePositionMsg);
            }
        }

        private void SetDirection(SetDirectionMessage msg)
        {
            if (_direction != msg.Direction)
            {
                _direction = msg.Direction.ToVector2Int();
                if (_unitState == MinigameUnitState.Idle)
                {
                    var updateDirectionMsg = MessageFactory.GenerateUpdateDirectionMsg();
                    updateDirectionMsg.Direction = _direction;
                    _controller.gameObject.SendMessageTo(updateDirectionMsg, _controller.transform.parent.gameObject);
                    MessageFactory.CacheMessage(updateDirectionMsg);
                }
            }
        }

        private void QueryDirection(QueryDirectionMessage msg)
        {
            msg.DoAfter.Invoke(_direction);
        }

        private void UpdateMinigameUnitState(UpdateMinigameUnitStateMessage msg)
        {
            var prevState = _unitState;
            _unitState = msg.State;
            if (_unitState != MinigameUnitState.Move && _moveTween != null)
            {
                if (_moveTween.IsActive())
                {
                    _moveTween.Kill(true);
                }

                _moveTween = null;
                _rigidBody.position = _mapTile.World;
            }

            if (_blockTile)
            {
                if (_unitState == MinigameUnitState.Disabled)
                {
                    MinigameController.Pathing.RemoveTileBlock(_controller.transform.parent.gameObject, _mapTile.Position);
                }
                else if (prevState == MinigameUnitState.Disabled)
                {
                    MinigameController.Pathing.SetTileBlock(_controller.transform.parent.gameObject, _mapTile.Position);
                }
            }

            if (prevState != MinigameUnitState.Move && _unitState == MinigameUnitState.Idle)
            {
                var updateDirectionMsg = MessageFactory.GenerateUpdateDirectionMsg();
                updateDirectionMsg.Direction = _direction;
                _controller.gameObject.SendMessageTo(updateDirectionMsg, _controller.transform.parent.gameObject);
                MessageFactory.CacheMessage(updateDirectionMsg);
            }

        }

        private void SetMapTile(SetMapTileMessage msg)
        {
            if (_mapTile != null && _blockTile)
            {
                MinigameController.Pathing.RemoveTileBlock(_controller.transform.parent.gameObject, _mapTile.Position);
            }
            _mapTile = msg.Tile;
            if (_blockTile)
            {
                MinigameController.Pathing.SetTileBlock(_controller.transform.parent.gameObject, _mapTile.Position);
            }
            if (_moveTween != null)
            {
                if (_moveTween.IsActive())
                {
                    _moveTween.Kill();
                }

                _moveTween = null;
            }
            
            _rigidBody.position = _mapTile.World;
            _position = _rigidBody.position;

            var updatePositionMsg = MessageFactory.GenerateUpdatePositionMsg();
            updatePositionMsg.Position = _position;
            _controller.gameObject.SendMessageTo(updatePositionMsg, _controller.transform.parent.gameObject);
            MessageFactory.CacheMessage(updatePositionMsg);

            var updateMapTileMsg = MessageFactory.GenerateUpdateMapTileMsg();
            updateMapTileMsg.Tile = _mapTile;
            _controller.gameObject.SendMessageTo(updateMapTileMsg, _controller.transform.parent.gameObject);
            MessageFactory.CacheMessage(updateMapTileMsg);
        }

        private void QueryMapTile(QueryMapTileMessage msg)
        {
            msg.DoAfter.Invoke(_mapTile);
        }

        private void Stun(StunMessage msg)
        {
            InterruptMovement();
        }

        private void Root(RootMessage msg)
        {
            InterruptMovement();
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