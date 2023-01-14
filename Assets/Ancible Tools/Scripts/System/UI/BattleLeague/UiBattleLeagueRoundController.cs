using MessageBusLib;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.Resources.Ancible_Tools.Scripts.System.UI.BattleLeague
{
    public class UiBattleLeagueRoundController : MonoBehaviour
    {
        [SerializeField] private Text _roundText = null;
        [SerializeField] private Text _battleStateText = null;

        void Awake()
        {

        }

        private void SubscribeToMessages()
        {
            gameObject.Subscribe<UpdateBattleRoundMessage>(UpdateBattleRound);
            gameObject.Subscribe<UpdateBattleStateMessage>(UpdateBattleState);
        }

        private void UpdateBattleRound(UpdateBattleRoundMessage msg)
        {
            _roundText.text = $"Round {msg.Round + 1}";
        }

        private void UpdateBattleState(UpdateBattleStateMessage msg)
        {
            switch (msg.State)
            {
                case System.BattleLeague.BattleState.Prep:
                    _battleStateText.text = "Preapre!";
                    break;
                case System.BattleLeague.BattleState.Countdown:
                    _battleStateText.text = "Get ready to fight!";
                    break;
                case System.BattleLeague.BattleState.Battle:
                    _battleStateText.text = "Battle";
                    break;
            }
        }


        void OnDestroy()
        {
            gameObject.UnsubscribeFromAllMessages();
        }
    }
}