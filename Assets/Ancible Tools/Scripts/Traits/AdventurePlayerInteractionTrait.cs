using Assets.Resources.Ancible_Tools.Scripts.System;
using Assets.Resources.Ancible_Tools.Scripts.System.Adventure;
using Assets.Resources.Ancible_Tools.Scripts.System.Pathing;
using MessageBusLib;
using UnityEngine;

namespace Assets.Ancible_Tools.Scripts.Traits
{
    [CreateAssetMenu(fileName = "Adventure Player Interaction Trait", menuName = "Ancible Tools/Traits/Adventure/Player/Adventure Player Interaction")]
    public class AdventurePlayerInteractionTrait : Trait
    {
        private MapTile _currentTile = null;
        private GameObject _interactObj = null;
        private Vector2Int _faceDirection = Vector2Int.zero;

        private AdventureUnitState _unitState = AdventureUnitState.Idle;

        public override void SetupController(TraitController controller)
        {
            base.SetupController(controller);
            SubscribeToMessages();
        }

        private void CheckFacingTile()
        {
            var facingTile = WorldAdventureController.MapController.PlayerPathing.GetTileByPosition(_currentTile.Position + _faceDirection);
            if (facingTile != null && facingTile.Block && facingTile.Block != _controller.transform.parent.gameObject)
            {
                _interactObj = facingTile.Block;
            }
            else if (_interactObj)
            {
                _interactObj = null;
            }
        }

        private void SubscribeToMessages()
        {
            _controller.gameObject.Subscribe<UpdateInputStateMessage>(UpdateInputState);
            _controller.transform.parent.gameObject.SubscribeWithFilter<UpdateFacingDirectionMessage>(UpdateFaceDirection, _instanceId);
            _controller.transform.parent.gameObject.SubscribeWithFilter<UpdateMapTileMessage>(UpdateMapTile, _instanceId);
            _controller.transform.parent.gameObject.SubscribeWithFilter<UpdateAdventureUnitStateMessage>(UpdateAdventureUnitState, _instanceId);
            _controller.transform.parent.gameObject.SubscribeWithFilter<SetPlayerInteractionObjectMessage>(SetPlayerInteractObject, _instanceId);
        }

        private void UpdateFaceDirection(UpdateFacingDirectionMessage msg)
        {
            _faceDirection = msg.Direction;
            if (_currentTile != null)
            {
                CheckFacingTile();
            }
        }

        private void UpdateMapTile(UpdateMapTileMessage msg)
        {
            _currentTile = msg.Tile;
            if (_faceDirection != Vector2Int.zero)
            {
                CheckFacingTile();
            }
        }

        private void UpdateInputState(UpdateInputStateMessage msg)
        {
            if (WorldController.State == WorldState.Adventure && WorldAdventureController.State == AdventureState.Overworld && _unitState == AdventureUnitState.Idle && _interactObj)
            {
                if (!msg.Previous.Interact && msg.Current.Interact)
                {
                    _controller.gameObject.SendMessageTo(PlayerInteractMessage.INSTANCE, _interactObj);
                }
            }
        }

        private void UpdateAdventureUnitState(UpdateAdventureUnitStateMessage msg)
        {
            _unitState = msg.State;
        }

        private void SetPlayerInteractObject(SetPlayerInteractionObjectMessage msg)
        {
            _interactObj = msg.Interact;
        }
        
    }
}