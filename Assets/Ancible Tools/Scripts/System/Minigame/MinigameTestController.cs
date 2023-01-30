using System.Collections.Generic;
using Assets.Resources.Ancible_Tools.Scripts.System.Minigame.Settings;
using Assets.Resources.Ancible_Tools.Scripts.System.UI.MInigame;
using Assets.Resources.Ancible_Tools.Scripts.System.Windows;
using MessageBusLib;
using UnityEngine;

namespace Assets.Resources.Ancible_Tools.Scripts.System.Minigame
{
    public class MinigameTestController : MonoBehaviour
    {
        [SerializeField] private MinigameSettings _settings;
        [SerializeField] private MazeMinigameController _mazeTemplate;
        [SerializeField] private UiMinigameResultsController _resultsTemplate;

        private MinigameController _minigameController = null;

        private UiBaseWindow[] _openWindows = new UiBaseWindow[0];

        void Awake()
        {
            SubscribeToMessages();
        }

        void Start()
        {
            LoadMinigame(_settings);
        }

        public void ResetMinigame()
        {
            if (_minigameController)
            {
                _minigameController.Destroy();
                Destroy(_minigameController.gameObject);
            }
            for (var i = 0; i < _openWindows.Length; i++)
            {
                UiWindowManager.CloseWindow(_openWindows[i], _openWindows[i].WorldName);
            }
            _openWindows = new UiBaseWindow[0];
            LoadMinigame(_settings);
        }

        private void LoadMinigame(MinigameSettings settings)
        {
            var windows = new List<UiBaseWindow>();
            for (var i = 0; i < settings.MinigameWindows.Length; i++)
            {
                windows.Add(UiWindowManager.OpenWindow(settings.MinigameWindows[i]));
            }

            _openWindows = windows.ToArray();

            switch (settings.Type)
            {
                case MinigameType.Maze:
                    var controller = Instantiate(_mazeTemplate, transform);
                    controller.gameObject.layer = gameObject.layer;
                    controller.Setup(settings, null);
                    _minigameController = controller;
                    MinigameCameraController.SetActive(true);
                    break;
            }


        }

        private void SubscribeToMessages()
        {
            gameObject.Subscribe<EndMinigameMessage>(EndMinigame);
            gameObject.Subscribe<TearDownMinigameMessage>(TearDownMinigame);
        }

        private void EndMinigame(EndMinigameMessage msg)
        {
            //_minigameController.Destroy();
            //Destroy(_minigameController.gameObject);
            //_minigameController = null;
            //Debug.Log($"Minigame Result: {msg.Result}");
            for (var i = 0; i < _openWindows.Length; i++)
            {
                UiWindowManager.CloseWindow(_openWindows[i], _openWindows[i].WorldName);
            }
            _openWindows = new UiBaseWindow[0];

            var minigameResultWindow = UiWindowManager.OpenWindow(_resultsTemplate);
            minigameResultWindow.Setup(msg.Result, msg.Unit, null);
        }

        private void TearDownMinigame(TearDownMinigameMessage msg)
        {
            _minigameController.Destroy();
            Destroy(_minigameController.gameObject);
            _minigameController = null;
        }

        void OnDestroy()
        {
            gameObject.UnsubscribeFromAllMessages();
        }
    }
}