using Assets.Resources.Ancible_Tools.Scripts.System;
using Assets.Resources.Ancible_Tools.Scripts.System.Adventure;
using Assets.Resources.Ancible_Tools.Scripts.System.Pathing;
using MessageBusLib;
using UnityEngine;

namespace Assets.Ancible_Tools.Scripts.Traits
{
    [CreateAssetMenu(fileName = "Adventure Checkpoint Trait", menuName = "Ancible Tools/Traits/Adventure/Interaction/Adventure Checkpoint")]
    public class AdventureCheckpointTrait : Trait
    {
        [SerializeField] private Vector2Int _spawnOffset = Vector2Int.zero;

        private MapTile _spawnTile = null;

        public override void SetupController(TraitController controller)
        {
            base.SetupController(controller);
            SubscribeToMessages();
        }

        private void SetPlayerCheckpoint(GameObject player)
        {
            if (_spawnTile != null)
            {
                var setPlayerCheckpointMsg = MessageFactory.GenerateSetPlayerCheckpointMsg();
                setPlayerCheckpointMsg.Position = _spawnTile.Position;
                setPlayerCheckpointMsg.Map = WorldAdventureController.Current;
                _controller.gameObject.SendMessageTo(setPlayerCheckpointMsg, player);
                MessageFactory.CacheMessage(setPlayerCheckpointMsg);
            }

        }

        private void FinishBump(GameObject player)
        {
            var setAdventureUnitStateMsg = MessageFactory.GenerateSetAdventureUnitStateMsg();
            setAdventureUnitStateMsg.State = AdventureUnitState.Idle;
            _controller.gameObject.SendMessageTo(setAdventureUnitStateMsg, player);
            MessageFactory.CacheMessage(setAdventureUnitStateMsg);
        }

        private void SubscribeToMessages()
        {
            _controller.transform.parent.gameObject.SubscribeWithFilter<InteractMessage>(Interact, _instanceId);
            _controller.transform.parent.gameObject.SubscribeWithFilter<UpdateMapTileMessage>(UpdateMapTile, _instanceId);
        }

        private void Interact(InteractMessage msg)
        {
            var setAdventureUnitStateMsg = MessageFactory.GenerateSetAdventureUnitStateMsg();
            setAdventureUnitStateMsg.State = AdventureUnitState.Interaction;
            _controller.gameObject.SendMessageTo(setAdventureUnitStateMsg, msg.Owner);
            MessageFactory.CacheMessage(setAdventureUnitStateMsg);

            var owner = msg.Owner;

            var doBumpMsg = MessageFactory.GenerateDoBumpMsg();
            doBumpMsg.Direction = (_controller.transform.parent.position.ToVector2() - msg.Owner.transform.position.ToVector2()).normalized;
            doBumpMsg.OnBump = () => SetPlayerCheckpoint(owner);
            doBumpMsg.DoAfter = () => FinishBump(owner);
            _controller.gameObject.SendMessageTo(doBumpMsg, owner);
            MessageFactory.CacheMessage(doBumpMsg);
        }

        private void UpdateMapTile(UpdateMapTileMessage msg)
        {
            _spawnTile = WorldAdventureController.MapController.PlayerPathing.GetTileByPosition(msg.Tile.Position + _spawnOffset);
        }
    }
}