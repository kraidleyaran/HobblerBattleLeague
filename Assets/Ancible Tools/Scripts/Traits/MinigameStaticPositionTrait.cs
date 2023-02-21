using Assets.Resources.Ancible_Tools.Scripts.System;
using Assets.Resources.Ancible_Tools.Scripts.System.Minigame;
using Assets.Resources.Ancible_Tools.Scripts.System.Pathing;
using MessageBusLib;
using UnityEngine;

namespace Assets.Ancible_Tools.Scripts.Traits
{
    [CreateAssetMenu(fileName = "Minigame Static Position Trait", menuName = "Ancible Tools/Traits/Minigame/Movement/Minigame Static Position")]
    public class MinigameStaticPositionTrait : Trait
    {
        [SerializeField] private bool _blocking = false;

        private MapTile _mapTile = null;
        private MinigameUnitState _unitState = MinigameUnitState.Idle;
        private Rigidbody2D _rigidBody = null;

        public override void SetupController(TraitController controller)
        {
            base.SetupController(controller);
            _rigidBody = _controller.transform.parent.gameObject.GetComponent<Rigidbody2D>();
            SubscribeToMessages();
        }

        private void SubscribeToMessages()
        {
            _controller.transform.parent.gameObject.SubscribeWithFilter<QueryMapTileMessage>(QueryMapTile, _instanceId);
            _controller.transform.parent.gameObject.SubscribeWithFilter<SetMapTileMessage>(SetMapTile, _instanceId);
            _controller.transform.parent.gameObject.SubscribeWithFilter<UpdateMinigameUnitStateMessage>(UpdateMinigameUnitState, _instanceId);
            _controller.transform.parent.gameObject.SubscribeWithFilter<UpdateMinigameUnitStateMessage>(UpdateMinigameUnitState, _instanceId);
        }

        private void SetMapTile(SetMapTileMessage msg)
        {
            var prevMapTile = _mapTile;
            _mapTile = msg.Tile;
            if (_blocking && _unitState != MinigameUnitState.Disabled)
            {
                if (prevMapTile != null)
                {
                    MinigameController.Pathing.RemoveTileBlock(_controller.transform.parent.gameObject, prevMapTile.Position);
                    MinigameFogOfWarController.RemoveBlockOnTile(prevMapTile.Position);
                }
                MinigameController.Pathing.SetTileBlock(_controller.transform.parent.gameObject, _mapTile.Position);
                MinigameFogOfWarController.SetBlockOnTile(_mapTile.Position);
            }

            _rigidBody.position = _mapTile.World;
            var updateMapTileMsg = MessageFactory.GenerateUpdateMapTileMsg();
            updateMapTileMsg.Tile = msg.Tile;
            _controller.gameObject.SendMessageTo(updateMapTileMsg, _controller.transform.parent.gameObject);
            MessageFactory.CacheMessage(updateMapTileMsg);
        }

        private void QueryMapTile(QueryMapTileMessage msg)
        {
            msg.DoAfter.Invoke(_mapTile);
        }

        private void UpdateMinigameUnitState(UpdateMinigameUnitStateMessage msg)
        {
            if (_blocking)
            {
                if (msg.State == MinigameUnitState.Disabled)
                {
                    MinigameController.Pathing.RemoveTileBlock(_controller.transform.parent.gameObject, _mapTile.Position);
                    MinigameFogOfWarController.RemoveBlockOnTile(_mapTile.Position);
                }
                else if (_unitState == MinigameUnitState.Disabled)
                {
                    MinigameController.Pathing.SetTileBlock(_controller.transform.parent.gameObject, _mapTile.Position);
                    MinigameFogOfWarController.SetBlockOnTile(_mapTile.Position);
                }
            }

            _unitState = msg.State;
        }

        public override void Destroy()
        {
            if (_blocking && _mapTile != null)
            {
                MinigameController.Pathing.RemoveTileBlock(_controller.transform.parent.gameObject, _mapTile.Position);
                MinigameFogOfWarController.RemoveBlockOnTile(_mapTile.Position);
            }
            base.Destroy();
        }
    }

}