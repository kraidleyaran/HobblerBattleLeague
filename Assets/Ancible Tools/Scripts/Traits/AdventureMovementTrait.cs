using System.Linq;
using Assets.Resources.Ancible_Tools.Scripts.System;
using Assets.Resources.Ancible_Tools.Scripts.System.Adventure;
using Assets.Resources.Ancible_Tools.Scripts.System.Animation;
using Assets.Resources.Ancible_Tools.Scripts.System.Pathing;
using DG.Tweening;
using MessageBusLib;
using UnityEngine;

namespace Assets.Ancible_Tools.Scripts.Traits
{
    [CreateAssetMenu(fileName = "Adventure Movement Trait", menuName = "Ancible Tools/Traits/Adventure/Adventure Movement")]
    public class AdventureMovementTrait : Trait
    {
        [SerializeField] private bool _isMonster = false;
        [SerializeField] private int _moveSpeed = 0;
        [SerializeField] private float _pathMovementMultiplier = 16f;

        private Rigidbody2D _rigidBody = null;
        private Vector2 _position = Vector2.zero;

        private Vector2Int _direction = Vector2Int.zero;
        private MapTile _currentTile = null;

        private Tween _moveTween = null;
        private Tween _pathMoveTween = null;

        private AdventureUnitState _unitState = AdventureUnitState.Idle;

        public override void SetupController(TraitController controller)
        {
            base.SetupController(controller);
            _rigidBody = _controller.transform.parent.gameObject.GetComponent<Rigidbody2D>();
            SubscribeToMessages();
        }

        private void CompleteMovement()
        {
            //WorldAdventureController.MapController.RemoveBlockingTile(_controller.transform.parent.gameObject, _currentTile.Position);
            _moveTween = null;
            //_currentTile = nextTile;

            _controller.gameObject.SendMessageTo(ActivateGlobalCooldownMessage.INSTANCE, _controller.transform.parent.gameObject);
            if (_unitState == AdventureUnitState.Move)
            {
                var setAdventureUnitStateMsg = MessageFactory.GenerateSetAdventureUnitStateMsg();
                setAdventureUnitStateMsg.State = AdventureUnitState.Idle;
                _controller.gameObject.SendMessageTo(setAdventureUnitStateMsg, _controller.transform.parent.gameObject);
                MessageFactory.CacheMessage(setAdventureUnitStateMsg);
            }
            _controller.gameObject.SendMessageTo(MovementCompletedMessage.INSTANCE, _controller.transform.parent.gameObject);

            if (_direction == Vector2Int.zero)
            {
                var setUnitAnimationStateMsg = MessageFactory.GenerateSetUnitAnimationStateMsg();
                setUnitAnimationStateMsg.State = UnitAnimationState.Idle;
                _controller.gameObject.SendMessageTo(setUnitAnimationStateMsg, _controller.transform.parent.gameObject);
                MessageFactory.CacheMessage(setUnitAnimationStateMsg);
            }
            else
            {
                var pathingGrid = _isMonster ? WorldAdventureController.MapController.MonsterPathing : WorldAdventureController.MapController.PlayerPathing;
                var tileAfter = pathingGrid.GetTileByPosition(_currentTile.Position + _direction);
                if (tileAfter == null)
                {
                    var setUnitAnimationStateMsg = MessageFactory.GenerateSetUnitAnimationStateMsg();
                    setUnitAnimationStateMsg.State = UnitAnimationState.Idle;
                    _controller.gameObject.SendMessageTo(setUnitAnimationStateMsg, _controller.transform.parent.gameObject);
                    MessageFactory.CacheMessage(setUnitAnimationStateMsg);
                }
            }

            //var updateMapTileMsg = MessageFactory.GenerateUpdateMapTileMsg();
            //updateMapTileMsg.Tile = _currentTile;
            //_controller.gameObject.SendMessageTo(updateMapTileMsg, _controller.transform.parent.gameObject);
            //MessageFactory.CacheMessage(updateMapTileMsg);

            var updatePositionMsg = MessageFactory.GenerateUpdatePositionMsg();
            updatePositionMsg.Position = _rigidBody.position;
            _controller.gameObject.SendMessageTo(updatePositionMsg, _controller.transform.parent.gameObject);
            MessageFactory.CacheMessage(updatePositionMsg);


        }

        private void SubscribeToMessages()
        {
            _controller.transform.parent.gameObject.SubscribeWithFilter<SetMapTileMessage>(SetMapTile, _instanceId);
            _controller.transform.parent.gameObject.SubscribeWithFilter<SetDirectionMessage>(SetDirection, _instanceId);
            _controller.transform.parent.gameObject.SubscribeWithFilter<UpdateAdventureUnitStateMessage>(UpdateAdventureUnitState, _instanceId);
            _controller.transform.parent.gameObject.SubscribeWithFilter<QueryDirectionMessage>(QueryDirection, _instanceId);
            _controller.transform.parent.gameObject.SubscribeWithFilter<SetPathMessage>(SetPath, _instanceId);
            _controller.transform.parent.gameObject.SubscribeWithFilter<QueryMapTileMessage>(QueryMapTile, _instanceId);
        }

