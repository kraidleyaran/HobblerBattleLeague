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

        public override void Awake()
        {
            base.Awake();
            _enterWorldButton.interactable = WorldHobblerManager.Roster.Count > 0;
            SubscribeToMessages();
        }

        public void Battle()
        {
            WorldController.SetWorldState(WorldState.Adventure);
        }

        private void SubscribeToMessages()
        {
            gameObject.Subscribe<WorldPopulationUpdatedMessage>(WorldPopulationUpdated);
        }

        private void WorldPopulationUpdated(WorldPopulationUpdatedMessage msg)
        {
            _enterWorldButton.interactable = WorldHobblerManager.Roster.Count > 0;
        }
    }
}