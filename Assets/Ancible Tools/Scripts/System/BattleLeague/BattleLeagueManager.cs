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
        [SerializeField] private float _manaPerDamageDone = 0f;
        [SerializeField] private float _manaPerDamageTaken = 0f;
        
        private QueryBattleUnitDataMessage _queryBattleUnitDataMsg = new QueryBattleUnitDataMessage();
        private ApplyHobblerBattleDataMessage _applyHobblerBattleDataMsg = new ApplyHobblerBattleDataMessage();
        private EncounterFinishedMessage _encounterFinished = new EncounterFinishedMessage();
        private ShowBattleResultsWindowMessage _showBattleResultsWindowMsg = new ShowBattleResultsWindowMessage();

        private UiBaseWindow[] _openBattleWindows = new UiBaseWindow[0];
        private BattleResult _result = BattleResult.Abandon;
        private int _pointsScored = 0;
        private int _roundsPlayed = 0;
        private bool _repeat = false;
        private ItemStack[] _loot = new ItemStack[0];
        private int _experienceGained = 0;

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

        public static void EncounterFinished(int leftScore, int rightScore, KeyValuePair<BattleUnitData, BattleAlignment>[] units, int totalRounds, BattleResult result)
        {
            switch (result)
            {
                case BattleResult.Victory:
                    if (_instance._currentEncounter)
                    {
                        var encounter = _instance._currentEncounter;
                        _instance._loot = encounter.GenerateLoot(encounter.Save && _instance._finishedEncounters.Contains(encounter));
                        _instance._experienceGained = Mathf.Max(1, Mathf.RoundToInt(encounter.VictoryExperiencePerPoint * leftScore));
                    }
                    break;
                case BattleResult.Defeat:
                    _instance._experienceGained = Mathf.Max(1, Mathf.RoundToInt(_instance._currentEncounter.DefeatExperiencePerPoint * leftScore));
                    break;
                case BattleResult.Abandon:
                    break;
            }
            _instance._showBattleResultsWindowMsg.Result = result;
            _instance._showBattleResultsWindowMsg.LeftScore = leftScore;
            _instance._showBattleResultsWindowMsg.RightScore = rightScore;
            _instance._showBattleResultsWindowMsg.Units = units;
            _instance._showBattleResultsWindowMsg.TotalRounds = totalRounds;
            _instance._showBattleResultsWindowMsg.TotalExperience = _instance._experienceGained;
            _instance._showBattleResultsWindowMsg.Items = _instance._loot;
            _instance._showBattleResultsWindowMsg.Hobblers = _instance._hobblers.ToArray();
            _instance.gameObject.SendMessage(_instance._showBattleResultsWindowMsg);
        }

        public static int GetManaFromDamageTaken(int amount)
        {
            return Mathf.Max(Mathf.RoundToInt(amount * _instance._manaPerDamageTaken), 1);
        }

        public static int GetManaFromDamageDone(int amount)
        {
            return Mathf.Max(Mathf.RoundToInt(amount * _instance._manaPerDamageDone), 1);
        }

        private void SubscribeToMessages()
        {
            gameObject.Subscribe<ShowBattleResultsWindowMessage>(ShowBattleResultsWindow);
            gameObject.Subscribe<CloseBattleMessage>(CloseBattle);
        }

        private void CloseBattle(CloseBattleMessage msg)
        {
            switch (_result)
            {
                case BattleResult.Victory:
                    if (_loot.Length > 0)
                    {
                        foreach (var item in _loot)
                        {
                            WorldStashController.AddItem(item.Item, item.Stack);
                        }
                    }
                    break;
                case BattleResult.Defeat:
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
            addExperienceMsg.Amount = _experienceGained;

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
            _loot = new ItemStack[0];
            _experienceGained = 0;
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