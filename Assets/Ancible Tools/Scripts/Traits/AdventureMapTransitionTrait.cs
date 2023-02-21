using Assets.Resources.Ancible_Tools.Scripts.System;
using Assets.Resources.Ancible_Tools.Scripts.System.Adventure;
using Assets.Resources.Ancible_Tools.Scripts.System.Pathing;
using MessageBusLib;
using UnityEngine;

namespace Assets.Ancible_Tools.Scripts.Traits
{
    [CreateAssetMenu(fileName = "Adventure Map Transition Trait", menuName = "Ancible Tools/Traits/Adventure/Interaction/Adventure Map Transition")]
    public class AdventureMapTransitionTrait : Trait
    {
        [SerializeField] private AdventureMap _map;
        [SerializeField] private Vector2Int _position = Vector2Int.zero;
        [SerializeField] private Vector2Int _direction = Vector2Int.down;

        private MapTile _mapTile = null;

        public override void SetupController(TraitController controller)
        {
            base.SetupController(controller);
            SubscribeToMessages();
        }

        private void ApplyEvent(GameObject obj)
        {
            if (obj == WorldAdventureController.Player)
            {
                var setUnitStateMsg = MessageFactory.GenerateSetAdventureUnitStateMsg();
                setUnitStateMsg.State = AdventureUnitState.Interaction;
                _controller.gameObject.SendMessageTo(setUnitStateMsg, WorldAdventureController.Player);
                MessageFactory.CacheMessage(setUnitStateMsg);

                WorldAdventureController.TransitionToMap(_map, _position, _direction);
            }
        }

        private void SubscribeToMessages()
        {
            _controller.transform.parent.gameObject.SubscribeWithFilter<UpdateMapTileMessage>(UpdateMapTile, _instanceId);
            _controller.transform.parent.gameObject.SubscribeWithFilter<SetAdventureMapTransitionMessage>(SetAdventureMapTransition, _instanceId);
        }

        private void UpdateMapTile(UpdateMapTileMessage msg)
        {
            if (_mapTile != null)
            {
                _mapTile.OnObjectEnteringTile -= ApplyEvent;
            }

            _mapTile = msg.Tile;
            _mapTile.OnObjectEnteringTile += ApplyEvent;
        }

        private void SetAdventureMapTransition(SetAdventureMapTransitionMessage msg)
        {
            _map = msg.Map;
            _position = msg.Position;
            _direction = msg.Direction;
        }

    }
}