using Assets.Resources.Ancible_Tools.Scripts.System;
using UnityEngine;

namespace Assets.Ancible_Tools.Scripts.Traits
{
    [CreateAssetMenu(fileName = "Apply Maximum Population Trait", menuName = "Ancible Tools/Traits/Hobbler/Population/Apply Maximum Population")]
    public class ApplyMaximumPopulationTrait : Trait
    {
        public override bool Instant => true;

        [SerializeField] private int _amount = 1;

        public override void SetupController(TraitController controller)
        {
            base.SetupController(controller);
            WorldHobblerManager.IncreaseMaxPopulation(_amount);
        }
    }
}