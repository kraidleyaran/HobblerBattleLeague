using Assets.Resources.Ancible_Tools.Scripts.System;
using Assets.Resources.Ancible_Tools.Scripts.System.WorldEvents;
using MessageBusLib;
using UnityEngine;

namespace Assets.Ancible_Tools.Scripts.Traits
{
    [CreateAssetMenu(fileName = "World Event Receiver", menuName = "Ancible Tools/Traits/World Event/World Event Receiver")]
    public class WorldEventReceiverTrait : Trait
    {
        [SerializeField] private WorldEvent _event;
        [SerializeField] private Trait[] _applyOnWorldEvent;
        [SerializeField] private int _applyMax = 0;
        public bool Save;
        [HideInInspector] public string SaveId;

        private int _applyCount = 0;

        public override void SetupController(TraitController controller)
        {
            base.SetupController(controller);
            if (Save)
            {
                var data = WorldEventManager.GetRecieverData(SaveId);
                if (data != null)
                {
                    _applyCount = data.TriggerCount;
                }
            }
            SubscribeToMessages();
        }

        private void SubscribeToMessages()
        {
            if (_applyCount < _applyMax)
            {
                _controller.gameObject.SubscribeWithFilter<TriggerWorldEventMessage>(TriggerWorldEvent, _event.GenerateFilter());
            }
        }

        private void TriggerWorldEvent(TriggerWorldEventMessage msg)
        {
            _applyCount++;
            _controller.gameObject.AddTraitsToUnit(_applyOnWorldEvent, _controller.transform.parent.gameObject);
            if (_applyCount >= _applyMax)
            {
                _controller.gameObject.UnsubscribeFromFilter<TriggerWorldEventMessage>(_event.GenerateFilter());
            }

            if (Save)
            {
                WorldEventManager.SetReceiverData(SaveId, _applyCount);
            }
        }
    }
}