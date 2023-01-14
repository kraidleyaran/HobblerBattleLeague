using Assets.Resources.Ancible_Tools.Scripts.System.BattleLeague;
using MessageBusLib;
using UnityEngine;

namespace Assets.Resources.Ancible_Tools.Scripts.System.UI.BattleLeague.Status
{
    public class UiBattleUnitStatusController : MonoBehaviour
    {
        private const string FILTER = "Ui Battle Unit Status";

        [SerializeField] private UiFillBarController _healthBarController = null;

        public GameObject Owner { get; private set; }

        private Transform _follow = null;
        private Color _healthBarColor = Color.red;
        private Vector2 _offset = Vector2.zero;

        public void Setup(GameObject owner, Transform follow, Color healthBar, Vector2 offset)
        {
            _healthBarColor = healthBar;
            _follow = follow;
            _offset = offset;
            Owner = owner;
            _healthBarController.Setup(1f,string.Empty, _healthBarColor);
            var pos = BattleLeagueCameraController.Camera.WorldToScreenPoint(_follow.position.ToVector2() + _offset).ToVector2();
            transform.SetTransformPosition(pos);
            SubscribeToMessages();
        }

        private void RefreshHealth(int current, int max)
        {
            var percent = (float) current / max;
            _healthBarController.Setup(percent, string.Empty, _healthBarColor);
        }

        private void SubscribeToMessages()
        {
            gameObject.Subscribe<UpdateTickMessage>(UpdateTick);
            gameObject.Subscribe<UpdateBattleStateMessage>(UpdateBattleState);
            Owner.SubscribeWithFilter<RefreshUnitMessage>(RefreshUnit, FILTER);
            Owner.SubscribeWithFilter<UpdateUnitBattleStateMessage>(UpdateUnitBattleState, FILTER);
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
            var queryHealthMsg = MessageFactory.GenerateQueryHealthMsg();
            queryHealthMsg.DoAfter = RefreshHealth;
            gameObject.SendMessageTo(queryHealthMsg, Owner);
            MessageFactory.CacheMessage(queryHealthMsg);
        }

        private void UpdateUnitBattleState(UpdateUnitBattleStateMessage msg)
        {
            if (msg.State == UnitBattleState.Dead)
            {
                gameObject.SetActive(false);
                _follow = null;
                gameObject.Unsubscribe<UpdateTickMessage>();
                gameObject.Unsubscribe<UpdateBattleStateMessage>();
            }
        }

        private void UpdateBattleState(UpdateBattleStateMessage msg)
        {
            if (msg.State == BattleState.Results)
            {
                gameObject.SetActive(false);
            }
        }

        public void Destroy()
        {
            Owner.UnsubscribeFromAllMessagesWithFilter(FILTER);
            Owner = null;
            _follow = null;
            gameObject.UnsubscribeFromAllMessages();

        }
    }
}