using Assets.Resources.Ancible_Tools.Scripts.System;
using Assets.Resources.Ancible_Tools.Scripts.System.Combat;
using MessageBusLib;
using UnityEngine;

namespace Assets.Ancible_Tools.Scripts.Traits
{
    [CreateAssetMenu(fileName = "Dispel Status Effects Trait", menuName = "Ancible Tools/Traits/Combat/Dispel Status Effects")]
    public class DispelStatusEffectsTrait : Trait
    {
        public override bool Instant => true;

        [SerializeField] private StatusEffectType[] _types = new StatusEffectType[0];

        public override void SetupController(TraitController controller)
        {
            base.SetupController(controller);
            var dispelStatusEffectsMsg = MessageFactory.GenerateDispelStatusEffectsMsg();
            dispelStatusEffectsMsg.Types = _types;
            _controller.gameObject.SendMessageTo(dispelStatusEffectsMsg, _controller.transform.parent.gameObject);
            MessageFactory.CacheMessage(dispelStatusEffectsMsg);
        }
    }
}