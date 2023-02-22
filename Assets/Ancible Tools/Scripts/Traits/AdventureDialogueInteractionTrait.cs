using Assets.Ancible_Tools.Scripts.System.Dialogue;
using Assets.Resources.Ancible_Tools.Scripts.System;
using Assets.Resources.Ancible_Tools.Scripts.System.Adventure;
using Assets.Resources.Ancible_Tools.Scripts.System.Pathing;
using Assets.Resources.Ancible_Tools.Scripts.System.SaveData;
using MessageBusLib;
using UnityEngine;

namespace Assets.Ancible_Tools.Scripts.Traits
{
    [CreateAssetMenu(fileName = "Adventure Dialogue Trait", menuName = "Ancible Tools/Traits/Adventure/Interaction/Dialogue")]
    public class AdventureDialogueInteractionTrait : Trait
    {
        [SerializeField] private DialogueData _dialogue = null;
        public bool Save;
        [HideInInspector] public string SaveId;

        private AdventureUnitState _unitState = AdventureUnitState.Idle;

        private Coroutine _showRoutine = null;
        private bool _active = false;

        public override void SetupController(TraitController controller)
        {
            base.SetupController(controller);
            if (Save && !string.IsNullOrEmpty(SaveId))
            {
                var data = PlayerDataController.GetDialogueDataById(SaveId);
                if (data != null)
                {
                    var dialogue = DialogueFactory.GetDialogueFromName(data.Dialogue);
                    if (dialogue)
                    {
                        _dialogue = dialogue;
                    }
                }
            }
            SubscribeToMessages();
        }

        private void ShowDialoge()
        {
            var showDialogueMsg = MessageFactory.GenerateShowDialogueMsg();
            showDialogueMsg.Dialogue = _dialogue;
            showDialogueMsg.Owner = _controller.gameObject;
            _controller.gameObject.SendMessage(showDialogueMsg);
            MessageFactory.CacheMessage(showDialogueMsg);
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
            _controller.transform.parent.gameObject.SubscribeWithFilter<SetDialogueMessage>(SetDialogue, _instanceId);
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
                _showRoutine = _controller.StartCoroutine(StaticMethods.WaitForFrames(1, ShowDialoge));
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

        private void SetDialogue(SetDialogueMessage msg)
        {
            _dialogue = msg.Dialogue;
            if (Save && !string.IsNullOrEmpty(SaveId))
            {
                PlayerDataController.SetDialogueData(SaveId, _dialogue.name);
            }
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