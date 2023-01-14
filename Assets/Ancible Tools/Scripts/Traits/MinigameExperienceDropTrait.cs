using System.Collections.Generic;
using Assets.Resources.Ancible_Tools.Scripts.System;
using MessageBusLib;
using UnityEngine;

namespace Assets.Ancible_Tools.Scripts.Traits
{
    [CreateAssetMenu(fileName = "Minigame Experience Drop Trait", menuName = "Ancible Tools/Traits/Minigame/Combat/Minigame Experience Drop")]
    public class MinigameExperienceDropTrait : Trait
    {
        [SerializeField] private IntNumberRange _amount = new IntNumberRange();

        private List<GameObject> _receivers = new List<GameObject>();

        public override void SetupController(TraitController controller)
        {
            base.SetupController(controller);
            SubscribeToMessages();
        }

        private void SubscribeToMessages()
        {
            _controller.transform.parent.gameObject.SubscribeWithFilter<UnitDiedMessage>(UnitDied, _instanceId);
            _controller.transform.parent.gameObject.SubscribeWithFilter<ReportDamageMessage>(ReportDamage, _instanceId);
        }

        private void UnitDied(UnitDiedMessage msg)
        {
            if (_receivers.Count > 0)
            {
                var addExperienceMsg = MessageFactory.GenerateAddExperienceMsg();
                addExperienceMsg.Amount = _amount.Roll();
                for (var i = 0; i < _receivers.Count; i++)
                {
                    if (_receivers[i])
                    {
                        _controller.gameObject.SendMessageTo(addExperienceMsg, _receivers[i]);
                    }
                    
                }
                MessageFactory.CacheMessage(addExperienceMsg);
                _receivers.Clear();
            }
        }

        private void ReportDamage(ReportDamageMessage msg)
        {
            if (!_receivers.Contains(msg.Owner))
            {
                _receivers.Add(msg.Owner);
            }
        }
    }
}