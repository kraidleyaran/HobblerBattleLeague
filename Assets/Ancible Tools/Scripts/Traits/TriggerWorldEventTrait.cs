using Assets.Resources.Ancible_Tools.Scripts.System.WorldEvents;
using UnityEngine;

namespace Assets.Ancible_Tools.Scripts.Traits
{
    [CreateAssetMenu(fileName = "Trigger World Event Receiver", menuName = "Ancible Tools/Traits/World Event/Trigger World Event Receiver")]
    public class TriggerWorldEventTrait : Trait
    {
        public override bool Instant => true;

        [SerializeField] private WorldEvent _event = null;

        public override void SetupController(TraitController controller)
        {
            base.SetupController(controller);
            WorldEventManager.TriggerWorldEvent(_event);
        }
    }
}