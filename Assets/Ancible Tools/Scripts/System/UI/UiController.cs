using Assets.Ancible_Tools.Scripts.System.UI.BattleLeague.Roster;
using Assets.Resources.Ancible_Tools.Scripts.System.UI.BattleLeague;
using Assets.Resources.Ancible_Tools.Scripts.System.UI.Building;
using Assets.Resources.Ancible_Tools.Scripts.System.UI.DetailedInfo;
using Assets.Resources.Ancible_Tools.Scripts.System.UI.Dialogue;
using Assets.Resources.Ancible_Tools.Scripts.System.UI.HobblerGenerator;
using Assets.Resources.Ancible_Tools.Scripts.System.UI.Roster;
using Assets.Resources.Ancible_Tools.Scripts.System.UI.Stash;
using Assets.Resources.Ancible_Tools.Scripts.System.Windows;
using MessageBusLib;
using UnityEngine;

namespace Assets.Resources.Ancible_Tools.Scripts.System.UI
{
    public class UiController : MonoBehaviour
    {
        private static UiController _instance = null;

        [Header("Window Templates")]
        [SerializeField] private UiDetailedHobblerInfoWindowController _detailedHobblerInfoTemplate;
        [SerializeField] private UiStashWindowController _stashWindowTemplate;
        [SerializeField] private UiBattleLeagueResultsController _battleResultsTemplate;
        [SerializeField] private UiBuildingWindowController _buildingWindowTemplate;
        [SerializeField] private UiHobGeneratorWindowController _hobGeneratorWindowTemplate;
        [SerializeField] private UiRosterWindow _rosterWindowTemplate;
        [SerializeField] private UiDialogueWindowController _dialogueWindowTemplate;

        void Awake()
        {
            if (_instance)
            {
                Destroy(gameObject);
                return;
            }

            _instance = this;
            SubscribeToMessages();
        }

        private void SubscribeToMessages()
        {
            gameObject.Subscribe<ShowDetailedHobblerInfoMessage>(ShowDetailedHobblerInfo);
            gameObject.Subscribe<ToggleStashWindowMessage>(ToggleStashWindow);
            gameObject.Subscribe<UpdateInputStateMessage>(UpdateInputState);
            gameObject.Subscribe<ShowBattleResultsWindowMessage>(ShowBattleResultsWindow);
            gameObject.Subscribe<ShowHobGeneratorWindowMessage>(ShowHobGeneratorWindow);
            gameObject.Subscribe<ShowDialogueMessage>(ShowDialogue);
        }

        private void ShowDetailedHobblerInfo(ShowDetailedHobblerInfoMessage msg)
        {
            var window = UiWindowManager.OpenWindow(_detailedHobblerInfoTemplate, $"{msg.Unit.GetInstanceID()}");
            window.Setup(msg.Unit);
        }

        private void ToggleStashWindow(ToggleStashWindowMessage msg)
        {
            UiWindowManager.ToggleWindow(_stashWindowTemplate);
        }

        private void UpdateInputState(UpdateInputStateMessage msg)
        {
            if (!msg.Previous.Stash && msg.Current.Stash && WorldController.State == WorldState.World)
            {
                UiWindowManager.ToggleWindow(_stashWindowTemplate);
            }

            if (!msg.Previous.Build && msg.Current.Build && WorldController.State == WorldState.World)
            {
                UiWindowManager.ToggleWindow(_buildingWindowTemplate);
            }

            if(!msg.Previous.Roster && msg.Current.Roster && WorldController.State == WorldState.World)
            {
                UiWindowManager.ToggleWindow(_rosterWindowTemplate);
            }
        }

        private void ShowBattleResultsWindow(ShowBattleResultsWindowMessage msg)
        {
            var window = UiWindowManager.OpenWindow(_battleResultsTemplate);
            window.Setup(msg.Units, msg.LeftScore, msg.RightScore, msg.Result, msg.TotalRounds);
        }

        private void ShowHobGeneratorWindow(ShowHobGeneratorWindowMessage msg)
        {
            var window = UiWindowManager.OpenWindow(_hobGeneratorWindowTemplate, $"{msg.Owner.GetInstanceID()}");
            window.Setup(msg.Owner);
        }

        private void ToggleRosterWindow(ToggleRosterWindowMessage msg)
        {
            UiWindowManager.ToggleWindow(_rosterWindowTemplate);
        }

        private void ShowDialogue(ShowDialogueMessage msg)
        {
            var window = UiWindowManager.OpenWindow(_dialogueWindowTemplate);
            window.Setup(msg.Dialogue, msg.Owner);
        }

        void OnDestroy()
        {
            gameObject.UnsubscribeFromAllMessages();
        }
    }
}