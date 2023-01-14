using Assets.Resources.Ancible_Tools.Scripts.System;
using Assets.Resources.Ancible_Tools.Scripts.System.Animation;
using MessageBusLib;
using UnityEngine;

namespace Assets.Ancible_Tools.Scripts.Traits
{
    [CreateAssetMenu(fileName = "Visual Fx Trait", menuName = "Ancible Tools/Traits/Animation/Visual Fx")]
    public class VisualFxTrait : Trait
    {
        public override bool Instant => InstantiateAlone;

        [SerializeField] private RuntimeAnimatorController _runtime;
        [SerializeField] private Vector2 _scaling = new Vector2(31.25f, 31.25f);
        [SerializeField] private Vector2 _offset = Vector2.zero;
        [SerializeField] private int _loop = 0;
        [SerializeField] private SpriteLayer _spriteLayer = null;
        [SerializeField] private int _sortingOrder = 0;
        public bool InstantiateAlone = false;

        private VisualFxController _fxController = null;

        public override void SetupController(TraitController controller)
        {
            base.SetupController(controller);
            VisualFxController fxController = null;
            if (InstantiateAlone)
            {
                var pos = _controller.transform.parent.position.ToVector2();
                fxController = Instantiate(FactoryController.VISUAL_FX_CONTROLLER, pos + _offset, Quaternion.identity);
            }
            else
            {
                fxController = Instantiate(FactoryController.VISUAL_FX_CONTROLLER, _controller.transform.parent);
                fxController.transform.SetLocalPosition(_offset);
                _fxController = fxController;
            }

            fxController.transform.SetLocalScaling(_scaling);
            fxController.gameObject.layer = _controller.transform.parent.gameObject.layer;
            fxController.SetSpriteLayer(_spriteLayer, _sortingOrder);
            fxController.Setup(_runtime, InstantiateAlone ? null : _controller.gameObject, _loop);
            if (!InstantiateAlone)
            {
                SubscribeToMessages();
            }
        }

        private void SubscribeToMessages()
        {
            _controller.gameObject.SubscribeWithFilter<FxAnimationFinishedMessage>(FxAnimationFinished, _instanceId);
        }

        private void FxAnimationFinished(FxAnimationFinishedMessage msg)
        {
            var removeTraitByControllerMsg = MessageFactory.GenerateRemoveTraitFromUnitByControllerMsg();
            removeTraitByControllerMsg.Controller = _controller;
            _controller.gameObject.SendMessageTo(removeTraitByControllerMsg, _controller.transform.parent.gameObject);
            MessageFactory.CacheMessage(removeTraitByControllerMsg);
        }

        public override void Destroy()
        {
            if (!InstantiateAlone)
            {
                Destroy(_fxController.gameObject);
            }
            base.Destroy();
        }
    }
}