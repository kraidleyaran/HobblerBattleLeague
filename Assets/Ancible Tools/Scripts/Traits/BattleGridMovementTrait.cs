using Assets.Resources.Ancible_Tools.Scripts.System;
using Assets.Resources.Ancible_Tools.Scripts.System.BattleLeague;
using Assets.Resources.Ancible_Tools.Scripts.System.Pathing;
using DG.Tweening;
using MessageBusLib;
using UnityEngine;

namespace Assets.Ancible_Tools.Scripts.Traits
{
    [CreateAssetMenu(fileName = "Battle Grid Movement Trait", menuName = "Ancible Tools/Traits/Battle/Battle Grid Movement")]
    public class BattleGridMovementTrait : Trait
    {
        [SerializeField] private int _defaultMovementSpeed = 0;

        private int _moveSpeed = 0;
        private MapTile _currentTile = null;
        private Vector2Int _direction = Vector2Int.zero;
        private Rigidbody2D _rigidBody = null;
        private UnitBattleState _battleState = UnitBattleState.Active;

        private Tween _moveTween = null;

        public override void SetupController(TraitController controller)
        {
            base.SetupController(controller);
            _rigidBody = _controller.transform.parent.gameObject.GetComponent<Rigidbody2D>();
            _moveSpeed = _defaultMovementSpeed;
            SubscribeToMessages();
        }

        private void MoveToTile(MapTile tile)
        {
            if (_moveTween != null)
            {
                if (_moveTween.IsActive())
                {
                    _moveTween.Kill();
                }

                _moveTween = null;
            }

            var distance = (tile.World - _rigidBody.position).magnitude;
            var moveSpeed = TickController.OneSecond / (_moveSpeed * DataController.Interpolation) * distance;
            BattleLeagueController.PathingGrid.SetTileBlock(_controller.transform.parent.gameObject,tile.Position);

            var updateDirectionMsg = MessageFactory.GenerateUpdateDirectionMsg();
            updateDirectionMsg.Direction = _direction;
            _controller.gameObject.SendMessageTo(updateDirectionMsg, _controller.transform.parent.gameObject);
            MessageFactory.CacheMessage(updateDirectionMsg);

            {
                var setUnitBattleStateMsg = MessageFactory.GenerateSetUnitBattleStateMsg();
                setUnitBattleStateMsg.State = UnitBattleState.Move;
                _controller.gameObject.SendMessageTo(setUnitBattleStateMsg, _controller.transform.parent.gameObject);
                MessageFactory.CacheMessage(setUnitBattleStateMsg);
            }

            _moveTween = _controller.transform.parent.DOJump(tile.World, 5f, 1, moveSpeed).SetSpeedBased(true).SetEase(Ease.Linear).OnComplete(
                () =>
                {
                    _moveTween = null;
                    if (_currentTile != null)
                    {
                        BattleLeagueController.PathingGrid.RemoveTileBlock(_controller.transform.parent.gameObject, _currentTile.Position);
                    }
                    _currentTile = tile;
                    _controller.gameObject.SendMessageTo(ActivateGlobalCooldownMessage.INSTANCE, _controller.transform.parent.gameObject);

                    var updateMapTileMsg = MessageFactory.GenerateUpdateMapTileMsg();
                    updateMapTileMsg.Tile = _currentTile;
                    _controller.gameObject.SendMessageTo(updateMapTileMsg, _controller.transform.parent.gameObject);
                    MessageFactory.CacheMessage(updateMapTileMsg);

                    var updatePositionMsg = MessageFactory.GenerateUpdatePositionMsg();
                    updatePositionMsg.Position = _currentTile.World;
                    _controller.gameObject.SendMessageTo(updatePositionMsg, _controller.transform.parent.gameObject);
                    MessageFactory.CacheMessage(updatePositionMsg);
                });
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
                _rigidBody.position = _currentTile.World;
            }
        }

        private void SubscribeToMessages()
        {
            _controller.transform.parent.gameObject.SubscribeWithFilter<UpdateTickMessage>(UpdateTick, _instanceId);
            _controller.transform.parent.gameObject.SubscribeWithFilter<SetMapTileMessage>(SetMapTile, _instanceId);
            _controller.transform.parent.gameObject.SubscribeWithFilter<QueryMapTileMessage>(QueryMapTile, _instanceId);
            _controller.transform.parent.gameObject.SubscribeWithFilter<SetDirectionMessage>(SetDirection, _instanceId);
            _controller.transform.parent.gameObject.SubscribeWithFilter<QueryDirectionMessage>(QueryDirection, _instanceId);
            _controller.transform.parent.gameObject.SubscribeWithFilter<UpdateUnitBattleStateMessage>(UpdateBattleUnitState, _instanceId);
            _controller.transform.parent.gameObject.SubscribeWithFilter<DoVictoryAnimationMessage>(DoVictoryAnimation, _instanceId);
            _controller.transform.parent.gameObject.SubscribeWithFilter<StunMessage>(Stun, _instanceId);
            _controller.transform.parent.gameObject.SubscribeWithFilter<RootMessage>(Root, _instanceId);
        }

