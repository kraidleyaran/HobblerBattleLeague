using System.Linq;
using Assets.Resources.Ancible_Tools.Scripts.System;
using Assets.Resources.Ancible_Tools.Scripts.System.Building;
using Assets.Resources.Ancible_Tools.Scripts.System.Pathing;
using MessageBusLib;
using UnityEngine;

namespace Assets.Ancible_Tools.Scripts.Traits
{
    [CreateAssetMenu(fileName = "Move Building Trait", menuName = "Ancible Tools/Traits/Building/Move Building")]
    public class MoveBuildingTrait : Trait
    {
        public override bool Instant => true;

        public override void SetupController(TraitController controller)
        {
            base.SetupController(controller);
            var queryBuildingMsg = MessageFactory.GenerateQueryBuildingMsg();
            queryBuildingMsg.DoAfter = SetupBuilding;
            _controller.gameObject.SendMessageTo(queryBuildingMsg, _controller.transform.parent.gameObject);
            MessageFactory.CacheMessage(queryBuildingMsg);
        }

        private void SetupBuilding(WorldBuilding building, MapTile tile, string id)
        {
            var surroundingTiles = building.GetBlockingPositions(tile.Position).Select(p => WorldController.Pathing.GetTileByPosition(p + tile.Position)).Where(t => t != null && (!t.Block || t.Block == _controller.transform.parent.gameObject)).ToList();
            surroundingTiles.AddRange(building.GetRequiredPositions(tile.Position).Where(p => !surroundingTiles.Exists(t => t.Position == p)).Select(WorldController.Pathing.GetTileByPosition));
            //surroundingTiles.Add(tile);
            WorldBuildingManager.SetupMovingBuilding(_controller.transform.parent.gameObject, surroundingTiles.ToArray(), building);
        }
    }
}