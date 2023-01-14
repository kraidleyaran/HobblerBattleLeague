using Assets.Resources.Ancible_Tools.Scripts.System.Minigame;
using MessageBusLib;
using UnityEngine;

namespace Assets.Resources.Ancible_Tools.Scripts.System.UI.MInigame
{
    public class UiMinigameUnitStatusBarController : MonoBehaviour
    {
        private const string FILTER = "UI_MINIGAME_UNIT_STATUS_BAR";

        [SerializeField] private UiMinigameCastingBarController _castingBarController = null;
        [SerializeField] private UiFillBarController _healthBarController = null;

        private Color _healthBarColor = Color.white;
        private GameObject _unit = null;
        private Transform _followTransform = null;
        private Vector2 _offset = Vector2.zero;
        private bool _visible = false;

        public void Setup(GameObject unit, Transform followTransform, Color healthBarColor, Vector2 offset)
        {
            _unit = unit;
            _followTransform = followTransform;
            _healthBarColor = healthBarColor;
            _healthBarController.gameObject.SetActive(false);
            _offset = offset;
            SubscribeToMessages();
            SubscribeUnitMessages();
        }

        private void RefreshInfo()
        {
            var queryHealthMsg = MessageFactory.GenerateQueryHealthMsg();
            queryHealthMsg.DoAfter = UpdateHealth;
            gameObject.SendMessageTo(queryHealthMsg, _unit);
            MessageFactory.CacheMessage(queryHealthMsg);
        }

        private void UpdateHealth(int current, int max)
        {
            if (current < max)
            {
                var percent = (float) current / max;
                _healthBarController.Setup(percent, $"{percent:P}", _healthBarColor);
            }
            else
            {
                _healthBarController.gameObject.SetActive(false);
            }
        }

        private void SubscribeUnitMessages()
        {
            _unit.SubscribeWithFilter<RefreshUnitMessage>(RefreshUnit, FILTER);
            _unit.SubscribeWithFilter<UpdateUnitCastTimerMessage>(UpdateUnitCastTimer, FILTER);
            _unit.SubscribeWithFilter<UpdateFogVisibilityMessage>(UpdateFogVisibility, FILTER);
            _unit.SubscribeWithFilter<UpdateMinigameUnitStateMessage>(UpdateMinigameUnitState, FILTER);
            _unit.SubscribeWithFilter<UnitDiedMessage>(UnitDied, FILTER);
        }

        private void SubscribeToMessages()
        {
            gameObject.Subscribe<UpdateTickMessage>(UpdateTick);

        }

        private void UpdateTick(UpdateTickMessage msg)
        {
            if (_followTransform)
            {
                var pos = MinigameCameraController.Camera.WorldToScreenPoint(_followTransform.position.ToVector2() +_offset).ToVector2();
                var currentPos = transform.position.ToVector2();
                if (pos != currentPos)
                {
                    transform.SetTransformPosition(pos);
                }
            }
        }

        private void RefreshUnit(RefreshUnitMessage msg)
        {
            RefreshInfo();
        }

        private void UpdateUnitCastTimer(UpdateUnitCastTimerMessage msg)
        {
            _castingBarController.Setup(msg.CastTimer, msg.Name, msg.Icon);
        }

        private void UpdateFogVisibility(UpdateFogVisibilityMessage msg)
        {
            _visible = msg.Visible;
            gameObject.SetActive(_visible);
        }

        private void UpdateMinigameUnitState(UpdateMinigameUnitStateMessage msg)
        {
            gameObject.SetActive(msg.State != MinigameUnitState.Disabled && _visible);
        }

        private void UnitDied(UnitDiedMessage msg)
        {
            gameObject.SetActive(false);
            _unit.gameObject.UnsubscribeFromAllMessagesWithFilter(FILTER);
            gameObject.UnsubscribeFromAllMessages();
            _unit = null;
        }

        public void Destroy()
        {
            if (_unit)
            {
                _unit.gameObject.UnsubscribeFromAllMessagesWithFilter(FILTER);
                gameObject.UnsubscribeFromAllMessages();
            }
            
        }

        void OnDestroy()
        {
            Destroy();
        }
    }
}