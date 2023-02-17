using Assets.Resources.Ancible_Tools.Scripts.System;
using Assets.Resources.Ancible_Tools.Scripts.System.Adventure;
using MessageBusLib;
using UnityEngine;

namespace Assets.Ancible_Tools.Scripts.Traits
{
    [CreateAssetMenu(fileName = "Adventure Custom Dialogue Trait", menuName = "Ancible Tools/Traits/Adventure/Interaction/Custom Dialogue")]
    public class AdventureCustomDialogueInteractionTrait : Trait
    {
        [SerializeField] [TextArea(3,5)] private string[] _dialogue = new string[0];

        private AdventureUnitState _unitState = AdventureUnitState.Idle;
        private Coroutine _showRoutine = null;
        private bool _active = false;

        public override void SetupController(TraitController controller)
        {
            base.SetupController(controller);
            SubscribeToMessages();
        }

        private void ShowDialogue()
        {
            var showCustomDialogueMsg = MessageFactory.GenerateShowCustomDialogueMsg();
            showCustomDialogueMsg.Dialogue = _dialogue;
            showCustomDialogueMsg.Owner = _controller.gameObject;
            _controller.gameObject.SendMessage(showCustomDialogueMsg);
            MessageFactory.CacheMessage(showCustomDialogueMsg);
            _showRoutine = null;
        }

        private void ResetDialogueState()
        {
            var setAdventureUnitStateMsg = MessageFactory.GenerateSetAdventureUnitStateMsg();
            setAdventureUnitStateMsg.State = AdventureUnitState.Idle;
            _controller.gameObject.SendMessageTo(setAdventureUnitStateMsg, _controller.transform.parent.gameObject);
            _controller.gameObject.SendMessageTo(setAdventureUnitStateMsg, WorldAdventureController.Player);
            MessageFactory.CacheMessage(setAdventureUnitStateMsg);
            WorldAdventureController.SetAdventureState(AdventureState.Overworld);

            _showRoutine = null;
        }

        private void SubscribeToMessages()
        {
            _controller.gameObject.SubscribeWithFilter<DialogueClosedMessage>(DialogueClosed, _instanceId);
            _controller.transform.parent.gameObject.SubscribeWithFilter<PlayerInteractMessage>(PlayerInteract, _instanceId);
            _controller.transform.parent.gameObject.SubscribeWithFilter<UpdateAdventureUnitStateMessage>(UpdateAdventureUnitState, _instanceId);
            _controller.transform.parent.gameObject.SubscribeWithFilter<SetCustomDialogueMessage>(SetCustomDialogue, _instanceId);
        }

        private void PlayerInteract(PlayerInteractMessage msg)
        {
            if (_unitState != AdventureUnitState.Interaction)
            {
                var direction = (WorldAdventureController.Player.transform.position.ToVector2() - _controller.transform.parent.position.ToVector2()).normalized.ToVector2Int();
                var setFacingDirectionMsg = MessageFactory.GenerateSetFaceDirectionMsg();
                setFacingDirectionMsg.Direction = direction;
                _controller.gameObject.SendMessageTo(setFacingDirectionMsg, _controller.transform.parent.gameObject);
                MessageFactory.CacheMessage(setFacingDirectionMsg);

                var setAdventureUnitStateMsg = MessageFactory.GenerateSetAdventureUnitStateMsg();
                setAdventureUnitStateMsg.State = AdventureUnitState.Interaction;
                _controller.gameObject.SendMessageTo(setAdventureUnitStateMsg, _controller.transform.parent.gameObject);
                _controller.gameObject.SendMessageTo(setAdventureUnitStateMsg, WorldAdventureController.Player);
                MessageFactory.CacheMessage(setAdventureUnitStateMsg);

                WorldAdventureController.SetAdventureState(AdventureState.Dialogue);
                _active = true;
                _showRoutine = _controller.StartCoroutine(StaticMethods.WaitForFrames(1, ShowDialogue));
            }
        }

        private void UpdateAdventureUnitState(UpdateAdventureUnitStateMessage msg)
        {
            _unitState = msg.State;
        }

        private void DialogueClosed(DialogueClosedMessage msg)
        {
            if (_active)
            {
                _active = false;
                _showRoutine = _controller.StartCoroutine(StaticMethods.WaitForFrames(1, ResetDialogueState));
            }

        }

        private void SetCustomDialogue(SetCustomDialogueMessage msg)
        {
            _dialogue = msg.Dialogue;
        }

        public override void Destroy()
        {
            if (_showRoutine != null)
            {
                _controller.StopCoroutine(_showRoutine);
            }
            base.Destroy();
        }
    }
}