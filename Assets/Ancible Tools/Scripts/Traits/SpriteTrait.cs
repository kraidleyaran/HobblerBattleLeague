using Assets.Resources.Ancible_Tools.Scripts.System;
using Assets.Resources.Ancible_Tools.Scripts.System.Animation;
using DG.Tweening;
using MessageBusLib;
using UnityEngine;

namespace Assets.Ancible_Tools.Scripts.Traits
{
    [CreateAssetMenu(fileName = "Sprite Trait", menuName = "Ancible Tools/Traits/Animation/Sprite")]
    public class SpriteTrait : Trait
    {
        public Sprite Sprite => _sprite;
        public RuntimeAnimatorController RuntimeController => _runtimeAnimator;
        public Vector2 Scaling => _scaling;
        public SpriteLayer SpriteLayer => _spriteLayer;
        public int SortingOrder => _sortingOrder;
        public Vector2 Offset => _offset;
        public Color ColorMask => _colorMask;
        public float Rotation => _rotation;
        public bool FlipX => _flipX;
        public bool FlipY => _flipY;
        public FlipSprite FlipSprite => _flipSprite;

        [SerializeField] private Sprite _sprite;
        [SerializeField] private RuntimeAnimatorController _runtimeAnimator;
        [SerializeField] private Vector2 _scaling = new Vector2(31.25f, 31.25f);
        [SerializeField] private SpriteLayer _spriteLayer;
        [SerializeField] private int _sortingOrder = 0;
        [SerializeField] private Vector2 _offset = Vector2.zero;
        [SerializeField] private Color _colorMask = Color.white;
        [SerializeField] private float _rotation = 0f;
        [SerializeField] private bool _flipX;
        [SerializeField] private bool _flipY;
        [SerializeField] private FlipSprite _flipSprite = FlipSprite.None;
        [SerializeField] private float _bumpDistance = 1f;
        [SerializeField] private int _bumpTicks = 1;
        [SerializeField] private bool _animationStates = false;
        [SerializeField] private bool _overrideMapTileSorting = false;
        [SerializeField] private int _defaultJumpHeight = 1;
        [SerializeField] private int _defaultJumpSpeed = 1;

        protected internal SpriteController _spriteController = null;
        protected internal Tween _bumpTween = null;
        private SpriteTrait _currentSprite = null;
        protected internal Vector2Int _currentDirection = Vector2Int.zero;
        private UnitAnimationState _animationState = UnitAnimationState.Idle;
        protected internal Sequence _jumpSequence = null;

        public override void SetupController(TraitController controller)
        {
            base.SetupController(controller);
            _spriteController = Instantiate(FactoryController.SPRITE_CONTROLLER, _controller.transform.parent);
            _spriteController.gameObject.layer = _controller.transform.parent.gameObject.layer;
            if (controller.Prefab is SpriteTrait origin)
            {
                _currentSprite = origin;
            }
            else
            {
                _currentSprite = this;
            }
            SetupSprite(_currentSprite);
            SubscribeToMessages();
        }

        protected internal virtual void CancelAllTweens(bool finish = false)
        {
            if (_bumpTween != null)
            {
                if (_bumpTween.IsActive())
                {
                    _bumpTween.Kill(finish);
                }

                _bumpTween = null;
            }

            if (_jumpSequence != null)
            {
                if (_jumpSequence.IsActive())
                {
                    _jumpSequence.Kill(finish);
                }

                _jumpSequence = null;
            }
        }

