using Assets.Resources.Ancible_Tools.Scripts.Hitbox;
using Assets.Resources.Ancible_Tools.Scripts.System;
using MessageBusLib;
using UnityEngine;

namespace Assets.Ancible_Tools.Scripts.Traits
{
    [CreateAssetMenu(fileName = "Selectable Unit Trait", menuName = "Ancible Tools/Traits/Selectable Unit")]
    public class SelectableUnitTrait : Trait
    {
        [SerializeField] private Resources.Ancible_Tools.Scripts.Hitbox.Hitbox _selectableHitbox;
        [SerializeField] private Vector2 _offset = Vector2.zero;
        [SerializeField] private Vector2 _size = new Vector2(31.25f, 31.25f);
        [SerializeField] private Vector2 _hoverSize = Vector2.one;
        [SerializeField] private WorldUnitType _unitType = WorldUnitType.Hobbler;

        private HitboxController _hitboxController = null;

        private UnitSelectorController _hovered;
        private UnitSelectorController _selected;
        

        public override void SetupController(TraitController controller)
        {
            base.SetupController(controller);
            _hitboxController = _controller.gameObject.SetupHitbox(_selectableHitbox, CollisionLayerFactory.UnitSelect);
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
            _controller.transform.parent.gameObject.SubscribeWithFilter<QueryWorldUnitTypeMessage>(QueryWorldUnitType, _instanceId);
            _controller.transform.parent.gameObject.SubscribeWithFilter<SetActiveSelectableStateMessage>(SetActiveSelectableState, _instanceId);
        }

        private void SetHoveredState(SetHoveredStateMessage msg)
        {
            _hovered = msg.Selector;
            if (_hovered)
            {
                _hovered.SetParent(_controller.transform.parent, _offset, _hoverSize);
                _hovered.gameObject.SetActive(true);
            }
        }

        private void SetSelectedState(SetSelectStateMesage msg)
        {
            _selected = msg.Selector;
            if (_selected)
            {
                _selected.SetParent(_controller.transform.parent, _offset, _size);
                _selected.gameObject.SetActive(true);
            }
        }

        

        private void QueryWorldUnitType(QueryWorldUnitTypeMessage msg)
        {
            msg.DoAfter.Invoke(_unitType);
        }

        private void SetActiveSelectableState(SetActiveSelectableStateMessage msg)
        {
            _hitboxController.gameObject.SetActive(msg.Selectable);
            if (!msg.Selectable && (_selected || _hovered))
            {
                if (_selected)
                {
                    var removeSelectedUnitMsg = MessageFactory.GenerateRemoveSelectedUnitMsg();
                    removeSelectedUnitMsg.Unit = _controller.transform.parent.gameObject;
                    _controller.gameObject.SendMessageTo(removeSelectedUnitMsg, _controller.transform.parent.gameObject);
                    MessageFactory.CacheMessage(removeSelectedUnitMsg);
                }

                if (_hovered)
                {
                    var removeHoveredUnitMsg = MessageFactory.GenerateRemoveHoveredUnitMsg();
                    removeHoveredUnitMsg.Unit = _controller.transform.parent.gameObject;
                    _controller.gameObject.SendMessage(removeHoveredUnitMsg);
                    MessageFactory.CacheMessage(removeHoveredUnitMsg);
                }
            }
        }

        public override void Destroy()
        {
            if (_hitboxController)
            {
                var unregisterCollisionMsg = MessageFactory.GenerateUnregisterCollisionMsg();
                unregisterCollisionMsg.Object = _controller.gameObject;
                _controller.gameObject.SendMessageTo(unregisterCollisionMsg, _hitboxController.gameObject);
                MessageFactory.CacheMessage(unregisterCollisionMsg);
            }
            if (_hovered)
            {
                var removeHoveredUnitMsg = MessageFactory.GenerateRemoveHoveredUnitMsg();
                removeHoveredUnitMsg.Unit = _controller.transform.parent.gameObject;
                _controller.gameObject.SendMessage(removeHoveredUnitMsg);
                MessageFactory.CacheMessage(removeHoveredUnitMsg);
            }
            _hovered = null;

            if (_selected)
            {
                var removeSelectedUnitMsg = MessageFactory.GenerateRemoveSelectedUnitMsg();
                removeSelectedUnitMsg.Unit = _controller.transform.parent.gameObject;
                _controller.gameObject.SendMessage(removeSelectedUnitMsg);
                MessageFactory.CacheMessage(removeSelectedUnitMsg);
            }
            _selected = null;

            base.Destroy();
        }

    }
}