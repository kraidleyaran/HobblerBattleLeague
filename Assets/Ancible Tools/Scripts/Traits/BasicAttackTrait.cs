using System.Linq;
using Assets.Resources.Ancible_Tools.Scripts.System;
using Assets.Resources.Ancible_Tools.Scripts.System.Combat;
using Assets.Resources.Ancible_Tools.Scripts.System.Minigame;
using MessageBusLib;
using UnityEngine;

namespace Assets.Ancible_Tools.Scripts.Traits
{
    [CreateAssetMenu(fileName = "Basic Attack Trait", menuName = "Ancible Tools/Traits/Minigame/Combat/Basic Attack")]
    public class BasicAttackTrait : Trait
    {
        public BasicAttackSetup AttackSetup => _attackSetup;

        [SerializeField] private BasicAttackSetup _attackSetup = new BasicAttackSetup();
        [SerializeField] private bool _activateGlobalCooldown = true;
        [SerializeField] private float _defaultAttackRange = 16 * .3125f;

        private BasicAttackSetup _currentAttackSetup = null;

        private MinigameUnitState _unitState = MinigameUnitState.Idle;
        private bool _globalCooldown = false;

        public override void SetupController(TraitController controller)
        {
            base.SetupController(controller);
            _currentAttackSetup = _attackSetup;
            SubscribeToMessages();
        }

        private void ApplyAttack(GameObject target)
        {
            var addTraitToUnitMsg = MessageFactory.GenerateAddTraitToUnitMsg();
            for (var i = 0; i < _currentAttackSetup.ApplyToTarget.Length; i++)
            {
                addTraitToUnitMsg.Trait = _currentAttackSetup.ApplyToTarget[i];
                _controller.transform.parent.gameObject.SendMessageTo(addTraitToUnitMsg, target);
            }
            MessageFactory.CacheMessage(addTraitToUnitMsg);
        }

        private void CleanupAttack()
        {
            if (_unitState != MinigameUnitState.Disabled)
            {
                var setUnitStateMsg = MessageFactory.GenerateSetMinigameUnitStateMsg();
                setUnitStateMsg.State = MinigameUnitState.Idle;
                _controller.gameObject.SendMessageTo(setUnitStateMsg, _controller.transform.parent.gameObject);
                MessageFactory.CacheMessage(setUnitStateMsg);

                if (_activateGlobalCooldown)
                {
                    _controller.gameObject.SendMessageTo(ActivateGlobalCooldownMessage.INSTANCE, _controller.transform.parent.gameObject);
                }
            }
        }

        private void InterruptAttack()
        {
            if (_unitState == MinigameUnitState.Interact)
            {
                _controller.gameObject.SendMessageTo(InterruptBumpMessage.INSTANCE, _controller.transform.parent.gameObject);

                var setUnitStateMsg = MessageFactory.GenerateSetMinigameUnitStateMsg();
                setUnitStateMsg.State = MinigameUnitState.Idle;
                _controller.gameObject.SendMessageTo(setUnitStateMsg, _controller.transform.parent.gameObject);
                MessageFactory.CacheMessage(setUnitStateMsg);
            }
        }

        private void SubscribeToMessages()
        {
            _controller.transform.parent.gameObject.SubscribeWithFilter<DoBasicAttackMessage>(DoBasicAttack, _instanceId);
            _controller.transform.parent.gameObject.SubscribeWithFilter<UpdateMinigameUnitStateMessage>(UpdateMinigameUnitState, _instanceId);
            _controller.transform.parent.gameObject.SubscribeWithFilter<BasicAttackCheckMessage>(BasicAttackCheck, _instanceId);
            _controller.transform.parent.gameObject.SubscribeWithFilter<SetBasicAttackSetupMessage>(SetBasicAttackSetup, _instanceId);
            _controller.transform.parent.gameObject.SubscribeWithFilter<ClearBasicAttackSetupMessage>(ClearBasicAttackSetup, _instanceId);
            _controller.transform.parent.gameObject.SubscribeWithFilter<QueryBasicAttackSetupMessage>(QueryBasicAttackSetup, _instanceId);
            _controller.transform.parent.gameObject.SubscribeWithFilter<StunMessage>(Stun, _instanceId);
            _controller.transform.parent.gameObject.SubscribeWithFilter<DisarmMessage>(Disarm, _instanceId);
        }

        private void DoBasicAttack(DoBasicAttackMessage msg)
        {
            if (_unitState == MinigameUnitState.Idle)
            {
                var setUnitStateMsg = MessageFactory.GenerateSetMinigameUnitStateMsg();
                setUnitStateMsg.State = MinigameUnitState.Interact;
                _controller.gameObject.SendMessageTo(setUnitStateMsg, _controller.transform.parent.gameObject);
                MessageFactory.CacheMessage(setUnitStateMsg);

                var target = msg.Target;
                var doBumpMsg = MessageFactory.GenerateDoBumpOverPixelsPerSecondMsg();
                doBumpMsg.Direction = msg.Direction;
                doBumpMsg.OnBump = () => { ApplyAttack(target); };
                doBumpMsg.DoAfter = CleanupAttack;
                doBumpMsg.PixelsPerSecond = _currentAttackSetup.AttackSpeed;
                doBumpMsg.Distance = _defaultAttackRange;
                _controller.gameObject.SendMessageTo(doBumpMsg, _controller.transform.parent.gameObject);
                MessageFactory.CacheMessage(doBumpMsg);
            }
        }

        private void UpdateMinigameUnitState(UpdateMinigameUnitStateMessage msg)
        {
            _unitState = msg.State;
        }

        private void BasicAttackCheck(BasicAttackCheckMessage msg)
        {
            var canAttack = true;
            var canAttackCheckMsg = MessageFactory.GenerateCanAttackCheckMsg();
            canAttackCheckMsg.DoAfter = () => canAttack = false;
            _controller.gameObject.SendMessageTo(canAttackCheckMsg, _controller.transform.parent.gameObject);
            MessageFactory.CacheMessage(canAttackCheckMsg);

            if (canAttack)
            {
                var areaTiles = MinigameController.Pathing.GetTilesInPov(msg.Origin.Position, _currentAttackSetup.Range);
                if (areaTiles.Contains(msg.TargetTile))
                {
                    var doBasicAttackMsg = MessageFactory.GenerateDoBasicAttackMsg();
                    doBasicAttackMsg.Direction = (msg.TargetTile.Position - msg.Origin.Position).Normalize();
                    doBasicAttackMsg.Target = msg.Target;
                    _controller.gameObject.SendMessageTo(doBasicAttackMsg, _controller.transform.parent.gameObject);
                    MessageFactory.CacheMessage(doBasicAttackMsg);
                    msg.DoAfter.Invoke();
                }
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
                _currentAttackSetup = _attackSetup;
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