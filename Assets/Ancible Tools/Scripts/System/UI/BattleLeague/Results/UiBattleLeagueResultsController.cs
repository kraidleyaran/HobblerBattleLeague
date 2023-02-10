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

        [SerializeField] private UiBattleResultsController _battleResultsController = null;
        [SerializeField] private UiBattleExperienceResultsController _experienceResultsController = null;
        [SerializeField] private UiBattleLootResultsController _battleLootResultsController = null;

        public void Setup(ShowBattleResultsWindowMessage msg)
        {
            _battleResultsController.Setup(msg.Units, msg.LeftScore, msg.RightScore, msg.Result, msg.TotalRounds);
            var hobblers = msg.Hobblers.Select(kv => kv.Value).ToArray();
            _experienceResultsController.Setup(hobblers, msg.TotalExperience);
            _battleLootResultsController.Setup(msg.Items);
        }
        
        public void ShowExperience()
        {
            _battleResultsController.gameObject.SetActive(false);
            _experienceResultsController.Activate();
        }

        public void ShowItems()
        {
            _experienceResultsController.gameObject.SetActive(false);
            _battleLootResultsController.gameObject.SetActive(true);
        }

        public override void Close()
        {
            gameObject.SendMessage(CloseBattleMessage.INSTANCE);
            base.Close();
        }
    }
}