        private void SetMapTile(SetMapTileMessage msg)
        {
            if (_currentTile == null)
            {
                _controller.gameObject.Subscribe<UpdateTickMessage>(UpdateTick);
            }
            else if (_currentTile != null)
            {
                WorldAdventureController.MapController.RemoveBlockingTile(_controller.transform.parent.gameObject, _currentTile.Position);
            }

            _currentTile = msg.Tile;
            WorldAdventureController.MapController.SetBlockingTile(_controller.transform.parent.gameObject, _currentTile.Position);

            if (_moveTween != null)
            {
                if (_moveTween.IsActive())
                {
                    _moveTween.Kill();
                }

                _moveTween = null;
            }

            _rigidBody.position = _currentTile.World;

            var updateMapTileMsg = MessageFactory.GenerateUpdateMapTileMsg();
            updateMapTileMsg.Tile = _currentTile;
            _controller.gameObject.SendMessageTo(updateMapTileMsg, _controller.transform.parent.gameObject);
            MessageFactory.CacheMessage(updateMapTileMsg);

            //_controller.gameObject.SendMessageTo(MovementCompletedMessage.INSTANCE, _controller.transform.parent.gameObject);
            
        }

        private void UpdateTick(UpdateTickMessage msg)
        {
            if (WorldController.State == WorldState.Adventure && WorldAdventureController.State == AdventureState.Overworld && _unitState == AdventureUnitState.Idle && _moveTween == null && _direction != Vector2Int.zero)
            {
                var pathingGrid = _isMonster ? WorldAdventureController.MapController.MonsterPathing : WorldAdventureController.MapController.PlayerPathing;
                var nextTile = pathingGrid.GetTileByPosition(_currentTile.Position + _direction);
                if (nextTile != null)
                {
                    if (nextTile.Block)
                    {
                        var obstacleMsg = MessageFactory.GenerateObstacleMsg();
                        obstacleMsg.Direction = _direction;
                        obstacleMsg.Obstacle = nextTile.Block;
                        _controller.gameObject.SendMessageTo(obstacleMsg, _controller.transform.parent.gameObject);
                        MessageFactory.CacheMessage(obstacleMsg);

                        var setFaceDirectionMsg = MessageFactory.GenerateSetFaceDirectionMsg();
                        setFaceDirectionMsg.Direction = _direction;
                        _controller.gameObject.SendMessageTo(setFaceDirectionMsg, _controller.transform.parent.gameObject);
                        MessageFactory.CacheMessage(setFaceDirectionMsg);

                        var setUnitAnimationStateMsg = MessageFactory.GenerateSetUnitAnimationStateMsg();
                        setUnitAnimationStateMsg.State = UnitAnimationState.Idle;
                        _controller.gameObject.SendMessageTo(setUnitAnimationStateMsg, _controller.transform.parent.gameObject);
                        MessageFactory.CacheMessage(setUnitAnimationStateMsg);
                    }
                    else
                    {
                        
                        var setAdventureUnitStateMsg = MessageFactory.GenerateSetAdventureUnitStateMsg();
                        setAdventureUnitStateMsg.State = _unitState;
                        _controller.gameObject.SendMessageTo(setAdventureUnitStateMsg, _controller.transform.parent.gameObject);
                        MessageFactory.CacheMessage(setAdventureUnitStateMsg);

                        var setUnitAnimationStateMsg = MessageFactory.GenerateSetUnitAnimationStateMsg();
                        setUnitAnimationStateMsg.State = UnitAnimationState.Move;
                        _controller.gameObject.SendMessageTo(setUnitAnimationStateMsg, _controller.transform.parent.gameObject);
                        MessageFactory.CacheMessage(setUnitAnimationStateMsg);

                        var updateDirectionMsg = MessageFactory.GenerateUpdateDirectionMsg();
                        updateDirectionMsg.Direction = _direction;
                        _controller.gameObject.SendMessageTo(updateDirectionMsg, _controller.transform.parent.gameObject);
                        MessageFactory.CacheMessage(updateDirectionMsg);

                        var speed = _moveSpeed / TickController.OneSecond * DataController.Interpolation;
                        WorldAdventureController.MapController.RemoveBlockingTile(_controller.transform.parent.gameObject, _currentTile.Position);
                        WorldAdventureController.MapController.SetBlockingTile(_controller.transform.parent.gameObject, nextTile.Position);
                        _currentTile = nextTile;

                        var updateMapTileMsg = MessageFactory.GenerateUpdateMapTileMsg();
                        updateMapTileMsg.Tile = _currentTile;
                        _controller.gameObject.SendMessageTo(updateMapTileMsg, _controller.transform.parent.gameObject);
                        MessageFactory.CacheMessage(updateMapTileMsg);

                        _moveTween = _rigidBody.DOMove(nextTile.World, speed).SetEase(Ease.Linear).SetSpeedBased(true).OnComplete(CompleteMovement);
                        //_moveTween.OnKill(() =>
                        //{
                        //    if (_controller.gameObject && _currentTile != null && nextTile != _currentTile)
                        //    {
                        //        WorldAdventureController.MapController.RemoveBlockingTile(_controller.transform.parent.gameObject, nextTile.Position);
                        //    }
                            
                        //});
                    }
                }
                else
                {
                    var obstacleMsg = MessageFactory.GenerateObstacleMsg();
                    obstacleMsg.Direction = _direction;
                    obstacleMsg.Obstacle = WorldAdventureController.MapController.gameObject;
                    _controller.gameObject.SendMessageTo(obstacleMsg, _controller.transform.parent.gameObject);
                    MessageFactory.CacheMessage(obstacleMsg);

                    _direction = Vector2Int.zero;

                    var updateDirectionMsg = MessageFactory.GenerateUpdateDirectionMsg();
                    updateDirectionMsg.Direction = Vector2.zero;
                    _controller.gameObject.SendMessageTo(updateDirectionMsg, _controller.transform.parent.gameObject);
                    MessageFactory.CacheMessage(updateDirectionMsg);

                    var setUnitAnimationStateMsg = MessageFactory.GenerateSetUnitAnimationStateMsg();
                    setUnitAnimationStateMsg.State = UnitAnimationState.Idle;
                    _controller.gameObject.SendMessageTo(setUnitAnimationStateMsg, _controller.transform.parent.gameObject);
                    MessageFactory.CacheMessage(setUnitAnimationStateMsg);
                }
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

        private void UpdateAdventureUnitState(UpdateAdventureUnitStateMessage msg)
        {
            _unitState = msg.State;
            if (_unitState != AdventureUnitState.Move)
            {             
                var pathing = false;
                if (_pathMoveTween != null)
                {
                    pathing = true;
                    _pathMoveTween.Complete(true);
                }

                if (_moveTween != null)
                {
                    if (_moveTween.IsActive())
                    {
                        if (_unitState == AdventureUnitState.Interaction)
                        {
                            pathing = true;
                            _moveTween.Complete(true);
                        }
                        else
                        {
                            _moveTween.Kill();
                        }                        
                    }

                    _moveTween = null;
                    if (!pathing)
                    {
                        _rigidBody.position = _currentTile.World;
                    }
                }
                //_direction = Vector2Int.zero;
            }

            if (_unitState == AdventureUnitState.Disabled)
            {
                WorldAdventureController.MapController.RemoveBlockingTile(_controller.transform.parent.gameObject, _currentTile.Position);
            }
        }

        private void SetDirection(SetDirectionMessage msg)
        {
            _direction = msg.Direction.ToVector2Int();
        }


        private void QueryDirection(QueryDirectionMessage msg)
        {
            msg.DoAfter.Invoke(_direction);
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
            var endTile = msg.Path[msg.Path.Length - 1];
            var time = _pathMovementMultiplier * msg.Path.Length / (_moveSpeed / TickController.OneSecond);
            var path = msg.Path.Select(t => t.World).ToArray();
            if (_unitState != AdventureUnitState.Move)
            {
                var setUnitStateMsg = MessageFactory.GenerateSetAdventureUnitStateMsg();
                setUnitStateMsg.State = AdventureUnitState.Move;
                _controller.gameObject.SendMessageTo(setUnitStateMsg, _controller.transform.parent.gameObject);
                MessageFactory.CacheMessage(setUnitStateMsg);
            }

            var doAfter = msg.DoAfter;
            _pathMoveTween = _rigidBody.DOPath(path, time).SetEase(Ease.Linear).OnComplete(() =>
                {
                    _pathMoveTween = null;
                    WorldAdventureController.MapController.RemoveBlockingTile(_controller.transform.parent.gameObject, _currentTile.Position);
                    _currentTile = endTile;
                    WorldAdventureController.MapController.SetBlockingTile(_controller.transform.parent.gameObject, _currentTile.Position);

                    var updateMapTileMsg = MessageFactory.GenerateUpdateMapTileMsg();
                    updateMapTileMsg.Tile = _currentTile;
                    _controller.gameObject.SendMessageTo(updateMapTileMsg, _controller.transform.parent.gameObject);
                    MessageFactory.CacheMessage(updateMapTileMsg);

                    var setUnitStateMsg = MessageFactory.GenerateSetAdventureUnitStateMsg();
                    setUnitStateMsg.State = AdventureUnitState.Idle;
                    _controller.gameObject.SendMessageTo(setUnitStateMsg, _controller.transform.parent.gameObject);
                    MessageFactory.CacheMessage(setUnitStateMsg);
                    
                    doAfter?.Invoke();
                });
        }

        private void QueryMapTile(QueryMapTileMessage msg)
        {
            msg.DoAfter.Invoke(_currentTile);
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

            if (_pathMoveTween != null)
            {
                if (_pathMoveTween.IsActive())
                {
                    _pathMoveTween.Kill();
                }

                _pathMoveTween = null;
            }

            if (_currentTile != null)
            {
                WorldAdventureController.MapController?.RemoveBlockingTile(_controller.transform.parent.gameObject, _currentTile.Position);
            }
            base.Destroy();
        }
    }
}