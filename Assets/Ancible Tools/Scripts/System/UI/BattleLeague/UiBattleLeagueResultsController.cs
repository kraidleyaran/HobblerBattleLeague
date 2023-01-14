using System.Collections.Generic;
using System.Linq;
using Assets.Ancible_Tools.Scripts.System.UI.BattleLeague.Roster;
using Assets.Resources.Ancible_Tools.Scripts.System.BattleLeague;
using Assets.Resources.Ancible_Tools.Scripts.System.Windows;
using MessageBusLib;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.Resources.Ancible_Tools.Scripts.System.UI.BattleLeague
{
    public class UiBattleLeagueResultsController : UiBaseWindow
    {
        public override bool Movable => false;
        public override bool Static => true;

        [SerializeField] private Text _resultsText = null;
        [SerializeField] private Text _roundsText = null;
        [SerializeField] private Color _victoryTextColor = Color.green;
        [SerializeField] private Color _defeatTextColor = Color.red;
        [SerializeField] private Color _abandonTextColor = Color.yellow;
        [SerializeField] private Text _leftSideScoreText = null;
        [SerializeField] private Text _rightSideScoreText = null;
        [SerializeField] private UiRosterController _leftSideRoster = null;
        [SerializeField] private UiRosterController _rightSideRoster = null;

        public void Setup(KeyValuePair<BattleUnitData, BattleAlignment>[] units, int leftScore, int rightScore, BattleResult result, int totalRounds)
        {
            if (totalRounds > 0)
            {
                SetRounds(totalRounds);
            }
            else
            {
                _roundsText.gameObject.SetActive(false);
            }
            switch (result)
            {
                case BattleResult.Victory:
                    _resultsText.text = "Victory!";
                    _resultsText.color = _victoryTextColor;
                    break;
                case BattleResult.Defeat:
                    _resultsText.text = $"Defeat";
                    _resultsText.color = _defeatTextColor;
                    break;
                case BattleResult.Abandon:
                    _resultsText.text = $"Abandoned";
                    _resultsText.color = _abandonTextColor;
                    break;
            }
            _leftSideRoster.Setup(units.Where(u => u.Value == BattleAlignment.Left).Select(u => u.Key).ToArray()); ;
            _rightSideRoster.Setup(units.Where(u => u.Value == BattleAlignment.Right).Select(u => u.Key).ToArray());
            _leftSideScoreText.text = $"{leftScore}";
            _rightSideScoreText.text = $"{rightScore}";
        }

        private void SetRounds(int rounds)
        {
            _roundsText.text = rounds > 1 ? $"{rounds} Rounds" : $"1 Round";
        }

        public override void Close()
        {
            gameObject.SendMessage(CloseBattleMessage.INSTANCE);
            base.Close();
        }
    }
}