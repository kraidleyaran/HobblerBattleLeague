using Assets.Resources.Ancible_Tools.Scripts.System.Adventure;
using Assets.Resources.Ancible_Tools.Scripts.System.BattleLeague;
using Assets.Resources.Ancible_Tools.Scripts.System.Windows;
using MessageBusLib;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.Resources.Ancible_Tools.Scripts.System.UI.Roster
{
    public class UiRosterWindow : UiBaseWindow
    {
        public override bool Movable => true;
        public override bool Static => true;

        [SerializeField] private Button _enterWorldButton;
        [SerializeField] private Button _returnHomeButton;

        public override void Awake()
        {
            base.Awake();
            //_enterWorldButton.interactable = WorldHobblerManager.Roster.Count > 0;
            _enterWorldButton.gameObject.SetActive(WorldController.State == WorldState.World);
            _returnHomeButton.gameObject.SetActive(WorldController.State == WorldState.Adventure);
            SubscribeToMessages();
        }

        public void Battle()
        {
            if (WorldHobblerManager.GetAvailableRoster().Length <= 0)
            {
                UiController.ShowConfirmationAlert("Your roster doesn't have any Hobblers who can battle. If you are challenged, you will immiedately lose and be returned to your last checkpoint. Continue?", IconFactoryController.DefaultAlertIcon, ConfirmEnterWorld, ColorFactoryController.ErrorAlertText);
            }
            else
            {
                ConfirmEnterWorld();
            }

        }

        public void ReturnHome()
        {
            if (WorldController.State == WorldState.Adventure)
            {
                var playerState = AdventureUnitState.Idle;

                var queryAdventureUnitStateMsg = MessageFactory.GenerateQueryAdventureUnitStateMsg();
                queryAdventureUnitStateMsg.DoAfter = state => playerState = state;
                gameObject.SendMessageTo(queryAdventureUnitStateMsg, WorldAdventureController.Player);
                MessageFactory.CacheMessage(queryAdventureUnitStateMsg);

                if (playerState == AdventureUnitState.Idle)
                {
                    WorldController.SetWorldState(WorldState.World);
                    Close();
                }
            }
        }

        private void ConfirmEnterWorld()
        {
            WorldController.SetWorldState(WorldState.Adventure);
            Close();

        }

        private void SubscribeToMessages()
        {
            //gameObject.Subscribe<WorldPopulationUpdatedMessage>(WorldPopulationUpdated);
        }

        private void WorldPopulationUpdated(WorldPopulationUpdatedMessage msg)
        {
            _enterWorldButton.interactable = WorldHobblerManager.Roster.Count > 0;
        }

        

        
    }
}