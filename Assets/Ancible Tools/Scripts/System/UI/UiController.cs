using System;
using Assets.Ancible_Tools.Scripts.System.UI.BattleLeague.Roster;
using Assets.Resources.Ancible_Tools.Scripts.System.UI.Alerts;
using Assets.Resources.Ancible_Tools.Scripts.System.UI.BattleLeague;
using Assets.Resources.Ancible_Tools.Scripts.System.UI.Building;
using Assets.Resources.Ancible_Tools.Scripts.System.UI.Crafting;
using Assets.Resources.Ancible_Tools.Scripts.System.UI.DetailedInfo;
using Assets.Resources.Ancible_Tools.Scripts.System.UI.Dialogue;
using Assets.Resources.Ancible_Tools.Scripts.System.UI.HobblerGenerator;
using Assets.Resources.Ancible_Tools.Scripts.System.UI.MInigame.MazeSelector;
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
        [SerializeField] private UiMazeSelectionWindowController _mazeSettingsWindowTemplate;
        [SerializeField] private UiCraftingWindowController _craftingWindowTemplate;
        [SerializeField] private UiConfirmationAlertWindowController _confirmationWindowTemplate;

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

        public static void ToggleRosterWindow()
        {
            UiWindowManager.ToggleWindow(_instance._rosterWindowTemplate);
        }

        public static void ToggleStashWindow()
        {
            UiWindowManager.ToggleWindow(_instance._stashWindowTemplate);
        }

        public static void ShowConfirmationAlert(string alertText, Sprite icon, Action onConfirm, Color colorMask)
        {
            var confirmWindow = UiWindowManager.OpenWindow(_instance._confirmationWindowTemplate);
            confirmWindow.Setup(alertText, icon,onConfirm, colorMask);
        }

        private void SubscribeToMessages()
        {
            gameObject.Subscribe<ShowDetailedHobblerInfoMessage>(ShowDetailedHobblerInfo);
            gameObject.Subscribe<ToggleStashWindowMessage>(ToggleStashWindow);
            gameObject.Subscribe<UpdateInputStateMessage>(UpdateInputState);
            gameObject.Subscribe<ShowBattleResultsWindowMessage>(ShowBattleResultsWindow);
            gameObject.Subscribe<ShowHobGeneratorWindowMessage>(ShowHobGeneratorWindow);
            gameObject.Subscribe<ShowDialogueMessage>(ShowDialogue);
            gameObject.Subscribe<ShowMazeSelectionWindowMessage>(ShowMazeSelectionWindow);
            gameObject.Subscribe<ShowCraftingWindowMessage>(ShowCraftingWindow);
            gameObject.Subscribe<ShowCustomDialogueMessage>(ShowCustomDialogue);
        }

        private void ShowDetailedHobblerInfo(ShowDetailedHobblerInfoMessage msg)
        {
            var hobblerId = string.Empty;
            var queryHobblerIdMsg = MessageFactory.GenerateQueryHobblerIdMsg();
            queryHobblerIdMsg.DoAfter = id => hobblerId = id;
            gameObject.SendMessageTo(queryHobblerIdMsg, msg.Unit);
            MessageFactory.CacheMessage(queryHobblerIdMsg);
            if (string.IsNullOrEmpty(hobblerId))
            {
                hobblerId = $"{GetInstanceID()}";
            }
            var window = UiWindowManager.OpenWindow(_detailedHobblerInfoTemplate, $"{UiDetailedHobblerInfoWindowController.FILTER}{hobblerId}");
            window.Setup(msg.Unit);
        }

        private void ToggleStashWindow(ToggleStashWindowMessage msg)
        {
            UiWindowManager.ToggleWindow(_stashWindowTemplate);
        }

        private void UpdateInputState(UpdateInputStateMessage msg)
        {
            if (!msg.Previous.Stash && msg.Current.Stash && (WorldController.State == WorldState.World || WorldController.State == WorldState.Adventure))
            {
                UiWindowManager.ToggleWindow(_stashWindowTemplate);
            }

            if (!msg.Previous.Build && msg.Current.Build && WorldController.State == WorldState.World)
            {
                UiWindowManager.ToggleWindow(_buildingWindowTemplate);
            }

            if(!msg.Previous.Roster && msg.Current.Roster && (WorldController.State == WorldState.Adventure || WorldController.State == WorldState.World))
            {
                UiWindowManager.ToggleWindow(_rosterWindowTemplate);
            }
        }

        private void ShowBattleResultsWindow(ShowBattleResultsWindowMessage msg)
        {
            var window = UiWindowManager.OpenWindow(_battleResultsTemplate);
            window.Setup(msg);
        }

        private void ShowHobGeneratorWindow(ShowHobGeneratorWindowMessage msg)
        {
            var id = string.Empty;
            var queryBuildingMsg = MessageFactory.GenerateQueryBuildingMsg();
            queryBuildingMsg.DoAfter = (building, tile, buildingId) => { id = buildingId; };
            _instance.gameObject.SendMessageTo(queryBuildingMsg, msg.Owner);
            MessageFactory.CacheMessage(queryBuildingMsg);
            if (string.IsNullOrEmpty(id))
            {
                id = $"{msg.Owner.GetInstanceID()}";
            }
            var window = UiWindowManager.OpenWindow(_hobGeneratorWindowTemplate, $"{UiHobGeneratorWindowController.FILTER}{id}");
            window.Setup(msg.Owner);
        }

        private void ToggleRosterWindow(ToggleRosterWindowMessage msg)
        {
            UiWindowManager.ToggleWindow(_rosterWindowTemplate);
        }

        private void ShowDialogue(ShowDialogueMessage msg)
        {
            var window = UiWindowManager.OpenWindow(_dialogueWindowTemplate);
            window.Setup(msg.Dialogue, msg.Owner, msg.DoAfter);
        }

        private void ShowMazeSelectionWindow(ShowMazeSelectionWindowMessage msg)
        {
            var id = string.Empty;
            var queryHobblerIdMsg = MessageFactory.GenerateQueryHobblerIdMsg();
            queryHobblerIdMsg.DoAfter = hobblerId => id = hobblerId;
            gameObject.SendMessageTo(queryHobblerIdMsg, msg.Hobbler);
            MessageFactory.CacheMessage(queryHobblerIdMsg);

            if (string.IsNullOrEmpty(id))
            {
                id = $"{msg.Hobbler.GetInstanceID()}";
            }

            var window = UiWindowManager.OpenWindow(_mazeSettingsWindowTemplate, $"{UiMazeSelectionWindowController.FILTER}{id}");
            window.Setup(msg.Hobbler);
        }

        private void ShowCraftingWindow(ShowCraftingWindowMessage msg)
        {
            var id = string.Empty;
            var queryBuildingMsg = MessageFactory.GenerateQueryBuildingMsg();
            queryBuildingMsg.DoAfter = (building, tile, buildingId) => { id = buildingId; };
            _instance.gameObject.SendMessageTo(queryBuildingMsg, msg.Owner);
            MessageFactory.CacheMessage(queryBuildingMsg);

            if (string.IsNullOrEmpty(id))
            {
                id = $"{msg.Owner.GetInstanceID()}";
            }

            var window = UiWindowManager.OpenWindow(_craftingWindowTemplate, $"{UiCraftingWindowController.FILTER}{id}");
            window.Setup(msg.Owner);
        }

        private void ShowCustomDialogue(ShowCustomDialogueMessage msg)
        {
            var window = UiWindowManager.OpenWindow(_dialogueWindowTemplate, $"{msg.Owner.GetInstanceID()}");
            window.Setup(msg.Dialogue, msg.Owner, msg.DoAfter);
        }

        void OnDestroy()
        {
            gameObject.UnsubscribeFromAllMessages();
        }
    }
}