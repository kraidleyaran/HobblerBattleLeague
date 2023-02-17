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
        private Vector2Int _checkpoint = Vector2Int.zero;
        private AdventureMap _checkpointMap = null;
        private MapTile _currentTile = null;

        public override void SetupController(TraitController controller)
        {
            base.SetupController(controller);
            _checkpointMap = WorldAdventureController.Default;
            _checkpoint = _checkpointMap.DefaultTile;
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
            _controller.transform.parent.gameObject.SubscribeWithFilter<QueryPlayerCheckpointMessage>(QueryPlayerCheckpoint, _instanceId);
        }

        private void UpdateMapTile(UpdateMapTileMessage msg)
        {
            _currentTile = msg.Tile;
            msg.Tile.ApplyEvent(_controller.transform.parent.gameObject);
            WorldAdventureController.SetPlayerTile(msg.Tile);
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
            _checkpoint = msg.Position;
            _checkpointMap = msg.Map;
        }

        private void UpdatePosition(UpdatePositionMessage msg)
        {
            AdventureCameraController.SetCameraPosition(msg.Position);
        }

        private void RespawnPlayer(RespawnPlayerMessage msg)
        {
            WorldAdventureController.Setup(_checkpointMap, _checkpoint);
            //var setMapTileMsg = MessageFactory.GenerateSetMapTileMsg();
            //setMapTileMsg.Tile = _checkpoint;
            //_controller.gameObject.SendMessageTo(setMapTileMsg, _controller.transform.parent.gameObject);
            //MessageFactory.CacheMessage(setMapTileMsg);

            AdventureCameraController.SetCameraPosition(_currentTile.World.ToPixelPerfect());

            var setAdventureUnitStateMsg = MessageFactory.GenerateSetAdventureUnitStateMsg();
            setAdventureUnitStateMsg.State = AdventureUnitState.Idle;
            _controller.gameObject.SendMessageTo(setAdventureUnitStateMsg, _controller.transform.parent.gameObject);
            MessageFactory.CacheMessage(setAdventureUnitStateMsg);
        }

        private void QueryPlayerCheckpoint(QueryPlayerCheckpointMessage msg)
        {
            msg.DoAfter.Invoke(_checkpointMap.name, _checkpoint);
        }
    }
}