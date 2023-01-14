using Assets.Resources.Ancible_Tools.Scripts.Hitbox;
using Assets.Resources.Ancible_Tools.Scripts.System;
using Assets.Resources.Ancible_Tools.Scripts.System.BattleLeague;
using Assets.Resources.Ancible_Tools.Scripts.System.UI.BattleLeague.Status;
using MessageBusLib;
using UnityEngine;

namespace Assets.Ancible_Tools.Scripts.Traits
{
    [CreateAssetMenu(fileName = "Battle Unit Trait", menuName = "Ancible Tools/Traits/Battle/Battle Unit")]
    public class BattleUnitTrait : Trait
    {
        [SerializeField] private Resources.Ancible_Tools.Scripts.Hitbox.Hitbox _selectableHitbox = null;
        [SerializeField] private Vector2 _offset = Vector2.zero;
        [SerializeField] private Vector2 _selectOffset = Vector2.zero;
        [SerializeField] private Vector2 _selectorSize = Vector2.one;

        private HitboxController _hitboxController = null;

        private BattleUnitData _data = null;
        private BattleAlignment _alignment = BattleAlignment.None;

        private UnitSelectorController _hovered = null;
        private UnitSelectorController _selected = null;

        public override void SetupController(TraitController controller)
        {
            base.SetupController(controller);
            _hitboxController = _controller.gameObject.SetupHitbox(_selectableHitbox, CollisionLayerFactory.BattleSelect);
            var registerCollisionMsg = MessageFactory.GenerateRegisterCollisionMsg();
            registerCollisionMsg.Object = _controller.gameObject;
            _controller.gameObject.SendMessageTo(registerCollisionMsg, _hitboxController.gameObject);
            MessageFactory.CacheMessage(registerCollisionMsg);
            SubscribeToMessages();
        }

        private void SubscribeToMessages()
        {
            _controller.gameObject.Subscribe<StartBattleMessage>(StartBattle);

            _controller.transform.parent.gameObject.SubscribeWithFilter<SetGamePieceDataMessage>(SetGamePieceData, _instanceId);
            _controller.transform.parent.gameObject.SubscribeWithFilter<QueryBattleUnitDataMessage>(QueryBattleUnitData, _instanceId);
            _controller.transform.parent.gameObject.SubscribeWithFilter<QueryBattlePieceMessage>(QueryBattleGamePiece, _instanceId);
            _controller.transform.parent.gameObject.SubscribeWithFilter<UpdateBattleAlignmentMessage>(UpdateBattleAlignment, _instanceId);
            _controller.transform.parent.gameObject.SubscribeWithFilter<UpdateUnitBattleStateMessage>(UpdateUnitBattleState, _instanceId);
            _controller.transform.parent.gameObject.SubscribeWithFilter<ReportDamageMessage>(ReportDamage, _instanceId);
            _controller.transform.parent.gameObject.SubscribeWithFilter<ReportHealMessage>(ReportHeal, _instanceId);
            _controller.transform.parent.gameObject.SubscribeWithFilter<SetSelectStateMesage>(SetSelectState, _instanceId);
            _controller.transform.parent.gameObject.SubscribeWithFilter<SetHoveredStateMessage>(SetHoverState, _instanceId);
        }

        private void SetGamePieceData(SetGamePieceDataMessage msg)
        {
            _data = msg.Data;
        }

        private void QueryBattleUnitData(QueryBattleUnitDataMessage msg)
        {
            msg.DoAfter.Invoke(_data);
        }

        private void QueryBattleGamePiece(QueryBattlePieceMessage msg)
        {
            msg.DoAfter.Invoke(_data, _alignment);
        }

        private void UpdateBattleAlignment(UpdateBattleAlignmentMessage msg)
        {
            _alignment = msg.Alignment;
            UiBattleUnitStatusManager.RegisterUnitStatusBar(_controller.transform.parent.gameObject, _controller.transform.parent, _alignment, _offset);
        }

        private void UpdateUnitBattleState(UpdateUnitBattleStateMessage msg)
        {
            if (msg.State == UnitBattleState.Dead)
            {
                _hitboxController.gameObject.SetActive(false);
                BattleLeagueController.RemoveUnit(_controller.transform.parent.gameObject, _alignment, _data.Stats.Spirit);
                _data.Deaths++;
                if (_selected)
                {
                    BattleSelectController.ReturnSelect(_controller.transform.parent.gameObject);
                }

                if (_hovered)
                {
                    BattleSelectController.ReturnHover(_controller.transform.parent.gameObject);
                }
            }
        }

        private void ReportDamage(ReportDamageMessage msg)
        {
            if (msg.Owner && msg.Owner == _controller.transform.parent.gameObject)
            {
                _data.TotalDamageDone += msg.Amount;
            }
            else
            {
                _data.TotalDamageTaken += msg.Amount;
            }
            
        }

        private void ReportHeal(ReportHealMessage msg)
        {
            if (msg.Owner && msg.Owner == _controller.transform.parent.gameObject)
            {
                _data.TotalHeals += msg.Amount;
            }
        }

        private void StartBattle(StartBattleMessage msg)
        {
            _controller.gameObject.Unsubscribe<StartBattleMessage>();
            _data.RoundsPlayed++;
        }

        private void SetSelectState(SetSelectStateMesage msg)
        {
            _selected = msg.Selector;
            if (_selected)
            {
                _selected.gameObject.SetActive(true);
                _selected.SetParent(_controller.transform.parent, _selectOffset, _selectorSize);
            }
        }

        private void SetHoverState(SetHoveredStateMessage msg)
        {
            _hovered = msg.Selector;
            if (_hovered)
            {
                _hovered.gameObject.SetActive(true);
                _hovered.SetParent(_controller.transform.parent, _selectOffset, _selectorSize);
                
            }
        }

        public override void Destroy()
        {
            if (_hitboxController)
            {
                var unregisterCollisionMsg = MessageFactory.GenerateUnregisterCollisionMsg();
                unregisterCollisionMsg.Object = _controller.gameObject;
                _controller.gameObject.SendMessageTo(unregisterCollisionMsg, _controller.transform.parent.gameObject);
                MessageFactory.CacheMessage(unregisterCollisionMsg);
            }

            if (_selected)
            {
                BattleSelectController.ReturnSelect(_controller.transform.parent.gameObject);
            }

            if (_hovered)
            {
                BattleSelectController.ReturnHover(_controller.transform.parent.gameObject);
            }
            _data = null;
            base.Destroy();
        }
    }
}