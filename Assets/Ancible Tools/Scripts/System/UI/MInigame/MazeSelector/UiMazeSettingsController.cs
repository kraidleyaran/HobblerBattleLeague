using Assets.Resources.Ancible_Tools.Scripts.System.Minigame;
using MessageBusLib;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.Resources.Ancible_Tools.Scripts.System.UI.MInigame.MazeSelector
{
    public class UiMazeSettingsController : MonoBehaviour
    {
        public RectTransform RectTransform;
        [SerializeField] private Image _borderImage = null;
        [SerializeField] private Image _iconImage = null;
        [SerializeField] private Text _nameText = null;

        public MazeMinigameSettings Settings { get; private set; }

        private GameObject _parent = null;

        public void Setup(MazeMinigameSettings settings, GameObject parent)
        {
            Settings = settings;
            _parent = parent;
            _iconImage.sprite = settings.Icon;
            _nameText.text = settings.DisplayName;
        }

        public void Click()
        {
            var setSelectedMazeSettingsControllerMsg = MessageFactory.GenerateSetSelectedMazeSettingsControllerMsg();
            setSelectedMazeSettingsControllerMsg.Controller = this;
            gameObject.SendMessageTo(setSelectedMazeSettingsControllerMsg, _parent);
            MessageFactory.CacheMessage(setSelectedMazeSettingsControllerMsg);
        }

        public void SetSelected(bool selected)
        {
            _borderImage.color = selected ? ColorFactoryController.BonusStat : Color.white;
        }
    }
}