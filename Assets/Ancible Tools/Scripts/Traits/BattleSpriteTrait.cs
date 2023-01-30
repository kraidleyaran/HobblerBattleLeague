using Assets.Resources.Ancible_Tools.Scripts.System;
using Assets.Resources.Ancible_Tools.Scripts.System.Animation;
using Assets.Resources.Ancible_Tools.Scripts.System.BattleLeague;
using DG.Tweening;
using MessageBusLib;
using UnityEngine;

namespace Assets.Ancible_Tools.Scripts.Traits
{
    [CreateAssetMenu(fileName = "Battle Sprite Trait", menuName = "Ancible Tools/Traits/Animation/Battle Sprite")]
    public class BattleSpriteTrait : SpriteTrait
    {
        [SerializeField] private Vector2 _flagOffset = Vector2.zero;
        [SerializeField] private int _victoryJumpHeight = 1;
        [SerializeField] private int _victoryJumpSpeed = 1;
        [SerializeField] private int _victoryJumpCount = 1;
        [SerializeField] private IntNumberRange _victoryAnimationDelay = IntNumberRange.One;
        [SerializeField] private int _framesBetweenJumps = 1;

        private BattleLeagueAlignmentController _alignmentController = null;
        private BattleAlignment _alignment = BattleAlignment.None;

        private Sequence _jumpSequence = null;
        private Sequence _waitSequence = null;
        private Coroutine _delayRoutine = null;

        public override void SetupController(TraitController controller)
        {
            base.SetupController(controller);
            _alignmentController = Instantiate(FactoryController.ALIGNMENT_CONTROLLER, _spriteController.transform);
            _alignmentController.transform.SetLocalPosition(_flagOffset / Scaling.x);
        }

        private void DoJump()
        {
            if (_jumpSequence != null)
            {
                if (_jumpSequence.IsActive())
                {
                    _jumpSequence.Kill();
                }

                _jumpSequence = null;
            }

            var distance = (_victoryJumpHeight * DataController.Interpolation);
            var moveSpeed = TickController.OneSecond / (_victoryJumpSpeed * DataController.Interpolation) * distance;
            _jumpSequence = _spriteController.transform.DOLocalJump(Offset, distance, 1, moveSpeed).SetEase(Ease.Linear).OnComplete(() =>
                {
                    _jumpSequence = null;
                    Wait();
                });
        }

        private void Wait()
        {
            if (_waitSequence != null)
            {
                if (_waitSequence.IsActive())
                {
                    _waitSequence.Kill();
                }

                _waitSequence = null;
            }

            _waitSequence = DOTween.Sequence().AppendInterval(_framesBetweenJumps * TickController.TickRate).OnComplete(
                () =>
                {
                    _waitSequence = null;
                    DoJump();
                });
        }

        protected internal override void SubscribeToMessages()
        {
            base.SubscribeToMessages();
            _controller.gameObject.Subscribe<UpdateBattleStateMessage>(UpdateBattleState);
            _controller.transform.parent.gameObject.SubscribeWithFilter<UpdateBattleAlignmentMessage>(UpdateBattleAlignment, _instanceId);
            _controller.transform.parent.gameObject.SubscribeWithFilter<DoVictoryAnimationMessage>(DoVictoryAnimation, _instanceId);
            _controller.transform.parent.gameObject.SubscribeWithFilter<UpdateUnitBattleStateMessage>(UpdateUnitBattleState, _instanceId);
            _controller.transform.parent.gameObject.SubscribeWithFilter<InterruptBumpMessage>(InterruptBump, _instanceId);
        }

        private void UpdateBattleAlignment(UpdateBattleAlignmentMessage msg)
        {
            _alignment = msg.Alignment;
            _alignmentController.Setup(_alignment);
            switch (_alignment)
            {
                case BattleAlignment.Left:
                    _alignmentController.FlipX(true);
                    break;
            }
        }

        protected internal override void SetFacingDirection(SetFaceDirectionMessage msg)
        {
            base.SetFacingDirection(msg);
            if (_currentDirection.x != 0)
            {
                switch (FlipSprite)
                {
                    case FlipSprite.Negative:
                        _alignmentController.FlipX(_currentDirection.x > 0);
                        break;
                    case FlipSprite.Positive:
                        _alignmentController.FlipX(_currentDirection.x < 0);
                        break;
                }
            }


        }

        protected internal override void UpdateDirection(UpdateDirectionMessage msg)
        {
            base.UpdateDirection(msg);
            if (_currentDirection.x != 0)
            {
                switch (FlipSprite)
                {
                    case FlipSprite.Negative:
                        _alignmentController.FlipX(_currentDirection.x > 0);
                        break;
                    case FlipSprite.Positive:
                        _alignmentController.FlipX(_currentDirection.x < 0);
                        break;
                }
            }

        }

        private void DoVictoryAnimation(DoVictoryAnimationMessage msg)
        {
            if (_bumpTween != null)
            {
                if (_bumpTween.IsActive())
                {
                    _bumpTween.Kill();
                }

                _bumpTween = null;
            }
            _spriteController.SetOffset(Offset);
            _delayRoutine = _controller.StartCoroutine(StaticMethods.WaitForFrames(_victoryAnimationDelay.Roll(), () =>
                {
                    _delayRoutine = null;
                    DoJump();
                }));
        }

        private void UpdateUnitBattleState(UpdateUnitBattleStateMessage msg)
        {
            if (msg.State == UnitBattleState.Dead)
            {
                if (_bumpTween != null)
                {
                    if (_bumpTween.IsActive())
                    {
                        _bumpTween.Kill();
                    }

                    _bumpTween = null;
                }
            }
        }

        private void UpdateBattleState(UpdateBattleStateMessage msg)
        {
            if (msg.State == BattleState.Results || msg.State == BattleState.End)
            {
                if (_bumpTween != null)
                {
                    if (_bumpTween.IsActive())
                    {
                        _bumpTween.Kill();
                    }

                    _bumpTween = null;
                }
            }
        }

        private void InterruptBump(InterruptBumpMessage msg)
        {
            if (_bumpTween != null)
            {
                if (_bumpTween.IsActive())
                {
                    _bumpTween.Kill();
                }

                _bumpTween = null;
            }

            _spriteController.transform.SetLocalPosition(Offset);
        }

        public override void Destroy()
        {
            if (_jumpSequence != null)
            {
                if (_jumpSequence.IsActive())
                {
                    _jumpSequence.Kill();
                }

                _jumpSequence = null;
            }

            if (_waitSequence != null)
            {
                if (_waitSequence.IsActive())
                {
                    _waitSequence.Kill();
                }

                _waitSequence = null;
            }

            if (_delayRoutine != null)
            {
                _controller.StopCoroutine(_delayRoutine);
            }

            base.Destroy();
        }
    }
}