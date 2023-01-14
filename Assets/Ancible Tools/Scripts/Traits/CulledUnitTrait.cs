using Assets.Resources.Ancible_Tools.Scripts.Hitbox;
using Assets.Resources.Ancible_Tools.Scripts.System;
using Assets.Resources.Ancible_Tools.Scripts.System.Culling;
using Assets.Resources.Ancible_Tools.Scripts.System.Minigame;
using MessageBusLib;
using UnityEngine;

namespace Assets.Ancible_Tools.Scripts.Traits
{
    [CreateAssetMenu(fileName = "Culled Unit Trait", menuName = "Ancible Tools/Traits/Culling/Culled Unit")]
    public class CulledUnitTrait : Trait
    {
        [SerializeField] private Resources.Ancible_Tools.Scripts.Hitbox.Hitbox _hitbox;

        private HitboxController _hitboxController = null;
        private CulledUnitController _culledController = null;

        private MinigameUnitState _unitState = MinigameUnitState.Idle;
        private bool _dead = false;

        public override void SetupController(TraitController controller)
        {
            base.SetupController(controller);
            _hitboxController = _controller.gameObject.SetupHitbox(_hitbox, CollisionLayerFactory.UnitCulling);
            var registerCollisionMsg = MessageFactory.GenerateRegisterCollisionMsg();
            registerCollisionMsg.Object = _controller.gameObject;
            _controller.gameObject.SendMessageTo(registerCollisionMsg, _hitboxController.gameObject);
            MessageFactory.CacheMessage(registerCollisionMsg);
            SubscribeToMessages();
        }

        private void SubscribeToMessages()
        {
            _controller.transform.parent.gameObject.SubscribeWithFilter<CullingCheckMessage>(CullingCheck, _instanceId);
            _controller.transform.parent.gameObject.SubscribeWithFilter<UnitDiedMessage>(UnitDied, _instanceId);
            _controller.transform.parent.gameObject.SubscribeWithFilter<UpdateMinigameUnitStateMessage>(UpdateMinigameUnitState, _instanceId);
        }

        private void CullingCheck(CullingCheckMessage msg)
        {
            var isCulled = _dead || (_unitState == MinigameUnitState.Disabled && _culledController ? _culledController.IsCulled() : CullingManager.IsCulled(_hitboxController.Collider2d));
            if (isCulled && _unitState != MinigameUnitState.Disabled)
            {
                if (!_culledController)
                {
                    _culledController = Instantiate(CullingManager.CulledUnit, _controller.transform.parent.position.ToVector2(), Quaternion.identity);
                }
                _culledController.transform.SetTransformPosition(_controller.transform.parent.position.ToVector2());
                _culledController.Setup(_controller.transform.parent.gameObject, _hitbox);
                var setUnitMinigameStateMsg = MessageFactory.GenerateSetMinigameUnitStateMsg();
                setUnitMinigameStateMsg.State = MinigameUnitState.Disabled;
                _controller.gameObject.SendMessageTo(setUnitMinigameStateMsg, _controller.transform.parent.gameObject);
                MessageFactory.CacheMessage(setUnitMinigameStateMsg);
            }
            else if (!isCulled && !_dead && _unitState == MinigameUnitState.Disabled)
            {
                if (_culledController)
                {
                    _culledController.Disable();
                }
                var setUnitMinigameStateMsg = MessageFactory.GenerateSetMinigameUnitStateMsg();
                setUnitMinigameStateMsg.State = MinigameUnitState.Idle;
                _controller.gameObject.SendMessageTo(setUnitMinigameStateMsg, _controller.transform.parent.gameObject);
                MessageFactory.CacheMessage(setUnitMinigameStateMsg);
            }
        }

        private void UnitDied(UnitDiedMessage msg)
        {
            _dead = true;
            if (_hitboxController)
            {
                var unregisterCollisionMsg = MessageFactory.GenerateUnregisterCollisionMsg();
                unregisterCollisionMsg.Object = _controller.gameObject;
                _controller.gameObject.SendMessageTo(unregisterCollisionMsg, _hitboxController.gameObject);
                MessageFactory.CacheMessage(unregisterCollisionMsg);
                _hitboxController = null;
            }
        }

        private void UpdateMinigameUnitState(UpdateMinigameUnitStateMessage msg)
        {
            _unitState = msg.State;
        }

        public override void Destroy()
        {
            if (_hitboxController)
            {
                var unregisterCollisionMsg = MessageFactory.GenerateUnregisterCollisionMsg();
                unregisterCollisionMsg.Object = _controller.gameObject;
                _controller.gameObject.SendMessageTo(unregisterCollisionMsg, _hitboxController.gameObject);
                MessageFactory.CacheMessage(unregisterCollisionMsg);
                _hitboxController = null;
            }

            if (_culledController)
            {
                Destroy(_culledController.gameObject);
            }
            base.Destroy();
        }

        
    }
}