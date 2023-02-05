using Assets.Resources.Ancible_Tools.Scripts.System;
using Assets.Resources.Ancible_Tools.Scripts.System.Pathing;
using MessageBusLib;
using UnityEngine;

namespace Assets.Ancible_Tools.Scripts.Traits
{
    [CreateAssetMenu(fileName = "Static Position Trait", menuName = "Ancible Tools/Traits/Static Position")]
    public class StaticPositionTrait : Trait
    {
        [SerializeField] private bool _blocking = false;

        private MapTile _mapTile = null;

        public override void SetupController(TraitController controller)
        {
            base.SetupController(controller);
            SubscribeToMessages();
        }

        private void SubscribeToMessages()
        {
            _controller.transform.parent.gameObject.SubscribeWithFilter<SetMapTileMessage>(SetMapTile, _instanceId);
            _controller.transform.parent.gameObject.SubscribeWithFilter<QueryMapTileMessage>(QueryMapTile, _instanceId);
            _controller.transform.parent.gameObject.SubscribeWithFilter<ResetPositionMessage>(ResetPosition, _instanceId);
        }

        private void SetMapTile(SetMapTileMessage msg)
        {

            if (_mapTile != null && _blocking)
            {
                WorldController.Pathing.RemoveTileBlock(_controller.transform.parent.gameObject, _mapTile.Position);
            }

            _mapTile = msg.Tile;
            if (_blocking)
            {
                WorldController.Pathing.SetTileBlock(_controller.transform.parent.gameObject, _mapTile.Position);
            }

            var updateTileMsg = MessageFactory.GenerateUpdateMapTileMsg();
            updateTileMsg.Tile = _mapTile;
            _controller.gameObject.SendMessageTo(updateTileMsg, _controller.transform.parent.gameObject);
            MessageFactory.CacheMessage(updateTileMsg);
        }

        private void QueryMapTile(QueryMapTileMessage msg)
        {
            msg.DoAfter.Invoke(_mapTile);
        }

        private void ResetPosition(ResetPositionMessage msg)
        {
            _controller.transform.parent.SetTransformPosition(_mapTile.World);
        }
    }
}