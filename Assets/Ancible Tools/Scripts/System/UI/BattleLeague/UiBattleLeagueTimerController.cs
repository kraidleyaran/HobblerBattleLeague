using Assets.Resources.Ancible_Tools.Scripts.System.BattleLeague;
using Assets.Resources.Ancible_Tools.Scripts.System.Windows;
using MessageBusLib;
using UnityEngine;

namespace Assets.Resources.Ancible_Tools.Scripts.System.UI.BattleLeague
{
    public class UiBattleLeagueTimerController : UiBaseWindow
    {
        public override bool Movable => false;
        public override bool Static => true;

        private static UiBattleLeagueTimerController _instance = null;

        [SerializeField] private UiFillBarController _fillBarController = null;
        [SerializeField] private Color _prepFillColor = Color.yellow;
        [SerializeField] private Color _preBattleColor = Color.red;
        [SerializeField] private Color _battleColor = Color.red;
        [SerializeField] private Color _roundEndColor = Color.green;

        public override void Awake()
        {
            if (_instance)
            {
                Destroy(_instance.gameObject);
            }

            _instance = this;
            gameObject.SetActive(false);
        }

        public static void UpdateTimer(int currentTicks, int maxTicks, BattleState state, int round, bool isOvertime)
        {
            var remainingTicks = (maxTicks - currentTicks);
            var percent = (float) remainingTicks / maxTicks;
            //var seconds = (int) (remainingTicks * TickController.TickRate);
            var color = _instance._prepFillColor;

            var text = $"Round {round + 1}{(isOvertime ? " (OT)" : string.Empty)} -";
            switch (state)
            {
                case BattleState.Prep:
                    text = $"{text} Prepare!";
                    break;
                case BattleState.Countdown:
                    text = $"{text} Get Ready!";
                    color = _instance._preBattleColor;
                    break;
                case BattleState.Battle:
                    text = $"{text} Battle!";
                    color = _instance._battleColor;
                    break;
                case BattleState.Results:
                    text = $"{text} Completed!";
                    color = _instance._roundEndColor;
                    break;
                case BattleState.End:
                    break;
            }
            _instance._fillBarController.Setup(percent, text, color);
        }

        public static void SetActive(bool active)
        {
            _instance.gameObject.SetActive(active);
        }

        void OnDestroy()
        {
            if (_instance && _instance == this)
            {
                _instance = null;
            }
        }
    }
}