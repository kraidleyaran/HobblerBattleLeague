using System.Linq;
using Assets.Resources.Ancible_Tools.Scripts.System;
using Assets.Resources.Ancible_Tools.Scripts.System.Adventure;
using Assets.Resources.Ancible_Tools.Scripts.System.Pathing;
using MessageBusLib;
using UnityEngine;

namespace Assets.Ancible_Tools.Scripts.Traits
{
    [CreateAssetMenu(fileName = "Adventure Static Position Trait", menuName = "Ancible Tools/Traits/Adventure/Adventure Static Position")]
    public class AdventureStaticPositionTrait : Trait
    {
        [SerializeField] private bool _blocking = false;
        [SerializeField] private Vector2Int[] _relativePositions = new Vector2Int[0];

        private MapTile _currentTile = null;
        private MapTile[] _relativeTiles = new MapTile[0];

        public override void SetupController(TraitController controller)
        {
            base.SetupController(controller);
            SubscribeToMessages();
        }

        private void SubscribeToMessages()
        {
            _controller.transform.parent.gameObject.SubscribeWithFilter<SetMapTileMessage>(SetMapTile, _instanceId);
        }

        private void SetMapTile(SetMapTileMessage msg)
        {
            var obj = _controller.transform.parent.gameObject;
            if (_currentTile != null && _blocking)
            {
                WorldAdventureController.MapController.RemoveBlockingTile(obj, _currentTile.Position);
                if (_relativeTiles.Length > 0)
                {
                    for (var i = 0; i < _relativeTiles.Length; i++)
                    {
                        WorldAdventureController.MapController.RemoveBlockingTile(obj, _currentTile.Position);
                    }
                }

            }

            _currentTile = msg.Tile;
            if (_blocking)
            {
                WorldAdventureController.MapController.SetBlockingTile(obj, _currentTile.Position);
                _relativeTiles = _relativePositions.Select(p => WorldAdventureController.MapController.PlayerPathing.GetTileByPosition(p + _currentTile.Position)).Where(t => t != null).ToArray();
                for (var i = 0; i < _relativeTiles.Length; i++)
                {
                    WorldAdventureController.MapController.SetBlockingTile(obj, _relativeTiles[i].Position);
                }
            }

            var updateMapTileMsg = MessageFactory.GenerateUpdateMapTileMsg();
            updateMapTileMsg.Tile = _currentTile;
            _controller.gameObject.SendMessageTo(updateMapTileMsg, _controller.transform.parent.gameObject);
            MessageFactory.CacheMessage(updateMapTileMsg);
        }
    }
}