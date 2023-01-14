using Assets.Resources.Ancible_Tools.Scripts.System;
using Assets.Resources.Ancible_Tools.Scripts.System.Minigame;
using MessageBusLib;
using UnityEngine;

namespace Assets.Ancible_Tools.Scripts.Traits
{
    [CreateAssetMenu(fileName = "Minigame Fog of War Trait", menuName = "Ancible Tools/Traits/Minigame/Player/Minigame Fog of War")]
    public class MinigameFogOfWarTrait : Trait
    {
        [SerializeField] private int _visionArea = 1;

        public override void SetupController(TraitController controller)
        {
            base.SetupController(controller);
            SubscribeToMessages();
        }

        private void SubscribeToMessages()
        {
            _controller.transform.parent.gameObject.SubscribeWithFilter<UpdateMapTileMessage>(UpdateMapTile, _instanceId);
        }

        private void UpdateMapTile(UpdateMapTileMessage msg)
        {
            MinigameFogOfWarController.SetPlayerPov(msg.Tile, _visionArea);
        }
    }
}