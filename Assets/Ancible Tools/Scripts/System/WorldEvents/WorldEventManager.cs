using System.Collections.Generic;
using System.Linq;
using Assets.Ancible_Tools.Scripts.System.SaveData;
using MessageBusLib;
using UnityEngine;

namespace Assets.Resources.Ancible_Tools.Scripts.System.WorldEvents
{
    public class WorldEventManager : MonoBehaviour
    {
        private static WorldEventManager _instance = null;

        [SerializeField] private string _worldEventFolder = string.Empty;

        private Dictionary<WorldEvent, WorldEventData> _triggeredEvents = new Dictionary<WorldEvent, WorldEventData>();
        private Dictionary<string, WorldEventReceiverData> _receivers = new Dictionary<string, WorldEventReceiverData>();

        private Dictionary<string, WorldEvent> _worldEvents = new Dictionary<string, WorldEvent>();

        private Dictionary<WorldEvent, string> _generatedFilters = new Dictionary<WorldEvent, string>();
        

        void Awake()
        {
            if (_instance)
            {
                Destroy(gameObject);
                return;
            }

            _instance = this;
            _worldEvents = UnityEngine.Resources.LoadAll<WorldEvent>(_worldEventFolder).ToDictionary(e => e.name, e => e);
        }

        public static void TriggerWorldEvent(WorldEvent worldEvent)
        {
            var activate = false;
            if (_instance._triggeredEvents.TryGetValue(worldEvent, out var data))
            {
                if (data.TriggerCount < worldEvent.MaxTriggerCount)
                {
                    activate = true;
                }
            }
            else
            {
                activate = true;
                _instance._triggeredEvents.Add(worldEvent, new WorldEventData{Name = worldEvent.name, TriggerCount = 1});
            }

            if (activate)
            {
                _instance.gameObject.SendMessageWithFilter(TriggerWorldEventMessage.INSTANCE, worldEvent.GenerateFilter());
            }
        }

        public static void LoadEvents(WorldEventData[] events)
        {
            _instance._triggeredEvents.Clear();
            foreach (var data in events)
            {
                if (_instance._worldEvents.TryGetValue(data.Name, out var worldEvent))
                {
                    _instance._triggeredEvents.Add(worldEvent, data);
                }
            }
        }

        public static void LoadReceivers(WorldEventReceiverData[] receivers)
        {
            _instance._receivers.Clear();
            foreach (var data in receivers)
            {
                if (!_instance._receivers.ContainsKey(data.Id))
                {
                    _instance._receivers.Add(data.Id, data);
                }
            }
        }

        public static WorldEventData[] GetData()
        {
            return _instance._triggeredEvents.Values.ToArray();
        }

        public static WorldEventReceiverData[] GetAllReceiverData()
        {
            return _instance._receivers.Values.ToArray();
        }

        public static WorldEventReceiverData GetRecieverData(string id)
        {
            if (_instance._receivers.TryGetValue(id, out var data))
            {
                return data;
            }

            return null;
        }

        public static void SetReceiverData(string id, int triggerCount)
        {
            if (!_instance._receivers.TryGetValue(id, out var data))
            {
                data = new WorldEventReceiverData {Id = id};
                _instance._receivers.Add(data.Id, data);
            }

            data.TriggerCount = triggerCount;
        }

        public static string GenerateFilter(WorldEvent worldEvent)
        {
            if (!_instance._generatedFilters.TryGetValue(worldEvent, out var filter))
            {
                filter = $"{WorldEvent.FILTER}{worldEvent.name}";
                _instance._generatedFilters.Add(worldEvent, filter);
            }

            return filter;
        }
    }



}