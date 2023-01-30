using System.Collections.Generic;
using System.Linq;
using Assets.Resources.Ancible_Tools.Scripts.System.SaveData;
using MessageBusLib;
using UnityEngine;

namespace Assets.Resources.Ancible_Tools.Scripts.System
{
    public class WorldHobblerManager : MonoBehaviour
    {
        public static List<GameObject> Roster { get; private set; }
        public static List<GameObject> All { get; private set; }
        public static int PopulationLimit => _instance._populationLimit;
        public static bool AvailablePopulation => All.Count < PopulationLimit;
        public static Transform Transform => _instance.transform;
        public static int RosterLimit => _instance._rosterLimit;

        private static WorldHobblerManager _instance = null;

        [SerializeField] private int _rosterLimit = 15;
        [SerializeField] private int _populationLimit = 30;
        [SerializeField] private string _templateFolder = string.Empty;
        
        private Dictionary<string, HobblerTemplate> _allHobblers = new Dictionary<string, HobblerTemplate>();

        private SetupHobblerFromDataMessage _setupHobblerFromDataMsg = new SetupHobblerFromDataMessage();

        void Awake()
        {
            if (_instance)
            {
                Destroy(gameObject);
                return;
            }

            _instance = this;
            Roster = new List<GameObject>();
            All = new List<GameObject>();
            _allHobblers = UnityEngine.Resources.LoadAll<HobblerTemplate>(_templateFolder).ToDictionary(h => h.name, h => h);
            Debug.Log($"Loaded {_allHobblers.Count} Hobbler Templates");
            SubscribeToMessages();
        }

        public static void RegisterHobbler(GameObject unit)
        {
            if (!All.Contains(unit) && All.Count < PopulationLimit)
            {
                All.Add(unit);
                _instance.gameObject.SendMessage(WorldPopulationUpdatedMessage.INSTANCE);
            }
        }

        public static void AddHobblerToRoster(GameObject unit)
        {
            if (All.Contains(unit) && !Roster.Contains(unit) && Roster.Count < _instance._rosterLimit)
            {
                Roster.Add(unit);
                _instance.gameObject.SendMessage(WorldPopulationUpdatedMessage.INSTANCE);
            }
        }

        public static void RemoveHobblerFromRoster(GameObject unit)
        {
            if (All.Contains(unit) && Roster.Contains(unit))
            {
                Roster.Remove(unit);
                _instance.gameObject.SendMessage(WorldPopulationUpdatedMessage.INSTANCE);
            }
        }

        public static HobblerData[] GetData()
        {
            var returnData = new List<HobblerData>();
            var queryHobblerDataMsg = MessageFactory.GenerateQueryHobblerDataMsg();
            queryHobblerDataMsg.DoAfter = data => returnData.Add(data);
            var units = All.ToArray();
            foreach (var unit in units)
            {
                _instance.gameObject.SendMessageTo(queryHobblerDataMsg, unit);
            }
            MessageFactory.CacheMessage(queryHobblerDataMsg);

            return returnData.ToArray();
        }

        public static void Clear()
        {
            var hobblers = All.ToArray();
            foreach (var hobbler in hobblers)
            {
                Destroy(hobbler);
            }
            All.Clear();
            Roster.Clear();
        }

        public static HobblerTemplate GetTemplateByName(string templateName)
        {
            if (_instance._allHobblers.TryGetValue(templateName, out var template))
            {
                return template;
            }

            return null;
        }

        public static void LoadHobblersFromData(HobblerData[] hobblers)
        {
            Clear();
            foreach (var hobbler in hobblers)
            {
                var mapTile = WorldController.Pathing.GetTileByPosition(hobbler.Position.ToVector());
                if (mapTile != null)
                {
                    
                    var unit = GenerateHobblerFromData(hobbler);
                    RegisterHobbler(unit);
                    if (hobbler.Roster)
                    {
                        AddHobblerToRoster(unit);
                    }
                }
            }
            Debug.Log($"Loaded {All.Count} Hobblers from data");
            Debug.Log($"Added {Roster.Count} Hobblers to roster from data");
        }

        private static GameObject GenerateHobblerFromData(HobblerData data)
        {
            var mapTile = WorldController.Pathing.GetTileByPosition(data.Position.ToVector());
            if (mapTile != null)
            {
                var unitController = FactoryController.HOBBLER_TEMPLATE.GenerateUnit(Transform, mapTile.World);
                unitController.gameObject.layer = WorldController.Transform.gameObject.layer;
                _instance._setupHobblerFromDataMsg.Data = data;
                _instance.gameObject.SendMessageTo(_instance._setupHobblerFromDataMsg, unitController.gameObject);

                var setMapTileMsg = MessageFactory.GenerateSetMapTileMsg();
                setMapTileMsg.Tile = mapTile;
                _instance.gameObject.SendMessageTo(setMapTileMsg, unitController.gameObject);
                MessageFactory.CacheMessage(setMapTileMsg);

                return unitController.gameObject;
            }

            return null;

        }

        private void SubscribeToMessages()
        {
            gameObject.Subscribe<ClearWorldMessage>(ClearWorld);
        }

        private void ClearWorld(ClearWorldMessage msg)
        {
            Clear();
        }

        void OnDestroy()
        {
            gameObject.UnsubscribeFromAllMessages();
        }

    }
}