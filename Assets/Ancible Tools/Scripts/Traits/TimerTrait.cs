using System.Collections.Generic;
using Assets.Resources.Ancible_Tools.Scripts.System;
using Assets.Resources.Ancible_Tools.Scripts.System.TickTimers;
using MessageBusLib;
using UnityEngine;

namespace Assets.Ancible_Tools.Scripts.Traits
{
    [CreateAssetMenu(fileName = "Timer Trait", menuName = "Ancible Tools/Traits/General/Timer")]
    public class TimerTrait : Trait
    {
        [SerializeField] private int _ticks;
        [SerializeField] private int _loops;
        [SerializeField] private Trait[] _applyOnStart = new Trait[0];
        [SerializeField] private Trait[] _applyOnComplete = new Trait[0];
        [SerializeField] private bool _refreshTimer = false;

        private TraitController[] _appliedTraits = new TraitController[0];
        private TickTimer _tickTimer = null;
        private GameObject _owner = null;
        private string _filter = string.Empty;

        public override void SetupController(TraitController controller)
        {
            base.SetupController(controller);

            
            if (_refreshTimer)
            {
                var refreshTimerMsg = MessageFactory.GenerateRefreshTimerMsg();
                refreshTimerMsg.Trait = _controller.Prefab as TimerTrait;
                _controller.gameObject.SendMessageTo(refreshTimerMsg, _controller.transform.parent.gameObject);
                MessageFactory.CacheMessage(refreshTimerMsg);

                if (MaxStack > 0)
                {
                    var timerCount = 0;
                    var queryTimerMsg = MessageFactory.GenerateQueryTimerMsg();
                    var timerPrefab = _controller.Prefab as TimerTrait;
                    queryTimerMsg.Trait = timerPrefab;
                    queryTimerMsg.DoAfter = () => timerCount++;
                    _controller.gameObject.SendMessageTo(queryTimerMsg, _controller.transform.parent.gameObject);
                    MessageFactory.CacheMessage(queryTimerMsg);

                    if (timerCount < MaxStack - 1)
                    {
                        StartTimer();
                    }
                    else
                    {
                        var removeTraitFromUnitByControllerMsg = MessageFactory.GenerateRemoveTraitFromUnitByControllerMsg();
                        removeTraitFromUnitByControllerMsg.Controller = _controller;
                        _controller.gameObject.SendMessageTo(removeTraitFromUnitByControllerMsg, _controller.transform.parent.gameObject);
                        MessageFactory.CacheMessage(removeTraitFromUnitByControllerMsg);
                    }
                }
                else
                {
                    StartTimer();
                }
            }
            else
            {
                StartTimer();
            }
            
        }

        private void StartTimer()
        {
            var parent = _controller.transform.parent.gameObject;
            if (_controller.Sender != null)
            {
                GameObject possibleOwner = null;
                var queryOwnerMsg = MessageFactory.GenerateQueryOwnerMsg();
                queryOwnerMsg.DoAfter = owner => possibleOwner = owner;
                _controller.gameObject.SendMessageTo(queryOwnerMsg, _controller.Sender);
                MessageFactory.CacheMessage(queryOwnerMsg);
                if (possibleOwner)
                {
                    parent = possibleOwner;
                    _owner = possibleOwner;
                }
            }

            if (_applyOnStart.Length > 0)
            {
                var addTraitToUnitMsg = MessageFactory.GenerateAddTraitToUnitMsg();
                var traits = new List<TraitController>();
                foreach (var trait in _applyOnStart)
                {
                    if (trait.Instant)
                    {
                        addTraitToUnitMsg.DoAfter = null;
                    }
                    else
                    {
                        addTraitToUnitMsg.DoAfter = traitController => traits.Add(traitController);
                    }
                    parent.SendMessageTo(addTraitToUnitMsg, _controller.transform.parent.gameObject);
                }
                MessageFactory.CacheMessage(addTraitToUnitMsg);
                _appliedTraits = traits.ToArray();
            }

            _tickTimer = new TickTimer(_ticks, _loops, ApplyComplete, Finish);
            SubscribeToMessages();
        }

        private void ApplyComplete()
        {
            var addTraitToUnitMsg = MessageFactory.GenerateAddTraitToUnitMsg();
            var parent = _owner ? _owner : _controller.transform.parent.gameObject;
            foreach (var trait in _applyOnComplete)
            {
                addTraitToUnitMsg.Trait = trait;
                parent.SendMessageTo(addTraitToUnitMsg, _controller.transform.parent.gameObject);
            }
            MessageFactory.CacheMessage(addTraitToUnitMsg);

        }

        private void Finish()
        {
            var removeTraitFromUnitByControllerMsg = MessageFactory.GenerateRemoveTraitFromUnitByControllerMsg();
            if (_appliedTraits.Length > 0)
            {
                foreach (var trait in _appliedTraits)
                {
                    removeTraitFromUnitByControllerMsg.Controller = trait;
                    _controller.gameObject.SendMessageTo(removeTraitFromUnitByControllerMsg, _controller.transform.parent.gameObject);
                }
                
            }
            _tickTimer.Destroy();
            _tickTimer = null;
            removeTraitFromUnitByControllerMsg.Controller = _controller;
            _controller.gameObject.SendMessageTo(removeTraitFromUnitByControllerMsg, _controller.transform.parent.gameObject);
            MessageFactory.CacheMessage(removeTraitFromUnitByControllerMsg);
        }

        private void SubscribeToMessages()
        {
            if (_refreshTimer)
            {
                _controller.transform.parent.gameObject.SubscribeWithFilter<RefreshTimerMessage>(RefreshTimer, _instanceId);
            }
        }

        private void RefreshTimer(RefreshTimerMessage msg)
        {
            if (msg.Trait == _controller.Prefab)
            {
                _tickTimer?.Restart();
            }
        }

        private void QueryTimer(QueryTimerMessage msg)
        {
            if (msg.Trait == _controller.Prefab)
            {
                msg.DoAfter.Invoke();
            }
        }

        public override void Destroy()
        {
            if (_appliedTraits.Length > 0)
            {
                var removeTraitFromUnitByControllerMsg = MessageFactory.GenerateRemoveTraitFromUnitByControllerMsg();
                foreach (var trait in _appliedTraits)
                {
                    removeTraitFromUnitByControllerMsg.Controller = trait;
                    _controller.gameObject.SendMessageTo(removeTraitFromUnitByControllerMsg, _controller.transform.parent.gameObject);
                }
                MessageFactory.CacheMessage(removeTraitFromUnitByControllerMsg);
            }
            _tickTimer?.Destroy();
            _tickTimer = null;
            _controller.transform.parent.gameObject.UnsubscribeFromAllMessagesWithFilter(_filter);
            base.Destroy();
        }
    }
}