using System.Collections.Generic;
using Assets.Resources.Ancible_Tools.Scripts.System.BattleLeague;
using Assets.Resources.Ancible_Tools.Scripts.System.UI.MInigame;
using MessageBusLib;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.Resources.Ancible_Tools.Scripts.System.UI.BattleLeague.Status
{
    public class UiBattleUnitStatusController : MonoBehaviour
    {
        private const string FILTER = "Ui Battle Unit Status";

        [SerializeField] private UiFillBarController _healthBarController = null;
        [SerializeField] private UiFillBarController _manaBarController = null;
        [SerializeField] private Image[] _allStatusEffects = new Image[0];
        [SerializeField] private Image _silence;
        [SerializeField] private Image _stun;
        [SerializeField] private Image _mute;
        [SerializeField] private Image _root;
        [SerializeField] private Image _disarm;
        [SerializeField] private UiMinigameCastingBarController _castinBarController = null;
        [SerializeField] private Vector2 _floatingTextOffset = Vector2.zero;

        public GameObject Owner { get; private set; }

        private Transform _follow = null;
        private Color _healthBarColor = Color.red;
        private Vector2 _offset = Vector2.zero;
        private List<UiFloatingTextController> _floatingText = new List<UiFloatingTextController>();

        public void Setup(GameObject owner, Transform follow, Color healthBar, Vector2 offset)
        {
            _healthBarColor = healthBar;
            _follow = follow;
            _offset = offset;
            Owner = owner;
            _healthBarController.Setup(1f,string.Empty, _healthBarColor);
            var pos = BattleLeagueCameraController.Camera.WorldToScreenPoint(_follow.position.ToVector2() + _offset).ToVector2();
            transform.SetTransformPosition(pos);
            _castinBarController.Interrupt();
            foreach (var effect in _allStatusEffects)
            {
                effect.gameObject.SetActive(false);
            }
            RefrehUnitStats();
            SubscribeToMessages();
        }

        private void RefreshHealth(int current, int max)
        {
            var percent = (float) current / max;
            _healthBarController.Setup(percent, string.Empty, _healthBarColor);
            _healthBarController.gameObject.SetActive(percent < 1f);
        }

        private void RefreshMana(int current, int max)
        {
            var percent = (float)current / max;
            _manaBarController.Setup(percent, string.Empty, ColorFactoryController.ManaBar);
            _manaBarController.gameObject.SetActive(max > 0);
        }

        private void RefrehUnitStats()
        {
            var queryHealthMsg = MessageFactory.GenerateQueryHealthMsg();
            queryHealthMsg.DoAfter = RefreshHealth;
            gameObject.SendMessageTo(queryHealthMsg, Owner);
            MessageFactory.CacheMessage(queryHealthMsg);

            var queryManaMsg = MessageFactory.GenerateQueryManaMsg();
            queryManaMsg.DoAfter = RefreshMana;
            gameObject.SendMessageTo(queryManaMsg, Owner);
            MessageFactory.CacheMessage(queryManaMsg);
        }

        private void SubscribeToMessages()
        {
            gameObject.Subscribe<UpdateTickMessage>(UpdateTick);
            gameObject.Subscribe<UpdateBattleStateMessage>(UpdateBattleState);
            Owner.SubscribeWithFilter<RefreshUnitMessage>(RefreshUnit, FILTER);
            Owner.SubscribeWithFilter<UpdateUnitBattleStateMessage>(UpdateUnitBattleState, FILTER);
            Owner.SubscribeWithFilter<StatusEffectFinishedMessage>(StatusEffectFinished, FILTER);
            Owner.SubscribeWithFilter<StunMessage>(Stun, FILTER);
            Owner.SubscribeWithFilter<SilenceMessage>(Silence, FILTER);
            Owner.SubscribeWithFilter<MuteMessage>(Mute, FILTER);
            Owner.SubscribeWithFilter<RootMessage>(Root, FILTER);
            Owner.SubscribeWithFilter<DisarmMessage>(Disarm, FILTER);
            Owner.SubscribeWithFilter<UpdateUnitCastTimerMessage>(UpdateUnitCastingTimer, FILTER);
            Owner.SubscribeWithFilter<CastInterruptedMessage>(CastInterrupted, FILTER);
            //Owner.SubscribeWithFilter<ShowFloatingTextMessage>(ShowFloatingText, FILTER);
        }

        private void UpdateTick(UpdateTickMessage msg)
        {
            if (_follow)
            {
                var pos = BattleLeagueCameraController.Camera.WorldToScreenPoint(_follow.position.ToVector2() + _offset).ToVector2();
                var currentPos = transform.position.ToVector2();
                if (pos != currentPos)
                {
                    transform.SetTransformPosition(pos);
                }
            }
            
        }

        private void RefreshUnit(RefreshUnitMessage msg)
        {
            RefrehUnitStats();
        }

        private void UpdateUnitBattleState(UpdateUnitBattleStateMessage msg)
        {
            if (msg.State == UnitBattleState.Dead)
            {
                gameObject.SetActive(false);
                _follow = null;
                gameObject.Unsubscribe<UpdateTickMessage>();
                gameObject.Unsubscribe<UpdateBattleStateMessage>();
                Owner.UnsubscribeFromAllMessagesWithFilter(FILTER);
                _castinBarController.Interrupt();
            }
        }

        private void UpdateBattleState(UpdateBattleStateMessage msg)
        {
            if (msg.State == BattleState.Results)
            {
                gameObject.SetActive(false);
            }
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

        private void UpdateUnitCastingTimer(UpdateUnitCastTimerMessage msg)
        {
            _castinBarController.Setup(msg.CastTimer, string.Empty, msg.Icon);
        }

        private void CastInterrupted(CastInterruptedMessage msg)
        {
            _castinBarController.Interrupt();
        }

        public void Destroy()
        {
            _castinBarController.Interrupt();
            for (var i = 0; i < _floatingText.Count; i++)
            {
                Destroy(_floatingText[i].gameObject);
            }
            _floatingText.Clear();
            Owner.UnsubscribeFromAllMessagesWithFilter(FILTER);
            Owner = null;
            _follow = null;
            gameObject.UnsubscribeFromAllMessages();

        }
    }
}