using System.Collections.Generic;
using Assets.Resources.Ancible_Tools.Scripts.System.Minigame;
using Assets.Resources.Ancible_Tools.Scripts.System.UI.BattleLeague.Status;
using MessageBusLib;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.Resources.Ancible_Tools.Scripts.System.UI.MInigame
{
    public class UiMinigameUnitStatusBarController : MonoBehaviour
    {
        private const string FILTER = "UI_MINIGAME_UNIT_STATUS_BAR";

        [SerializeField] private UiMinigameCastingBarController _castingBarController = null;
        [SerializeField] private Image[] _allStatusEffects = new Image[0];
        [SerializeField] private Image _silence;
        [SerializeField] private Image _stun;
        [SerializeField] private Image _mute;
        [SerializeField] private Image _root;
        [SerializeField] private Image _disarm;
        [SerializeField] private UiFillBarController _healthBarController = null;
        [SerializeField] private Vector2 _floatingTextOffset = Vector2.zero;

        private Color _healthBarColor = Color.white;
        private GameObject _unit = null;
        private Transform _followTransform = null;
        private Vector2 _offset = Vector2.zero;
        private bool _visible = false;
        private List<UiFloatingTextController> _floatingText = new List<UiFloatingTextController>();
        private List<GameObject> _placeHolders = new List<GameObject>();

        public void Setup(GameObject unit, Transform followTransform, Color healthBarColor, Vector2 offset)
        {
            _unit = unit;
            _followTransform = followTransform;
            _healthBarColor = healthBarColor;
            _healthBarController.gameObject.SetActive(false);
            _offset = offset;
            foreach (var effect in _allStatusEffects)
            {
                effect.gameObject.SetActive(false);
            }
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

        private void FloatingTextFinished(UiFloatingTextController controller, GameObject parent)
        {
            _placeHolders.Remove(parent);
            Destroy(parent);
            _floatingText.Remove(controller);
            Destroy(controller.gameObject);
            if ((!_unit || !_visible) && _floatingText.Count <= 0)
            {
                gameObject.SetActive(false);
            }
        }

        private void SubscribeUnitMessages()
        {
            _unit.SubscribeWithFilter<RefreshUnitMessage>(RefreshUnit, FILTER);
            _unit.SubscribeWithFilter<UpdateUnitCastTimerMessage>(UpdateUnitCastTimer, FILTER);
            _unit.SubscribeWithFilter<UpdateFogVisibilityMessage>(UpdateFogVisibility, FILTER);
            _unit.SubscribeWithFilter<UpdateMinigameUnitStateMessage>(UpdateMinigameUnitState, FILTER);
            _unit.SubscribeWithFilter<UnitDiedMessage>(UnitDied, FILTER);
            _unit.SubscribeWithFilter<StatusEffectFinishedMessage>(StatusEffectFinished, FILTER);
            _unit.SubscribeWithFilter<StunMessage>(Stun, FILTER);
            _unit.SubscribeWithFilter<SilenceMessage>(Silence, FILTER);
            _unit.SubscribeWithFilter<MuteMessage>(Mute, FILTER);
            _unit.SubscribeWithFilter<RootMessage>(Root, FILTER);
            _unit.SubscribeWithFilter<DisarmMessage>(Disarm, FILTER);
            //_unit.SubscribeWithFilter<ShowFloatingTextMessage>(ShowFloatingText, FILTER);
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
            if (_floatingText.Count <= 0)
            {
                gameObject.SetActive(msg.State != MinigameUnitState.Disabled && _visible);
            }
        }

        private void UnitDied(UnitDiedMessage msg)
        {
            _unit.gameObject.UnsubscribeFromAllMessagesWithFilter(FILTER);
            gameObject.UnsubscribeFromAllMessages();
            _unit = null;
        }



        private void Stun(StunMessage msg)
        {
            _stun.gameObject.SetActive(true);
        }

        private void Silence(SilenceMessage msg)
        {
            _silence.gameObject.SetActive(true);
        }

        private void Root(RootMessage msg)
        {
            _root.gameObject.SetActive(true);
        }

        private void Mute(MuteMessage msg)
        {
            _mute.gameObject.SetActive(true);
        }

        private void Disarm(DisarmMessage msg)
        {
            _disarm.gameObject.SetActive(true);
        }

        private void StatusEffectFinished(StatusEffectFinishedMessage msg)
        {
            switch (msg.Type)
            {
                case Combat.StatusEffectType.Stun:
                    _stun.gameObject.SetActive(false);
                    break;
                case Combat.StatusEffectType.Silence:
                    _silence.gameObject.SetActive(false);
                    break;
                case Combat.StatusEffectType.Root:
                    _root.gameObject.SetActive(false);
                    break;
                case Combat.StatusEffectType.Mute:
                    _mute.gameObject.SetActive(false);
                    break;
                case Combat.StatusEffectType.Disarm:
                    _disarm.gameObject.SetActive(false);
                    break;
            }
        }

        public void Destroy()
        {
            if (_unit)
            {
                _unit.gameObject.UnsubscribeFromAllMessagesWithFilter(FILTER);
                gameObject.UnsubscribeFromAllMessages();
            }

            foreach (var floatingText in _floatingText)
            {
                Destroy(floatingText.gameObject);
            }
            _floatingText.Clear();
            foreach (var placeHolder in _placeHolders)
            {
                Destroy(placeHolder);
            }
            _placeHolders.Clear();
            
        }

        void OnDestroy()
        {
            Destroy();
        }
    }
}