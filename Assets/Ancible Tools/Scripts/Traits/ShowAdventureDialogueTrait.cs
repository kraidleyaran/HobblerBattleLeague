using Assets.Ancible_Tools.Scripts.System.Dialogue;
using Assets.Resources.Ancible_Tools.Scripts.System;
using Assets.Resources.Ancible_Tools.Scripts.System.Adventure;
using MessageBusLib;
using UnityEngine;

namespace Assets.Ancible_Tools.Scripts.Traits
{
    [CreateAssetMenu(fileName = "Show Adventure Dialogue Trait", menuName = "Ancible Tools/Traits/Adventure/Interaction/Show Adventure Dialogue (Global)")]
    public class ShowAdventureDialogueTrait : Trait
    {
        [SerializeField] private DialogueData _dialogue = null;

        private Coroutine _dialogueRoutine = null;

        public override void SetupController(TraitController controller)
        {
            base.SetupController(controller);
            var setAdventureUnitStateMsg = MessageFactory.GenerateSetAdventureUnitStateMsg();
            setAdventureUnitStateMsg.State = AdventureUnitState.Interaction;
            _controller.gameObject.SendMessageTo(setAdventureUnitStateMsg, WorldAdventureController.Player);
            MessageFactory.CacheMessage(setAdventureUnitStateMsg);

            var showDialogueMsg = MessageFactory.GenerateShowDialogueMsg();
            showDialogueMsg.Dialogue = _dialogue;
            showDialogueMsg.Owner = _controller.gameObject;
            showDialogueMsg.DoAfter = null;
            _controller.gameObject.SendMessage(showDialogueMsg);
            MessageFactory.CacheMessage(showDialogueMsg);

            SubscribeToMessages();
        }

        private void FinishInteract()
        {
            _dialogueRoutine = null;
            var setAdventureUnitStateMsg = MessageFactory.GenerateSetAdventureUnitStateMsg();
            setAdventureUnitStateMsg.State = AdventureUnitState.Idle;
            _controller.gameObject.SendMessageTo(setAdventureUnitStateMsg, WorldAdventureController.Player);
            MessageFactory.CacheMessage(setAdventureUnitStateMsg);

            WorldAdventureController.SetAdventureState(AdventureState.Overworld);

            var removeTraitFromUnitByControllerMsg = MessageFactory.GenerateRemoveTraitFromUnitByControllerMsg();
            removeTraitFromUnitByControllerMsg.Controller = _controller;
            _controller.gameObject.SendMessageTo(removeTraitFromUnitByControllerMsg, _controller.transform.parent.gameObject);
            MessageFactory.CacheMessage(removeTraitFromUnitByControllerMsg);
        }

        private void SubscribeToMessages()
        {
            _controller.gameObject.SubscribeWithFilter<DialogueClosedMessage>(DialogueClosed, _instanceId);
        }

        private void DialogueClosed(DialogueClosedMessage msg)
        {
            _dialogueRoutine = _controller.StartCoroutine(StaticMethods.WaitForFrames(1, FinishInteract));
        }

        public override void Destroy()
        {
            if (_dialogueRoutine != null)
            {
                _controller.StopCoroutine(_dialogueRoutine);
            }
            base.Destroy();
        }
    }
}