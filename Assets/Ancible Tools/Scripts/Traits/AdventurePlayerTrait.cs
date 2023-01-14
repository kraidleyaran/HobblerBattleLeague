using Assets.Resources.Ancible_Tools.Scripts.System;
using Assets.Resources.Ancible_Tools.Scripts.System.Adventure;
using Assets.Resources.Ancible_Tools.Scripts.System.Pathing;
using MessageBusLib;
using UnityEngine;

namespace Assets.Ancible_Tools.Scripts.Traits
{
    [CreateAssetMenu(fileName = "Adventure Player Trait", menuName = "Ancible Tools/Traits/Adventure/Player/Adventure Player")]
    public class AdventurePlayerTrait : Trait
    {
        private AdventureUnitState _unitState = AdventureUnitState.Idle;
        private MapTile _checkpoint = null;

        public override void SetupController(TraitController controller)
        {
            base.SetupController(controller);
            SubscribeToMessages();
        }

        private void SubscribeToMessages()
        {
            _controller.transform.parent.gameObject.SubscribeWithFilter<UpdateMapTileMessage>(UpdateMapTile, _instanceId);
            _controller.transform.parent.gameObject.SubscribeWithFilter<ObstacleMessage>(Obstacle, _instanceId);
            _controller.transform.parent.gameObject.SubscribeWithFilter<UpdatePositionMessage>(UpdatePosition, _instanceId);
            _controller.transform.parent.gameObject.SubscribeWithFilter<UpdateAdventureUnitStateMessage>(UpdateAdventureUnitState, _instanceId);
            _controller.transform.parent.gameObject.SubscribeWithFilter<SetPlayerCheckpointMessage>(SetPlayerCheckpoint, _instanceId);
            _controller.transform.parent.gameObject.SubscribeWithFilter<RespawnPlayerMessage>(RespawnPlayer, _instanceId);
        }

        private void UpdateMapTile(UpdateMapTileMessage msg)
        {
            if (_checkpoint == null)
            {
                _checkpoint = msg.Tile;
            }
            msg.Tile.ApplyEvent(_controller.transform.parent.gameObject);
        }

        private void Obstacle(ObstacleMessage msg)
        {
            if (_unitState == AdventureUnitState.Idle)
            {
                var interactMsg = MessageFactory.GenerateInteractMsg();
                interactMsg.Owner = _controller.transform.parent.gameObject;
                _controller.gameObject.SendMessageTo(interactMsg, msg.Obstacle);
                MessageFactory.CacheMessage(interactMsg);
            }
        }

        private void UpdateAdventureUnitState(UpdateAdventureUnitStateMessage msg)
        {
            _unitState = msg.State;
        }

        private void SetPlayerCheckpoint(SetPlayerCheckpointMessage msg)
        {
            _checkpoint = msg.Tile;
        }

        private void UpdatePosition(UpdatePositionMessage msg)
        {
            AdventureCameraController.SetCameraPosition(msg.Position.ToPixelPerfect());
        }

        private void RespawnPlayer(RespawnPlayerMessage msg)
        {
            var setMapTileMsg = MessageFactory.GenerateSetMapTileMsg();
            setMapTileMsg.Tile = _checkpoint;
            _controller.gameObject.SendMessageTo(setMapTileMsg, _controller.transform.parent.gameObject);
            MessageFactory.CacheMessage(setMapTileMsg);

            var setAdventureUnitStateMsg = MessageFactory.GenerateSetAdventureUnitStateMsg();
            setAdventureUnitStateMsg.State = AdventureUnitState.Idle;
            _controller.gameObject.SendMessageTo(setAdventureUnitStateMsg, _controller.transform.parent.gameObject);
            MessageFactory.CacheMessage(setAdventureUnitStateMsg);
        }
    }
}