        private void SetupSprite(SpriteTrait sprite)
        {
            if (_currentSprite && _currentSprite._animationStates)
            {
                _controller.transform.parent.gameObject.UnsubscribeFromFilter<SetUnitAnimationStateMessage>(_instanceId);
            }
            _currentSprite = sprite;
            if (sprite._runtimeAnimator)
            {
                _spriteController.SetRuntimeController(sprite._runtimeAnimator);
            }
            else
            {
                _spriteController.SetSprite(sprite._sprite);
            }
            _spriteController.SetScale(sprite._scaling);
            //_spriteController.SetSortingOrder(sprite._sortingOrder);
            if (sprite._spriteLayer)
            {
                _spriteController.SetSortingLayerFromSpriteLayer(sprite._spriteLayer);
            }
            _spriteController.SetOffset(sprite._offset);
            _spriteController.SetColorMask(sprite._colorMask);
            _spriteController.SetRotation(sprite._rotation);
            _spriteController.FlipX(sprite._flipX);
            _spriteController.FlipY(sprite._flipY);
            _flipSprite = sprite._flipSprite;
            if (_currentSprite._animationStates)
            {
                _controller.transform.parent.gameObject.SubscribeWithFilter<SetUnitAnimationStateMessage>(SetUnitAnimationState, _instanceId);
            }
        }

        protected internal virtual void SubscribeToMessages()
        {
            _controller.transform.parent.gameObject.SubscribeWithFilter<UpdateDirectionMessage>(UpdateDirection, _instanceId);
            _controller.transform.parent.gameObject.SubscribeWithFilter<QuerySpriteMessage>(QuerySprite, _instanceId);
            _controller.transform.parent.gameObject.SubscribeWithFilter<SetSpriteVisibilityMessage>(SetSpriteVisibility, _instanceId);
            _controller.transform.parent.gameObject.SubscribeWithFilter<DoBumpMessage>(DoBump, _instanceId);
            _controller.transform.parent.gameObject.SubscribeWithFilter<DoBumpOverPixelsPerSecondMessage>(DoBumpOverPixelsPerSecond, _instanceId);
            _controller.transform.parent.gameObject.SubscribeWithFilter<SetSpriteMessage>(SetSprite, _instanceId);
            _controller.transform.parent.gameObject.SubscribeWithFilter<SetFaceDirectionMessage>(SetFacingDirection, _instanceId);
            _controller.transform.parent.gameObject.SubscribeWithFilter<SetSpriteAlphaMessage>(SetSpriteAlpha, _instanceId);
            if (!_overrideMapTileSorting)
            {
                _controller.transform.parent.gameObject.SubscribeWithFilter<UpdateMapTileMessage>(UpdateMapTile, _instanceId);
            }
            _controller.transform.parent.gameObject.SubscribeWithFilter<DoJumpMessage>(DoJump, _instanceId);
        }

        protected internal virtual void UpdateDirection(UpdateDirectionMessage msg)
        {
            var direction = msg.Direction.ToVector2Int();
            if (direction != Vector2Int.zero && direction != _currentDirection)
            {
                _currentDirection = direction;
                if (_currentDirection.x > 0 || _currentDirection.x < 0)
                {
                    switch (_currentSprite._flipSprite)
                    {
                        case FlipSprite.None:
                            break;
                        case FlipSprite.Negative:
                            _spriteController.FlipX(_currentDirection.x < 0);
                            break;
                        case FlipSprite.Positive:
                            _spriteController.FlipX(_currentDirection.x > 0);
                            break;
                    }
                }
                _spriteController.SetDirection(_currentDirection);

                var updateFacingDirectionMsg = MessageFactory.GenerateUpdateFacingDirectionMsg();
                updateFacingDirectionMsg.Direction = _currentDirection;
                _controller.gameObject.SendMessageTo(updateFacingDirectionMsg, _controller.transform.parent.gameObject);
                MessageFactory.CacheMessage(updateFacingDirectionMsg);
            }

        }

        private void QuerySprite(QuerySpriteMessage msg)
        {
            msg.DoAfter.Invoke(_currentSprite);
        }

        private void DoBump(DoBumpMessage msg)
        {
            _bumpTween?.Kill();
            _bumpTween = null;
            var basePos = _offset;
            var bumpPos = (basePos + msg.Direction * _bumpDistance).ToPixelPerfect();
            var onBump = msg.OnBump;
            var doAfter = msg.DoAfter;
            _bumpTween = _spriteController.transform.DOLocalMove(bumpPos, _bumpTicks * TickController.TickRate).SetEase(Ease.Linear).OnComplete(
                () =>
                {
                    onBump?.Invoke();
                    _bumpTween = _spriteController.transform.DOLocalMove(basePos, _bumpTicks * TickController.TickRate).SetEase(Ease.Linear).OnComplete(
                        () =>
                        {
                            _bumpTween = null;
                            doAfter?.Invoke();
                        });
                });
        }

