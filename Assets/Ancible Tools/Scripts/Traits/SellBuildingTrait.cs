using Assets.Resources.Ancible_Tools.Scripts.System.Building;
using UnityEngine;

namespace Assets.Ancible_Tools.Scripts.Traits
{
    [CreateAssetMenu(fileName = "Sell Building Trait", menuName = "Ancible Tools/Traits/Building/Sell Building")]
    public class SellBuildingTrait : Trait
    {
        public override bool Instant => true;

        public override void SetupController(TraitController controller)
        {
            base.SetupController(controller);
            WorldBuildingManager.SellBuilding(_controller.transform.parent.gameObject);
        }


    }
}