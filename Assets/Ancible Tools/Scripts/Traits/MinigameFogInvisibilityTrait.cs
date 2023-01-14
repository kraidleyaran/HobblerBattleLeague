using Assets.Resources.Ancible_Tools.Scripts.System;
using Assets.Resources.Ancible_Tools.Scripts.System.Minigame;
using Assets.Resources.Ancible_Tools.Scripts.System.Pathing;
using MessageBusLib;
using UnityEngine;

namespace Assets.Ancible_Tools.Scripts.Traits
{
    [CreateAssetMenu(fileName = "Minigame Fog Invisibility Trait", menuName = "Ancible Tools/Traits/Minigame/Minigame Fog Invisibility")]
    public class MinigameFogInvisibilityTrait : Trait
    {
        private bool _visible = false;
        private MapTile _mapTile = null;

        public override void SetupController(TraitController controller)
        {
            base.SetupController(controller);
            SubscribeToMessages();
        }

        private void SubscribeToMessages()
        {
            _controller.transform.parent.gameObject.SubscribeWithFilter<UpdateMapTileMessage>(UpdateMapTile, _instanceId);
            _controller.transform.parent.gameObject.SubscribeWithFilter<MinigamePlayerPovUpdatedMessage>(MinigamePlayerPovUpdated, _instanceId);
        }

        private void UpdateMapTile(UpdateMapTileMessage msg)
        {
            _mapTile = msg.Tile;
            var visible = MinigameFogOfWarController.IsVisible(_mapTile);
            if (visible != _visible)
            {
                _visible = visible;
                var setSpriteVisibilityMsg = MessageFactory.GenerateSetSpriteVisibilityMsg();
                setSpriteVisibilityMsg.Visible = _visible;
                _controller.gameObject.SendMessageTo(setSpriteVisibilityMsg, _controller.transform.parent.gameObject);
                MessageFactory.CacheMessage(setSpriteVisibilityMsg);

                var updateFogVisibilityMsg = MessageFactory.GenerateUpdateFogVisibilityMsg();
                updateFogVisibilityMsg.Visible = _visible;
                _controller.gameObject.SendMessageTo(updateFogVisibilityMsg, _controller.transform.parent.gameObject);
                MessageFactory.CacheMessage(updateFogVisibilityMsg);
            }
        }

        private void MinigamePlayerPovUpdated(MinigamePlayerPovUpdatedMessage msg)
        {
            if (_mapTile != null)
            {
                var visible = MinigameFogOfWarController.IsVisible(_mapTile);
                if (visible != _visible)
                {
                    _visible = visible;
                    var setSpriteVisibilityMsg = MessageFactory.GenerateSetSpriteVisibilityMsg();
                    setSpriteVisibilityMsg.Visible = _visible;
                    _controller.gameObject.SendMessageTo(setSpriteVisibilityMsg, _controller.transform.parent.gameObject);
                    MessageFactory.CacheMessage(setSpriteVisibilityMsg);

                    var updateFogVisibilityMsg = MessageFactory.GenerateUpdateFogVisibilityMsg();
                    updateFogVisibilityMsg.Visible = _visible;
                    _controller.gameObject.SendMessageTo(updateFogVisibilityMsg, _controller.transform.parent.gameObject);
                    MessageFactory.CacheMessage(updateFogVisibilityMsg);
                }
            }
        }


    }
}