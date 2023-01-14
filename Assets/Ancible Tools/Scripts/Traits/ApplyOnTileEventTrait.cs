using Assets.Resources.Ancible_Tools.Scripts.System;
using Assets.Resources.Ancible_Tools.Scripts.System.Pathing;
using MessageBusLib;
using UnityEngine;

namespace Assets.Ancible_Tools.Scripts.Traits
{
    [CreateAssetMenu(fileName = "Apply On Tile Event Trait", menuName = "Ancible Tools/Traits/Apply On Tile Event")]
    public class ApplyOnTileEventTrait : Trait
    {
        [SerializeField] private Trait[] _applyOnTile;

        private MapTile _mapTile = null;

        public override void SetupController(TraitController controller)
        {
            base.SetupController(controller);
            SubscribeToMessages();
        }

        private void ApplyOnTile(GameObject obj)
        {
            var addTraitToUnitMsg = MessageFactory.GenerateAddTraitToUnitMsg();
            for (var i = 0; i < _applyOnTile.Length; i++)
            {
                addTraitToUnitMsg.Trait = _applyOnTile[i];
                _controller.transform.parent.gameObject.SendMessageTo(addTraitToUnitMsg, obj);
            }
            MessageFactory.CacheMessage(addTraitToUnitMsg);
        }

        private void SubscribeToMessages()
        {
            _controller.transform.parent.gameObject.SubscribeWithFilter<UpdateMapTileMessage>(UpdateMapTile, _instanceId);
        }

        private void UpdateMapTile(UpdateMapTileMessage msg)
        {
            if (_mapTile != null)
            {
                _mapTile.OnObjectEnteringTile -= ApplyOnTile;
            }

            _mapTile = msg.Tile;
            _mapTile.OnObjectEnteringTile += ApplyOnTile;
        }

        public override void Destroy()
        {
            if (_mapTile != null)
            {
                _mapTile.OnObjectEnteringTile -= ApplyOnTile;
            }

            _mapTile = null;
            base.Destroy();
        }
    }
}