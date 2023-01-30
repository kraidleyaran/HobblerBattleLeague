using System.Collections.Generic;
using Assets.Ancible_Tools.Scripts.System.UI.WorldInfo;
using Assets.Resources.Ancible_Tools.Scripts.System.Adventure;
using Assets.Resources.Ancible_Tools.Scripts.System.UI;
using Assets.Resources.Ancible_Tools.Scripts.System.Windows;
using MessageBusLib;
using UnityEngine;

namespace Assets.Resources.Ancible_Tools.Scripts.System.BattleLeague
{
    public class BattleLeagueTestController : MonoBehaviour
    {
        [SerializeField] private BattleUnitData[] _leftSideUnits = new BattleUnitData[0];
        [SerializeField] private BattleEncounter _encounter = null;
        [SerializeField] private UiBaseWindow[] _battleWindows = new UiBaseWindow[0];

        [SerializeField] private BattleLeagueController _battleLeagueController = null;
        [SerializeField] private AdventureMap _testMap = null;

        private UiBaseWindow[] _openBattleWindows = new UiBaseWindow[0];

        void Start()
        {
            UiWorldInfoManager.SetActive(false);
            WorldAdventureController.Setup(_testMap, _testMap.DefaultTile);
            //var windows = new List<UiBaseWindow>();
            //for (var i = 0; i < _battleWindows.Length; i++)
            //{
            //    windows.Add(UiWindowManager.OpenWindow(_battleWindows[i]));
            //}

            //_openBattleWindows = windows.ToArray();
            //BattleLeagueCameraController.SetActive(true);
            //_battleLeagueController.Setup(_leftSideUnits, _encounter);
            SubscribeToMessages();
        }

        private void SubscribeToMessages()
        {
            gameObject.Subscribe<CloseBattleMessage>(CloseBattle);
        }

        private void CloseBattle(CloseBattleMessage msg)
        {
            _battleLeagueController.Clear();
            for (var i = 0; i < _openBattleWindows.Length; i++)
            {
                UiWindowManager.CloseWindow(_openBattleWindows[i], _openBattleWindows[i].WorldName);
            }
            _openBattleWindows = new UiBaseWindow[0];
        }

        void OnDestroy()
        {
            gameObject.UnsubscribeFromAllMessages();
        }
    }
}