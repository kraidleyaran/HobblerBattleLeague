using System;
using Assets.Resources.Ancible_Tools.Scripts.System;
using Assets.Resources.Ancible_Tools.Scripts.System.Animation;
using MessageBusLib;
using UnityEngine;

namespace Assets.Ancible_Tools.Scripts.Traits
{
    [CreateAssetMenu(fileName = "Particle Fx Trait", menuName = "Ancible Tools/Traits/Animation/Particle Fx")]
    public class ParticleFxTrait : Trait
    {
        public override bool Instant => _instantiateAlone;

        [SerializeField] private ParticleSystem _particleSystem;
        [SerializeField] private bool _instantiateAlone = false;
        [SerializeField] private Vector2 _offset = Vector2.zero;

        private ParticleFxController _fxController = null;

        public override void SetupController(TraitController controller)
        {
            base.SetupController(controller);
            ParticleFxController fxController = null;
            Action doAfter = null;
            if (_instantiateAlone)
            {
                fxController = Instantiate(FactoryController.PARTICLE_FX_CONTROLLER, _controller.transform.parent.position.ToVector2(), Quaternion.identity);
            }
            else
            {
                fxController = Instantiate(FactoryController.PARTICLE_FX_CONTROLLER, _controller.transform.parent);
                _fxController = fxController;
                doAfter = ParticleFxFinished;
            }
            fxController.Setup(_particleSystem, doAfter, _offset, _instantiateAlone, _controller.transform.parent.gameObject.layer);
        }

        private void ParticleFxFinished()
        {
            Destroy(_fxController.gameObject);
            var removeTraitFromUnitByControllerMsg = MessageFactory.GenerateRemoveTraitFromUnitByControllerMsg();
            removeTraitFromUnitByControllerMsg.Controller = _controller;
            _controller.gameObject.SendMessageTo(removeTraitFromUnitByControllerMsg, _controller.transform.parent.gameObject);
            MessageFactory.CacheMessage(removeTraitFromUnitByControllerMsg);
        }
    }
}