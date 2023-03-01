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

        public override string GetDescription(bool equipment = false)
        {
            var description = string.Empty;
            if (_loops > 0)
            {
                if (_applyOnStart.Length > 0)
                {
                    var startDescriptions = _applyOnStart.GetTraitDescriptions();
                    for (var i = 0; i < startDescriptions.Length; i++)
                    {
                        if (i == 0)
                        {
                            description = $"{description} {startDescriptions[i]}";
                        }
                        else if (i < startDescriptions.Length)
                        {
                            description = $"{description}, {startDescriptions[i]}";
                        }
                        else
                        {
                            description = $"{description}, and {startDescriptions[i]}.";
                        }
                    }
                }


                description = $"{description}. ";
                var endDescriptions = _applyOnComplete.GetTraitDescriptions();
                for (var i = 0; i < endDescriptions.Length; i++)
                {
                    if (i == 0)
                    {
                        description = $"{description} {endDescriptions[i]}";
                    }
                    else if (i < endDescriptions.Length)
                    {
                        description = $"{description}, {endDescriptions[i]}";
                    }
                    else
                    {
                        description = $"{description}, and {endDescriptions[i]}";
                    }
                }

                description = $"{description} every {_ticks * TickController.TickRate:N} seconds, {_loops + 1} times";
            }
            else
            {
                if (_applyOnComplete.Length <= 0)
                {
                    description = string.Empty;
                    var startDescriptions = _applyOnStart.GetTraitDescriptions();
                    for (var i = 0; i < startDescriptions.Length; i++)
                    {
                        if (i == 0)
                        {
                            description = $"{startDescriptions[i]}";
                        }
                        else if (i < startDescriptions.Length)
                        {
                            description = $"{description}, {startDescriptions[i]}";
                        }
                        else
                        {
                            description = $"{description}, and {startDescriptions[i]}";
                        }
                    }

                    description = $"{description} for {_ticks * TickController.TickRate:N} seconds";
                }
                else if (_applyOnStart.Length <= 0)
                {
                    description = $"After {_ticks * TickController.TickRate:N} seconds, ";

                    var endDescriptions = _applyOnComplete.GetTraitDescriptions();
                    for (var i = 0; i < endDescriptions.Length; i++)
                    {
                        if (i == 0)
                        {
                            description = $"{description} {endDescriptions[i]}";
                        }
                        else if (i < endDescriptions.Length)
                        {
                            description = $"{description}, {endDescriptions[i]}";
                        }
                        else
                        {
                            description = $"{description}, and {endDescriptions[i]}";
                        }
                    }
                }
                else
                {
                    var startDescriptions = _applyOnStart.GetTraitDescriptions();
                    for (var i = 0; i < startDescriptions.Length; i++)
                    {
                        if (i == 0)
                        {
                            description = $"{description} {startDescriptions[i]}";
                        }
                        else if (i < startDescriptions.Length)
                        {
                            description = $"{description}, {startDescriptions[i]}";
                        }
                        else
                        {
                            description = $"{description}, and {startDescriptions[i]}.";
                        }
                    }

                    description = $"{description}. ";
                    var endDescriptions = _applyOnComplete.GetTraitDescriptions();
                    for (var i = 0; i < endDescriptions.Length; i++)
                    {
                        if (i == 0)
                        {
                            description = $"{description} {endDescriptions[i]}";
                        }
                        else if (i < endDescriptions.Length)
                        {
                            description = $"{description}, {endDescriptions[i]}";
                        }
                        else
                        {
                            description = $"{description}, and {endDescriptions[i]}";
                        }
                    }

                    description = $"{description} every {_ticks * TickController.TickRate:N} seconds, {_loops + 1} times";
                }
            }

            return description;
        }

        private void StartTimer()
        {


            if (_applyOnStart.Length > 0)
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

                var addTraitToUnitMsg = MessageFactory.GenerateAddTraitToUnitMsg();
                var traits = new List<TraitController>();
                foreach (var trait in _applyOnStart)
                {
                    addTraitToUnitMsg.Trait = trait;
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