        private void DoBumpOverPixelsPerSecond(DoBumpOverPixelsPerSecondMessage msg)
        {
            _bumpTween?.Kill();
            _bumpTween = null;
            var basePos = _offset;
            var bumpPos = (basePos + msg.Direction * msg.Distance).ToPixelPerfect();
            var onBump = msg.OnBump;
            var doAfter = msg.DoAfter;
            //var distance = (bumpPos - basePos).magnitude.ToPixels();
            var time = (TickController.OneSecond / msg.PixelsPerSecond * msg.Distance);
            _bumpTween = _spriteController.transform.DOLocalMove(bumpPos, time / 2f).SetEase(Ease.Linear).OnComplete(
                () =>
                {
                    onBump?.Invoke();
                    _bumpTween = _spriteController.transform.DOLocalMove(basePos, time).SetEase(Ease.Linear).OnComplete(
                        () =>
                        {
                            _bumpTween = null;
                            doAfter?.Invoke();
                        });
                });
        }

        private void SetSpriteVisibility(SetSpriteVisibilityMessage msg)
        {
            _spriteController.gameObject.SetActive(msg.Visible);
        }

        private void SetSprite(SetSpriteMessage msg)
        {
            _currentSprite = msg.Sprite;
            SetupSprite(msg.Sprite);
        }

        protected internal virtual void SetFacingDirection(SetFaceDirectionMessage msg)
        {
            if (msg.Direction != _currentDirection)
            {
                _currentDirection = msg.Direction.ToVector2Int();
                if (_currentDirection.x > 0 || _currentDirection.x < 0)
                {
                    switch (_currentSprite._flipSprite)
                    {
                        case FlipSprite.None:
                            break;
                        case FlipSprite.Negative:
                            _spriteController.FlipX(_currentDirection.x < 0);
                            break;
                        case FlipSprite.Positive:
                            _spriteController.FlipX(_currentDirection.x > 0);
                            break;
                    }
                    
                }

                
                _spriteController.SetDirection(msg.Direction.ToStaticDirections());

                var updateFacingDirectionMsg = MessageFactory.GenerateUpdateFacingDirectionMsg();
                updateFacingDirectionMsg.Direction = _currentDirection;
                _controller.gameObject.SendMessageTo(updateFacingDirectionMsg, _controller.transform.parent.gameObject);
                MessageFactory.CacheMessage(updateFacingDirectionMsg);
            }
        }

        private void SetUnitAnimationState(SetUnitAnimationStateMessage msg)
        {
            if (_animationState != msg.State)
            {
                _animationState = msg.State;
                _spriteController.SetUnitAnimationState(_animationState);
            }
        }

        protected internal virtual void UpdateMapTile(UpdateMapTileMessage msg)
        {
            _spriteController.SetSortingOrder(msg.Tile.Position.y * -1 + _currentSprite.SortingOrder);
        }

        private void SetSpriteAlpha(SetSpriteAlphaMessage msg)
        {
            _spriteController.SetAlpha(msg.Alpha);
        }

        private void DoJump(DoJumpMessage msg)
        {
            CancelAllTweens(false);
            var distance = (_defaultJumpHeight * DataController.Interpolation);
            var time = TickController.OneSecond / (_defaultJumpSpeed * DataController.Interpolation) * distance;
            var doAfter = msg.DoAfter;
            _jumpSequence = _spriteController.transform.DOLocalJump(Offset, distance, 1, time).SetEase(Ease.Linear).OnComplete(
                () =>
                {
                    _jumpSequence = null; 
                    doAfter?.Invoke();
                });
        }

        public override void Destroy()
        {
            if (_bumpTween != null)
            {
                if (_bumpTween.IsActive())
                {
                    _bumpTween.Kill();
                }

                _bumpTween = null;
            }
            base.Destroy();
        }
    }
}