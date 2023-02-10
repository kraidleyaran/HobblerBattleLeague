using Assets.Resources.Ancible_Tools.Scripts.System;
using Assets.Resources.Ancible_Tools.Scripts.System.BattleLeague;
using Assets.Resources.Ancible_Tools.Scripts.System.Combat;
using MessageBusLib;
using UnityEngine;

namespace Assets.Ancible_Tools.Scripts.Traits
{
    [CreateAssetMenu(fileName = "Battle Attack Trait", menuName = "Ancible Tools/Traits/Battle/Battle Attack")]
    public class BattleAttackTrait : Trait
    {
        [SerializeField] private BasicAttackSetup _defaultSetup = null;
        [SerializeField] private float _defaultAttackRange = 16 * .3125f;

        private BasicAttackSetup _currentAttackSetup = null;

        private UnitBattleState _battleState = UnitBattleState.Active;

        public override void SetupController(TraitController controller)
        {
            base.SetupController(controller);
            _currentAttackSetup = _defaultSetup;
            SubscribeToMessages();
        }

        private void ApplyAttack(GameObject target)
        {
            var addTraitToUnitMsg = MessageFactory.GenerateAddTraitToUnitMsg();

            if (_currentAttackSetup.ApplyToOwner.Length > 0)
            {
                foreach (var trait in _currentAttackSetup.ApplyToOwner)
                {
                    addTraitToUnitMsg.Trait = trait;
                    _controller.transform.parent.gameObject.SendMessageTo(addTraitToUnitMsg, _controller.transform.parent.gameObject);
                }
            }

            foreach (var trait in _currentAttackSetup.ApplyToTarget)
            {
                addTraitToUnitMsg.Trait = trait;
                _controller.transform.parent.gameObject.SendMessageTo(addTraitToUnitMsg, target);
            }


            MessageFactory.CacheMessage(addTraitToUnitMsg);
        }

        private void CleanupAttack()
        {
            if (_battleState != UnitBattleState.Dead)
            {
                _controller.gameObject.SendMessageTo(ActivateGlobalCooldownMessage.INSTANCE, _controller.transform.parent.gameObject);
            }
        }

        private void InterruptAttack()
        {
            if (_battleState == UnitBattleState.Attack)
            {
                _controller.gameObject.SendMessageTo(InterruptBumpMessage.INSTANCE, _controller.transform.parent.gameObject);
                var setUnitbattleStateMsg = MessageFactory.GenerateSetUnitBattleStateMsg();
                setUnitbattleStateMsg.State = UnitBattleState.Active;
                _controller.gameObject.SendMessageTo(setUnitbattleStateMsg, _controller.transform.parent.gameObject);
                MessageFactory.CacheMessage(setUnitbattleStateMsg);
            }
        }

        private void SubscribeToMessages()
        {
            _controller.transform.parent.gameObject.SubscribeWithFilter<DoBasicAttackMessage>(DoBasicAttack, _instanceId);
            _controller.transform.parent.gameObject.SubscribeWithFilter<BasicAttackCheckMessage>(BasicAttackCheck, _instanceId);
            _controller.transform.parent.gameObject.SubscribeWithFilter<SetBasicAttackSetupMessage>(SetBasicAttackSetup, _instanceId);
            _controller.transform.parent.gameObject.SubscribeWithFilter<ClearBasicAttackSetupMessage>(ClearBasicAttackSetup, _instanceId);
            _controller.transform.parent.gameObject.SubscribeWithFilter<QueryBasicAttackSetupMessage>(QueryBasicAttackSetup, _instanceId);
            _controller.transform.parent.gameObject.SubscribeWithFilter<UpdateUnitBattleStateMessage>(UpdateUnitBattleState, _instanceId);
            _controller.transform.parent.gameObject.SubscribeWithFilter<StunMessage>(Stun, _instanceId);
            _controller.transform.parent.gameObject.SubscribeWithFilter<DisarmMessage>(Disarm, _instanceId);
        }

        private void DoBasicAttack(DoBasicAttackMessage msg)
        {
            if (_battleState == UnitBattleState.Active)
            {
                var setUnitStateMsg = MessageFactory.GenerateSetUnitBattleStateMsg();
                setUnitStateMsg.State = UnitBattleState.Attack;
                _controller.gameObject.SendMessageTo(setUnitStateMsg, _controller.transform.parent.gameObject);
                MessageFactory.CacheMessage(setUnitStateMsg);
                var diff = msg.Target.transform.position.ToVector2() - _controller.transform.parent.position.ToVector2();

                var setFaceDirectionMsg = MessageFactory.GenerateSetFaceDirectionMsg();
                setFaceDirectionMsg.Direction = diff.ToStaticDirections();
                _controller.gameObject.SendMessageTo(setFaceDirectionMsg, _controller.transform.parent.gameObject);
                MessageFactory.CacheMessage(setFaceDirectionMsg);

                var target = msg.Target;
                var doBumpMsg = MessageFactory.GenerateDoBumpOverPixelsPerSecondMsg();
                doBumpMsg.Direction = diff.normalized;
                doBumpMsg.OnBump = () => { ApplyAttack(target); };
                doBumpMsg.DoAfter = CleanupAttack;
                doBumpMsg.PixelsPerSecond = (int)(_currentAttackSetup.AttackSpeed * BattleLeagueController.AttackSpeedModifier);
                doBumpMsg.Distance = _defaultAttackRange;
                _controller.gameObject.SendMessageTo(doBumpMsg, _controller.transform.parent.gameObject);
                MessageFactory.CacheMessage(doBumpMsg);


            }
        }

        private void UpdateUnitBattleState(UpdateUnitBattleStateMessage msg)
        {
            _battleState = msg.State;
        }

        private void BasicAttackCheck(BasicAttackCheckMessage msg)
        {
            var canAttack = true;
            var canAttackCheckMsg = MessageFactory.GenerateCanAttackCheckMsg();
            canAttackCheckMsg.DoAfter = () => canAttack = false;
            _controller.gameObject.SendMessageTo(canAttackCheckMsg, _controller.transform.parent.gameObject);
            MessageFactory.CacheMessage(canAttackCheckMsg);
            
            if (canAttack && _currentAttackSetup.Range >= msg.Distance)
            {
                var doBasicAttackMsg = MessageFactory.GenerateDoBasicAttackMsg();
                doBasicAttackMsg.Direction = (msg.TargetTile.Position - msg.Origin.Position).Normalize();
                doBasicAttackMsg.Target = msg.Target;
                _controller.gameObject.SendMessageTo(doBasicAttackMsg, _controller.transform.parent.gameObject);
                MessageFactory.CacheMessage(doBasicAttackMsg);
                msg.DoAfter.Invoke();
            }
        }

        private void SetBasicAttackSetup(SetBasicAttackSetupMessage msg)
        {
            _currentAttackSetup = msg.Setup;
        }

        private void ClearBasicAttackSetup(ClearBasicAttackSetupMessage msg)
        {
            if (msg.Setup == _currentAttackSetup)
            {
                _currentAttackSetup = _defaultSetup;
            }
        }

        private void QueryBasicAttackSetup(QueryBasicAttackSetupMessage msg)
        {
            msg.DoAfter.Invoke(_currentAttackSetup);
        }

        private void Stun(StunMessage msg)
        {
            InterruptAttack();
        }

        private void Disarm(DisarmMessage msg)
        {
            InterruptAttack();
        }
    }
}