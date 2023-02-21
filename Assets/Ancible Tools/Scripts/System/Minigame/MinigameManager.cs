using System.Collections.Generic;
using Assets.Resources.Ancible_Tools.Scripts.System.Minigame.Settings;
using Assets.Resources.Ancible_Tools.Scripts.System.UI.MInigame;
using Assets.Resources.Ancible_Tools.Scripts.System.Windows;
using MessageBusLib;
using UnityEngine;

namespace Assets.Resources.Ancible_Tools.Scripts.System.Minigame
{
    public class MinigameManager : MonoBehaviour
    {
        public static MazeMinigameSettings[] AvailableMazeSettings => _instance._availableMazeSettings;

        private static MinigameManager _instance = null;

        [SerializeField] private MazeMinigameController _mazeTemplate;
        [SerializeField] private UiMinigameResultsController _resultsTemplate;
        [SerializeField] private MazeMinigameSettings[] _availableMazeSettings = new MazeMinigameSettings[0];

        private MinigameController _minigameController = null;
        private MinigameSettings _currentSettings = null;

        private GameObject _unit = null;
        private UiBaseWindow[] _openWindows = new UiBaseWindow[0];

        void Start()
        {
            if (_instance)
            {
                Destroy(gameObject);
                return;
            }

            _instance = this;
            gameObject.layer = CollisionLayerFactory.Minigame.ToLayer();
            SubscribeToMessages();
        }

        private void SubscribeToMessages()
        {
            gameObject.Subscribe<StartMinigameMessage>(StartMinigame);
            gameObject.Subscribe<EndMinigameMessage>(EndMinigame);
            gameObject.Subscribe<TearDownMinigameMessage>(TeardownMinigame);
        }

        private void StartMinigame(StartMinigameMessage msg)
        {
            if (_minigameController)
            {
                _minigameController.Destroy();
                Destroy(_minigameController.gameObject);
            }

            _unit = msg.Owner;
            var setMonsterStateMsg = MessageFactory.GenerateSetMonsterStateMsg();
            setMonsterStateMsg.State = MonsterState.Minigame;
            gameObject.SendMessageTo(setMonsterStateMsg, _unit);
            MessageFactory.CacheMessage(setMonsterStateMsg);

            _currentSettings = msg.Settings;

            var windows = new List<UiBaseWindow>();
            for (var i = 0; i < _currentSettings.MinigameWindows.Length; i++)
            {
                var window = UiWindowManager.OpenWindow(_currentSettings.MinigameWindows[i]);
                windows.Add(window);
            }
            _openWindows = windows.ToArray();

            
            switch (msg.Settings.Type)
            {
                case MinigameType.Maze:
                    _minigameController = Instantiate(_mazeTemplate, transform);
                    _minigameController.Setup(msg.Settings, _unit);
                    WorldController.SetWorldState(WorldState.Minigame);
                    break;
            }
        }

        private void EndMinigame(EndMinigameMessage msg)
        {
            for (var i = 0; i < _openWindows.Length; i++)
            {
                UiWindowManager.CloseWindow(_openWindows[i]);
            }
            _openWindows = null;

            var results = UiWindowManager.OpenWindow(_resultsTemplate);
            results.Setup(msg.Result, msg.Unit, _unit);
        }

        private void TeardownMinigame(TearDownMinigameMessage msg)
        {
            if (_minigameController)
            {
                _minigameController.Destroy();
                Destroy(_minigameController.gameObject);
                _minigameController = null;
            }

            if (WorldController.State == WorldState.Minigame)
            {
                if (_unit)
                {
                    var setMonsterStateMsg = MessageFactory.GenerateSetMonsterStateMsg();
                    setMonsterStateMsg.State = MonsterState.Idle;
                    gameObject.SendMessageTo(setMonsterStateMsg, _unit);
                    MessageFactory.CacheMessage(setMonsterStateMsg);

                    var setHobblerAiStateMsg = MessageFactory.GenerateSetHobblerAiStateMsg();
                    setHobblerAiStateMsg.State = HobblerAiState.Auto;
                    gameObject.SendMessageTo(setHobblerAiStateMsg, _unit);
                    MessageFactory.CacheMessage(setHobblerAiStateMsg);

                    _unit = null;
                }
                WorldController.SetWorldState(WorldState.World);
            }
        }

        void OnDestroy()
        {
            gameObject.UnsubscribeFromAllMessages();
        }
    }
}