        private void UpdateTick(UpdateTickMessage msg)
        {
            if (_moveTween == null && _battleState == UnitBattleState.Active)
            {
                _controller.gameObject.SendMessageTo(BattleAggroCheckMessage.INSTANCE, _controller.transform.parent.gameObject);
                if (_battleState == UnitBattleState.Active && _direction != Vector2Int.zero)
                {
                    var canMove = true;
                    var moveCheckMsg = MessageFactory.GenerateCanMoveCheckMsg();
                    moveCheckMsg.DoAfter = () => canMove = false;
                    _controller.gameObject.SendMessageTo(moveCheckMsg, _controller.transform.parent.gameObject);
                    MessageFactory.CacheMessage(moveCheckMsg);
                    if (canMove)
                    {
                        var nextTile = BattleLeagueController.PathingGrid.GetTileByPosition(_currentTile.Position + _direction);
                        if (nextTile != null && (!nextTile.Block || nextTile.Block == _controller.transform.parent.gameObject))
                        {
                            MoveToTile(nextTile);
                        }
                        else
                        {
                            if (nextTile != null && nextTile.Block)
                            {
                                var obstacleMsg = MessageFactory.GenerateObstacleMsg();
                                obstacleMsg.Direction = _direction;
                                obstacleMsg.Obstacle = nextTile.Block;
                                _controller.gameObject.SendMessageTo(obstacleMsg, _controller.transform.parent.gameObject);
                                MessageFactory.CacheMessage(obstacleMsg);
                            }
                            _direction = Vector2Int.zero;
                        }
                    }
                }

            }
        }

        private void SetMapTile(SetMapTileMessage msg)
        {
            _currentTile = msg.Tile;
            BattleLeagueController.PathingGrid.SetTileBlock(_controller.transform.parent.gameObject, _currentTile.Position);
            _rigidBody.position = _currentTile.World;

            var updateMapTileMsg = MessageFactory.GenerateUpdateMapTileMsg();
            updateMapTileMsg.Tile = _currentTile;
            _controller.gameObject.SendMessageTo(updateMapTileMsg, _controller.transform.parent.gameObject);
            MessageFactory.CacheMessage(updateMapTileMsg);
        }

        private void QueryMapTile(QueryMapTileMessage msg)
        {
            msg.DoAfter.Invoke(_currentTile);
        }

        private void SetDirection(SetDirectionMessage msg)
        {
            _direction = msg.Direction.ToVector2Int();
        }

        private void QueryDirection(QueryDirectionMessage msg)
        {
            msg.DoAfter.Invoke(_direction);
        }

        private void UpdateBattleUnitState(UpdateUnitBattleStateMessage msg)
        {
            _battleState = msg.State;
            if (_battleState == UnitBattleState.Dead)
            {
                if (_moveTween != null)
                {
                    if (_moveTween.IsActive())
                    {
                        _moveTween.Kill();
                    }
                }
                _moveTween = null;
                BattleLeagueController.PathingGrid.RemoveTileBlock(_controller.transform.parent.gameObject, _currentTile.Position);
            }
            else if (_battleState == UnitBattleState.End)
            {
                if (_moveTween != null)
                {
                    if (_moveTween.IsActive())
                    {
                        _moveTween.Complete(true);
                    }

                    _moveTween = null;
                }
                _controller.gameObject.Unsubscribe<UpdateTickMessage>();
            }
        }

        private void Stun(StunMessage msg)
        {
            InterruptMovement();
        }

        private void Root(RootMessage msg)
        {
            InterruptMovement();
        }

        private void DoVictoryAnimation(DoVictoryAnimationMessage msg)
        {
            if (_moveTween != null)
            {
                if (_moveTween.IsActive())
                {
                    _moveTween.Complete(true);
                }
            }
        }

        public override void Destroy()
        {
            if (_moveTween != null)
            {
                if (_moveTween.IsActive())
                {
                    _moveTween.Kill();
                }
            }

            _moveTween = null;
            base.Destroy();
        }
    }
}