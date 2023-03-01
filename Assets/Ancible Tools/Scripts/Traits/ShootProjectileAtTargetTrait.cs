using Assets.Resources.Ancible_Tools.Scripts.System;
using Assets.Resources.Ancible_Tools.Scripts.System.BattleLeague;
using Assets.Resources.Ancible_Tools.Scripts.System.Combat;
using Assets.Resources.Ancible_Tools.Scripts.System.Minigame;
using MessageBusLib;
using UnityEngine;

namespace Assets.Ancible_Tools.Scripts.Traits
{
    [CreateAssetMenu(fileName = "Shoot Projectile at Target Trait", menuName = "Ancible Tools/Traits/Combat/Shoot Projectile at Target")]
    public class ShootProjectileAtTargetTrait : Trait
    {
        [SerializeField] private SpriteTrait _sprite = null;
        [SerializeField] private Trait[] _applyOnContact = new Trait[0];
        [SerializeField] private int _pixelsPerSecond = 0;
        [SerializeField] private float _offset = 0f;
        [SerializeField] private bool _rotate = false;
        [SerializeField] private float _rotationOffset = 0f;

        private ProjectileController _projectile = null;

        public override string GetDescription(bool equipment = false)
        {
            var description = string.Empty;
            var applyDescriptions = _applyOnContact.GetTraitDescriptions();
            if (applyDescriptions.Length > 0)
            {
                description = "Shoots a projectile at a target applying";
                for (var i = 0; i < applyDescriptions.Length; i++)
                {
                    if (i < applyDescriptions.Length - 1)
                    {

                        description = $"{description} {applyDescriptions[i]}";
                        if (i + 1 < applyDescriptions.Length - 1)
                        {
                            description = $"{description},";
                        }
                    }
                    else
                    {
                        description = i == 0 ? $"{description} {applyDescriptions[i]}" : $"{description} and {applyDescriptions[i]}";
                    }
                }
            }
            else
            {
                description = "Shoots a projectile at a target";
            }

            return description;
        }

        public override void SetupController(TraitController controller)
        {
            base.SetupController(controller);

            GameObject owner = null;
            var queryOwnerMsg = MessageFactory.GenerateQueryOwnerMsg();
            queryOwnerMsg.DoAfter = obj => owner = obj;
            _controller.gameObject.SendMessageTo(queryOwnerMsg, _controller.Sender);
            MessageFactory.CacheMessage(queryOwnerMsg);

            if (owner)
            {
                var ownerPos = owner.transform.position.ToVector2();
                var targetPos = _controller.transform.parent.position.ToVector2();
                var diff = (targetPos - ownerPos);
                var direction = diff.normalized;
                var pos = ownerPos + (direction * _offset);
                _projectile = Instantiate(FactoryController.PROJECTILE_CONTROLLER, pos, Quaternion.identity);
                _projectile.gameObject.layer = _controller.transform.parent.gameObject.layer;
                _projectile.Setup(_sprite, _applyOnContact, _pixelsPerSecond, _controller.transform.parent.gameObject, owner, TargetReached);
                if (_rotate)
                {
                    _projectile.SetRotation(_rotate, _rotationOffset);
                }
                SubscribeToMessages();
            }
            else
            {
                var removeTraitFromUnitByControllerMsg = MessageFactory.GenerateRemoveTraitFromUnitByControllerMsg();
                removeTraitFromUnitByControllerMsg.Controller = _controller;
                _controller.gameObject.SendMessageTo(removeTraitFromUnitByControllerMsg, _controller.transform.parent.gameObject);
                MessageFactory.CacheMessage(removeTraitFromUnitByControllerMsg);
            }
            
        }

        private void TargetReached()
        {
            var removeTraitFromUnitByControllerMsg = MessageFactory.GenerateRemoveTraitFromUnitByControllerMsg();
            removeTraitFromUnitByControllerMsg.Controller = _controller;
            _controller.gameObject.SendMessageTo(removeTraitFromUnitByControllerMsg, _controller.transform.parent.gameObject);
            MessageFactory.CacheMessage(removeTraitFromUnitByControllerMsg);
        }

        private void SubscribeToMessages()
        {
            _controller.transform.parent.gameObject.SubscribeWithFilter<UpdateUnitBattleStateMessage>(UpdateUnitBattleState, _instanceId);
            _controller.transform.parent.gameObject.SubscribeWithFilter<UpdateMinigameUnitStateMessage>(UpdateMinigameUnitState, _instanceId);
        }

        private void UpdateUnitBattleState(UpdateUnitBattleStateMessage msg)
        {
            if (msg.State == UnitBattleState.Dead)
            {
                var removeTraitFromUnitByControllerMsg = MessageFactory.GenerateRemoveTraitFromUnitByControllerMsg();
                removeTraitFromUnitByControllerMsg.Controller = _controller;
                _controller.gameObject.SendMessageTo(removeTraitFromUnitByControllerMsg, _controller.transform.parent.gameObject);
                MessageFactory.CacheMessage(removeTraitFromUnitByControllerMsg);
            }
        }

        private void UpdateMinigameUnitState(UpdateMinigameUnitStateMessage msg)
        {
            if (msg.State == MinigameUnitState.Disabled)
            {
                var removeTraitFromUnitByControllerMsg = MessageFactory.GenerateRemoveTraitFromUnitByControllerMsg();
                removeTraitFromUnitByControllerMsg.Controller = _controller;
                _controller.gameObject.SendMessageTo(removeTraitFromUnitByControllerMsg, _controller.transform.parent.gameObject);
                MessageFactory.CacheMessage(removeTraitFromUnitByControllerMsg);
            }
        }

        public override void Destroy()
        {
            if (_projectile)
            {
                _projectile.Destroy();
                Destroy(_projectile.gameObject);
                _projectile = null;
            }
            base.Destroy();
        }
    }
}