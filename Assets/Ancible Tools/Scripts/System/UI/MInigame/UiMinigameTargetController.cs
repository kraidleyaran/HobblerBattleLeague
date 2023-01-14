using Assets.Ancible_Tools.Scripts.Traits;
using Assets.Resources.Ancible_Tools.Scripts.System.Windows;
using MessageBusLib;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.Resources.Ancible_Tools.Scripts.System.UI.MInigame
{
    public class UiMinigameTargetController : UiBaseWindow
    {
        private const string FILTER = "UI_MINIGAME_TARGET";

        [SerializeField] private Image _iconImage = null;
        [SerializeField] private Image _frameImage = null;
        [SerializeField] private UiFillBarController _healthBarController;

        public override bool Static => true;
        public override bool Movable => true;

        private GameObject _target = null;

        public override void Awake()
        {
            base.Awake();
            SubscribeToMessages();
            gameObject.SetActive(false);
        }

        private void RefreshInfo()
        {
            var queryHealthMsg = MessageFactory.GenerateQueryHealthMsg();
            queryHealthMsg.DoAfter = UpdateHealth;
        }

        private void UpdateSprite(SpriteTrait sprite)
        {
            _iconImage.sprite = sprite.Sprite;
        }

        private void UpdateHealth(int current, int max)
        {
            var percent = (float) current / max;
            _healthBarController.Setup(percent, $"{percent:P}", ColorFactoryController.HealthBar);
        }

        private void SubscribeToMessages()
        {
            gameObject.Subscribe<UpdateSelectedMinigameUnitMessage>(UpdateSelectedMinigameUnit);
        }

        private void UpdateSelectedMinigameUnit(UpdateSelectedMinigameUnitMessage msg)
        {
            if (_target)
            {
                _target.UnsubscribeFromAllMessagesWithFilter(FILTER);
            }
            _target = msg.Unit;
            _target.SubscribeWithFilter<RefreshUnitMessage>(RefreshUnit, FILTER);
            if (_target)
            {
                var querySpriteMsg = MessageFactory.GenerateQuerySpriteMsg();
                querySpriteMsg.DoAfter = UpdateSprite;
                gameObject.SendMessageTo(querySpriteMsg, _target);
                MessageFactory.CacheMessage(querySpriteMsg);

                RefreshInfo();
            }
            gameObject.SetActive(_target);
        }

        private void RefreshUnit(RefreshUnitMessage msg)
        {
            RefreshInfo();
        }

        public override void Destroy()
        {
            if (_target)
            {
                _target.UnsubscribeFromAllMessagesWithFilter(FILTER);
            }
            base.Destroy();
        }
    }
}