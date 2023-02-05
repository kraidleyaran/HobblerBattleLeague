using System.Collections.Generic;
using System.Linq;
using Assets.Ancible_Tools.Scripts.Traits;
using Assets.Resources.Ancible_Tools.Scripts.System.Minigame;
using Assets.Resources.Ancible_Tools.Scripts.System.Windows;
using MessageBusLib;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.Resources.Ancible_Tools.Scripts.System.UI.MInigame.MazeSelector
{
    public class UiMazeSelectionWindowController : UiBaseWindow
    {
        public const string FILTER = "UI_MAZE_SELECTION_WINDOW";

        public override bool Movable => true;
        public override bool Static => false;

        [SerializeField] private UiMazeSettingsController _settingsTemplate = null;
        [SerializeField] private VerticalLayoutGroup _grid = null;
        [SerializeField] private RectTransform _content = null;
        [SerializeField] private Image _hobblerIconImage;
        [SerializeField] private Text _hobblerNameText;
        [SerializeField] private Text _hobblerLevelText;
        [SerializeField] private Button _playButton = null;

        private UiMazeSettingsController[] _controllers = new UiMazeSettingsController[0];

        private GameObject _hobbler = null;
        private string _filter = string.Empty;
        private UiMazeSettingsController _selectedController = null;

        public void Setup(GameObject hobbler)
        {
            _hobbler = hobbler;
            var availableSettings = MinigameManager.AvailableMazeSettings.ToArray();
            var controllers = new List<UiMazeSettingsController>();
            foreach (var setting in availableSettings)
            {
                var controller = Instantiate(_settingsTemplate, _grid.transform);
                controller.Setup(setting, gameObject);
                controllers.Add(controller);
            }

            _controllers = controllers.ToArray();
            var height = _controllers.Length * (_settingsTemplate.RectTransform.rect.height + _grid.spacing) + _grid.padding.top;
            _content.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, height);

            var querySpriteMsg = MessageFactory.GenerateQuerySpriteMsg();
            querySpriteMsg.DoAfter = UpdateSprite;
            gameObject.SendMessageTo(querySpriteMsg, _hobbler);
            MessageFactory.CacheMessage(querySpriteMsg);

            var queryNameMsg = MessageFactory.GenerateQueryUnitNameMsg();
            queryNameMsg.DoAfter = UpdateName;
            gameObject.SendMessageTo(queryNameMsg, _hobbler);
            MessageFactory.CacheMessage(queryNameMsg);

            var queryExperienceMsg = MessageFactory.GenerateQueryExperienceMsg();
            queryExperienceMsg.DoAfter = UpdateLevel;
            gameObject.SendMessageTo(queryExperienceMsg, _hobbler);
            MessageFactory.CacheMessage(queryExperienceMsg);

            _playButton.interactable = _selectedController;
            SubscribeToMessages();
        }

        public void Play()
        {
            if (_selectedController)
            {
                var startMinigameMsg = MessageFactory.GenerateStartMinigameMsg();
                startMinigameMsg.Owner = _hobbler;
                startMinigameMsg.Settings = _selectedController.Settings;
                gameObject.SendMessage(startMinigameMsg);
                MessageFactory.CacheMessage(startMinigameMsg);
            }
        }

        private void UpdateSprite(SpriteTrait trait)
        {
            _hobblerIconImage.sprite = trait.Sprite;
        }

        private void UpdateName(string hobblerName)
        {
            _hobblerNameText.text = hobblerName;
        }

        private void UpdateLevel(int experience, int level, int nextLevelExerpience)
        {
            _hobblerLevelText.text = $"Level {level + 1}";
        }

        private void SubscribeToMessages()
        {
            _filter = $"{FILTER}-{_hobbler.GetInstanceID()}";
            gameObject.SubscribeWithFilter<SetSelectedMazeSettingsControllerMessage>(SetSelectedMazeSettingsController, _filter);
        }

        private void SetSelectedMazeSettingsController(SetSelectedMazeSettingsControllerMessage msg)
        {
            if (!_selectedController || _selectedController != msg.Controller)
            {
                if (_selectedController)
                {
                    _selectedController.SetSelected(false);
                }

                _selectedController = msg.Controller;
                _selectedController.SetSelected(true);
                _playButton.interactable = _selectedController;
            }
        }

        public override void Destroy()
        {
            gameObject.UnsubscribeFromAllMessagesWithFilter(_filter);
            base.Destroy();
        }
    }
}