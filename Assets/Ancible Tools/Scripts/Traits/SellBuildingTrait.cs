using Assets.Resources.Ancible_Tools.Scripts.System;
using Assets.Resources.Ancible_Tools.Scripts.System.Building;
using Assets.Resources.Ancible_Tools.Scripts.System.UI;
using MessageBusLib;
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
            WorldBuilding building = null;
            var parent = _controller.transform.parent.gameObject;
            var queryBuildingMsg = MessageFactory.GenerateQueryBuildingMsg();
            queryBuildingMsg.DoAfter = (worldBuilding, tile, id) =>
            {
                building = worldBuilding;
            };
            _controller.gameObject.SendMessageTo(queryBuildingMsg, parent);
            MessageFactory.CacheMessage(queryBuildingMsg);

            
            if (building)
            {
                UiController.ShowConfirmationAlert($"Are you sure you want to sell {building.DisplayName} for {WorldBuildingManager.CalculateSellbackValue(building.Cost)}g?", building.Icon,
                    () =>
                    {
                        WorldBuildingManager.SellBuilding(parent);
                    }, Color.white);
            }
        }


    }
}