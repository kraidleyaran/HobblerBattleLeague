using Assets.Resources.Ancible_Tools.Scripts.Hitbox;
using Assets.Resources.Ancible_Tools.Scripts.System;
using Assets.Resources.Ancible_Tools.Scripts.System.Minigame;
using MessageBusLib;
using UnityEngine;

namespace Assets.Ancible_Tools.Scripts.Traits
{
    [CreateAssetMenu(fileName = "Minigame Selectable Unit Trait", menuName = "Ancible Tools/Traits/Minigame/Minigame Selectable Unit")]
    public class MinigameSelectableUnitTrait : Trait
    {
        [SerializeField] private Hitbox _hitbox;
        [SerializeField] private Vector2 _offset = Vector2.zero;
        [SerializeField] private Vector2 _selectorSize = new Vector2(31.25f, 31.25f);

        private HitboxController _hitboxController = null;

        private UnitSelectorController _selected = null;
        private UnitSelectorController _hovered = null;

        private bool _visible = true;
        private MinigameUnitState _unitState = MinigameUnitState.Idle;

        public override void SetupController(TraitController controller)
        {
            base.SetupController(controller);
            _hitboxController = _controller.gameObject.SetupHitbox(_hitbox, CollisionLayerFactory.MinigameSelect);
            var registerCollisionMsg = MessageFactory.GenerateRegisterCollisionMsg();
            registerCollisionMsg.Object = _controller.gameObject;
            _controller.gameObject.SendMessageTo(registerCollisionMsg, _hitboxController.gameObject);
            MessageFactory.CacheMessage(registerCollisionMsg);
            SubscribeToMessages();
        }

        private void SubscribeToMessages()
        {
            _controller.transform.parent.gameObject.SubscribeWithFilter<SetHoveredStateMessage>(SetHoveredState, _instanceId);
            _controller.transform.parent.gameObject.SubscribeWithFilter<SetSelectStateMesage>(SetSelectedState, _instanceId);
            _controller.transform.parent.gameObject.SubscribeWithFilter<UpdateFogVisibilityMessage>(UpdateFogVisibility, _instanceId);
            _controller.transform.parent.gameObject.SubscribeWithFilter<UpdateMinigameUnitStateMessage>(UpdateMinigameUnitState, _instanceId);
        }

        private void SetHoveredState(SetHoveredStateMessage msg)
        {
            _hovered = msg.Selector;
            if (_hovered)
            {
                _hovered.SetParent(_controller.transform.parent, _offset, _selectorSize);
                _hovered.gameObject.SetActive(true);
            }
        }

        private void UpdateFogVisibility(UpdateFogVisibilityMessage msg)
        {
            _hitboxController.gameObject.SetActive(msg.Visible);
            if (_selected)
            {
                MinigameUnitSelectController.RemoveSelectedUnit(_controller.transform.parent.gameObject);
            }
        }

        private void SetSelectedState(SetSelectStateMesage msg)
        {
            _selected = msg.Selector;
            if (_selected)
            {
                _selected.SetParent(_controller.transform.parent, _offset, _selectorSize);
                _selected.gameObject.SetActive(true);
            }
        }

        private void UpdateMinigameUnitState(UpdateMinigameUnitStateMessage msg)
        {
            if (msg.State == MinigameUnitState.Disabled)
            {
                if (_selected)
                {
                    MinigameUnitSelectController.RemoveSelectedUnit(_controller.transform.parent.gameObject);
                }

                if (_hovered)
                {
                    MinigameUnitSelectController.RemoveHoveredUnit(_controller.transform.parent.gameObject);
                }
                _hitboxController.gameObject.SetActive(false);
            }
            else if (_unitState == MinigameUnitState.Disabled && msg.State != MinigameUnitState.Disabled)
            {
                _hitboxController.gameObject.SetActive(true);
            }

            _unitState = msg.State;
        }

        public override void Destroy()
        {
            if (_selected)
            {
                MinigameUnitSelectController.RemoveSelectedUnit(_controller.transform.parent.gameObject);
            }

            if (_hovered)
            {
                MinigameUnitSelectController.RemoveHoveredUnit(_controller.transform.parent.gameObject);
            }


            if (_hitboxController)
            {
                var unregisterCollisionMsg = MessageFactory.GenerateUnregisterCollisionMsg();
                unregisterCollisionMsg.Object = _controller.gameObject;
                _controller.gameObject.SendMessageTo(unregisterCollisionMsg, _hitboxController.gameObject);
                MessageFactory.CacheMessage(unregisterCollisionMsg);
            }
            base.Destroy();
        }
    }
}