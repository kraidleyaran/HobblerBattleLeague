using System.Collections.Generic;
using System.Linq;
using Assets.Resources.Ancible_Tools.Scripts.System;
using Assets.Resources.Ancible_Tools.Scripts.System.Building;
using Assets.Resources.Ancible_Tools.Scripts.System.Pathing;
using MessageBusLib;
using UnityEngine;

namespace Assets.Ancible_Tools.Scripts.Traits
{
    [CreateAssetMenu(fileName = "Building Trait", menuName = "Ancible Tools/Traits/Building")]
    public class BuildingTrait : Trait
    {
        private MapTile[] _blockedTiles = new MapTile[0];
        private WorldBuilding _building = null;

        private TraitController[] _activeSprites = new TraitController[0];

        public override void SetupController(TraitController controller)
        {
            base.SetupController(controller);
            SubscribeToMessages();
        }

        private void SubscribeToMessages()
        {
            _controller.transform.parent.gameObject.SubscribeWithFilter<UpdateMapTileMessage>(UpdateMapTile, _instanceId);
            _controller.transform.parent.gameObject.SubscribeWithFilter<SetupBuildingMessage>(SetupBuilding, _instanceId);
            _controller.transform.parent.gameObject.SubscribeWithFilter<QueryUnitNameMessage>(QueryUnitName, _instanceId);
        }

        private void UpdateMapTile(UpdateMapTileMessage msg)
        {
            if (_blockedTiles.Length > 0)
            {
                for (var i = 0; i < _blockedTiles.Length; i++)
                {
                    WorldController.Pathing.RemoveTileBlock(_controller.transform.parent.gameObject, _blockedTiles[i].Position);
                }
            }

            _blockedTiles = _building.RequiredTiles.Select(WorldController.Pathing.GetTileByPosition).ToArray();
            for (var i = 0; i < _blockedTiles.Length; i++)
            {
                WorldController.Pathing.SetTileBlock(_controller.transform.parent.gameObject, _blockedTiles[i].Position);
            }
        }

        private void SetupBuilding(SetupBuildingMessage msg)
        {
            _building = msg.Building;
            if (_activeSprites.Length > 0)
            {
                var removeTraitFromUnitByControllerMsg = MessageFactory.GenerateRemoveTraitFromUnitByControllerMsg();
                for (var i = 0; i < _activeSprites.Length; i++)
                {
                    removeTraitFromUnitByControllerMsg.Controller = _activeSprites[i];
                    _controller.gameObject.SendMessageTo(removeTraitFromUnitByControllerMsg, _controller.transform.parent.gameObject);
                }
                MessageFactory.CacheMessage(removeTraitFromUnitByControllerMsg);
            }

            var sprites = new List<TraitController>();
            var addTraitToUnitMsg = MessageFactory.GenerateAddTraitToUnitMsg();
            addTraitToUnitMsg.DoAfter = controller => { sprites.Add(controller); };
            for (var i = 0; i < _building.Sprites.Length; i++)
            {
                addTraitToUnitMsg.Trait = _building.Sprites[i];
                _controller.gameObject.SendMessageTo(addTraitToUnitMsg, _controller.transform.parent.gameObject);
            }
            MessageFactory.CacheMessage(addTraitToUnitMsg);
            _activeSprites = sprites.ToArray();
        }

        private void QueryUnitName(QueryUnitNameMessage msg)
        {
            msg.DoAfter.Invoke(_building.DisplayName);
        }
    }
}