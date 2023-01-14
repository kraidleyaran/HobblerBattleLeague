using System;
using MessageBusLib;
using UnityEngine;

namespace Assets.Resources.Ancible_Tools.Scripts.System.BattleLeague
{
    public class BattleBenchSlotController : MonoBehaviour
    {
        public GameObject CurrentPiece { get; private set; }
        public BattleAlignment Alignment { get; private set; }

        [SerializeField] private Vector2 _offset = Vector2.zero;

        private string _filter = string.Empty;

        public void Setup(BattleAlignment alignment)
        {
            Alignment = alignment;
            _filter = $"{GetInstanceID()}";
            SubscribeToMessages();
        }

        public void SetCurrentPiece(GameObject piece)
        {
            CurrentPiece = piece;
            piece.transform.SetParent(transform);
            piece.transform.SetLocalPosition(_offset);

            var setGamePieceBenchMsg = MessageFactory.GenerateSetGamePieceBenchMsg();
            setGamePieceBenchMsg.Bench = this;
            gameObject.SendMessageTo(setGamePieceBenchMsg, CurrentPiece);
            MessageFactory.CacheMessage(setGamePieceBenchMsg);
        }

        public void Clear()
        {
            CurrentPiece = null;
        }

        private void SubscribeToMessages()
        {
            gameObject.SubscribeWithFilter<QueryBenchSlotMessage>(QueryBenchSlot, _filter);
        }

        private void QueryBenchSlot(QueryBenchSlotMessage msg)
        {
            if (msg.Alignment == Alignment)
            {
                msg.DoAfter.Invoke(this);
            }
        }

        void OnDestroy()
        {
            CurrentPiece = null;
            gameObject.UnsubscribeFromAllMessages();
        }
    }
}