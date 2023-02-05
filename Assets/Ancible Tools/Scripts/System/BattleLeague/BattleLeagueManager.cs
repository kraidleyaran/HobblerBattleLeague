using System.Collections.Generic;
using System.Linq;
using Assets.Resources.Ancible_Tools.Scripts.System.Items;
using Assets.Resources.Ancible_Tools.Scripts.System.Windows;
using MessageBusLib;
using UnityEngine;

namespace Assets.Resources.Ancible_Tools.Scripts.System.BattleLeague
{
    public class BattleLeagueManager : MonoBehaviour
    {
        public static BattleEncounter DefaultEncounter => _instance._encounter;

        private static BattleLeagueManager _instance = null;

        [SerializeField] private BattleLeagueCameraController _camera = null;
        [SerializeField] private UiBaseWindow[] _battleWindows = new UiBaseWindow[0];
        [SerializeField] private BattleLeagueController _battleLeagueController = null;
        [SerializeField] private BattleEncounter _encounter = null;
        
        private QueryBattleUnitDataMessage _queryBattleUnitDataMsg = new QueryBattleUnitDataMessage();
        private ApplyHobblerBattleDataMessage _applyHobblerBattleDataMsg = new ApplyHobblerBattleDataMessage();
        private EncounterFinishedMessage _encounterFinished = new EncounterFinishedMessage();

        private UiBaseWindow[] _openBattleWindows = new UiBaseWindow[0];
        private BattleResult _result = BattleResult.Abandon;
        private int _pointsScored = 0;
        private int _roundsPlayed = 0;
        private bool _repeat = false;

        private Dictionary<BattleUnitData, GameObject> _hobblers = new Dictionary<BattleUnitData, GameObject>();

        private List<BattleEncounter> _finishedEncounters = new List<BattleEncounter>();

        private BattleEncounter _currentEncounter = null;
        private GameObject _encounterUnit = null;

        void Awake()
        {
            if (_instance)
            {
                Destroy(gameObject);
                return;
            }

            _instance = this;
            _battleLeagueController.WakeUp();
            _battleLeagueController.gameObject.SetActive(false);
            _camera.WakeUp();
            SubscribeToMessages();
        }

        public static void SetupEncounter(BattleEncounter encounter, GameObject encounterUnit)
        {
            _instance._hobblers.Clear();
            _instance._currentEncounter = encounter;
            WorldController.SetWorldState(WorldState.Battle);
            var windows = new List<UiBaseWindow>();
            for (var i = 0; i < _instance._battleWindows.Length; i++)
            {
                windows.Add(UiWindowManager.OpenWindow(_instance._battleWindows[i]));
            }

            _instance._openBattleWindows = windows.ToArray();
            BattleLeagueCameraController.SetActive(true);

            var roster = WorldHobblerManager.Roster.ToArray();

            var setMonsterStateMsg = MessageFactory.GenerateSetMonsterStateMsg();
            setMonsterStateMsg.State = MonsterState.Battle;
            
            
            for (var i = 0; i < roster.Length; i++)
            {
                var obj = roster[i];
                _instance._queryBattleUnitDataMsg.DoAfter = data =>
                {
                    _instance._hobblers.Add(data, obj);
                };
                _instance.gameObject.SendMessageTo(_instance._queryBattleUnitDataMsg, roster[i]);
                _instance.gameObject.SendMessageTo(setMonsterStateMsg, obj);
            }
            MessageFactory.CacheMessage(setMonsterStateMsg);
            _instance._encounterUnit = encounterUnit;
            _instance._battleLeagueController.Setup(_instance._hobblers.Keys.ToArray(), encounter);
        }

        private void SubscribeToMessages()
        {
            gameObject.Subscribe<ShowBattleResultsWindowMessage>(ShowBattleResultsWindow);
            gameObject.Subscribe<CloseBattleMessage>(CloseBattle);
        }

        private void CloseBattle(CloseBattleMessage msg)
        {
            var experience = 0;
            switch (_result)
            {
                case BattleResult.Victory:
                    if (_currentEncounter)
                    {
                        var loot = _currentEncounter.GenerateLoot(_currentEncounter.Save && _finishedEncounters.Contains(_currentEncounter));
                        if (loot.Length > 0)
                        {
                            for (var i = 0; i < loot.Length; i++)
                            {
                                WorldStashController.AddItem(loot[i].Item, loot[i].Stack);
                            }
                        }
                        experience = Mathf.Max(1, Mathf.RoundToInt(_currentEncounter.VictoryExperiencePerPoint * _pointsScored));
                    }
                    break;
                case BattleResult.Defeat:
                    experience = Mathf.Max(1, Mathf.RoundToInt(_currentEncounter.DefeatExperiencePerPoint * _pointsScored));
                    break;
                case BattleResult.Abandon:
                    break;
            }

            if (!_finishedEncounters.Contains(_currentEncounter))
            {
                _finishedEncounters.Add(_currentEncounter);
            }
            var objs = _hobblers.ToArray();
            var addExperienceMsg = MessageFactory.GenerateAddExperienceMsg();
            addExperienceMsg.Amount = experience;

            _applyHobblerBattleDataMsg.Result = _result;
            _applyHobblerBattleDataMsg.MatchId = _battleLeagueController.MatchId;
            for (var i = 0; i < objs.Length; i++)
            {
                _applyHobblerBattleDataMsg.Data = objs[i].Key;

                gameObject.SendMessageTo(addExperienceMsg, objs[i].Value);
                gameObject.SendMessageTo(_applyHobblerBattleDataMsg, objs[i].Value);
                objs[i].Key.Dispose();
            }
            MessageFactory.CacheMessage(addExperienceMsg);
            _encounterFinished.Result = _result;
            _applyHobblerBattleDataMsg.Data = null;
            _result = BattleResult.Abandon;
            _pointsScored = 0;
            _hobblers.Clear();
            _battleLeagueController.Clear();
            _currentEncounter = null;
            for (var i = 0; i < _openBattleWindows.Length; i++)
            {
                UiWindowManager.CloseWindow(_openBattleWindows[i], _openBattleWindows[i].WorldName);
            }
            _openBattleWindows = new UiBaseWindow[0];
            BattleLeagueCameraController.SetActive(false);
            if (_encounterUnit)
            {
                gameObject.SendMessageTo(_encounterFinished,_encounterUnit);
                _encounterUnit = null;
            }
            if (WorldController.State == WorldState.Battle)
            {
                WorldController.SetWorldState(WorldState.Adventure);
            }

        }

        private void ShowBattleResultsWindow(ShowBattleResultsWindowMessage msg)
        {
            _result = msg.Result;
            _pointsScored = msg.LeftScore;
            _roundsPlayed = msg.TotalRounds;
        }

        void OnDestroy()
        {
            if (_instance && _instance == this)
            {
                gameObject.UnsubscribeFromAllMessages();
            }
        }
    }
}