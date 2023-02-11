using Assets.Resources.Ancible_Tools.Scripts.System;
using Assets.Resources.Ancible_Tools.Scripts.System.UI;
using MessageBusLib;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.Ancible_Tools.Scripts.System.UI.WorldInfo
{
    public class UiWorldPopulationController : MonoBehaviour
    {
        [SerializeField] private Text _populationText = null;
        [SerializeField] private Text _rosterText = null;

        void Awake()
        {
            SubscribeToMessages();
        }

        void Start()
        {
            RefreshPopulation();
        }

        public void ToggleRoster()
        {
            if (WorldController.State == WorldState.World)
            {
                UiController.ToggleRosterWindow();
            }
        }

        private void RefreshPopulation()
        {
            _populationText.text = $"{WorldHobblerManager.All.Count}/{WorldHobblerManager.PopulationLimit}";
            _rosterText.text = $"{WorldHobblerManager.Roster.Count}/{WorldHobblerManager.RosterLimit}";
        }

        private void SubscribeToMessages()
        {
            gameObject.Subscribe<WorldPopulationUpdatedMessage>(WorldPopulationUpdated);
        }

        private void WorldPopulationUpdated(WorldPopulationUpdatedMessage msg)
        {
            RefreshPopulation();
        }

        void OnDestroy()
        {
            gameObject.UnsubscribeFromAllMessages();
        }
    }
}