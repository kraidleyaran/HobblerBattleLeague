using Assets.Resources.Ancible_Tools.Scripts.System.Windows;
using MessageBusLib;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.Resources.Ancible_Tools.Scripts.System.UI.BattleLeague
{
    public class UiBattleLeagueScoreController : UiBaseWindow
    {
        public override bool Movable => false;

        private static UiBattleLeagueScoreController _instance = null;

        [SerializeField] private Text _leftScoreText;
        [SerializeField] private Text _rightScoreText;
        [SerializeField] private Text _requiredPointsText;
        [SerializeField] private Text _maxRoundsText = null;
        [SerializeField] private Button _readyButton = null;

        public override void Awake()
        {
            if (_instance)
            {
                Destroy(_instance.gameObject);
                return;
            }

            _instance = this;
            _leftScoreText.text = "0";
            _rightScoreText.text = "0";
            SubscribeToMessages();
            base.Awake();
        }

        public static void SetReadyButtonInteractable(bool interactable)
        {
            _instance._readyButton.interactable = interactable;
        }

        public static void SetReadyButtonAvailable(bool available)
        {
            _instance._readyButton.gameObject.SetActive(available);
        }

        private void SubscribeToMessages()
        {
            gameObject.Subscribe<UpdateBattleLeagueScoreMessage>(UpdateBattleLeagueScore);
            gameObject.Subscribe<UpdateBattlePointRequirementMessage>(UpdateBattlePointRequirement);
        }

        private void UpdateBattleLeagueScore(UpdateBattleLeagueScoreMessage msg)
        {
            _leftScoreText.text = $"{msg.LeftSide}";
            _rightScoreText.text = $"{msg.RightSide}";
        }

        private void UpdateBattlePointRequirement(UpdateBattlePointRequirementMessage msg)
        {
            _requiredPointsText.text = $"{msg.Requirement}";
            _maxRoundsText.text = msg.Rounds > 0 ? $"{msg.Rounds} Rounds" : "Score Limit";
        }

        public void ReadyClicked()
        {
            gameObject.SendMessage(PlayerReadyForBattleMessage.INSTANCE);
        }
    }
}