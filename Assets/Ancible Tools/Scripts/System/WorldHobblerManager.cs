using System.Collections.Generic;
using System.Linq;
using Assets.Ancible_Tools.Scripts.System.Factories;
using Assets.Ancible_Tools.Scripts.System.Wellbeing;
using Assets.Ancible_Tools.Scripts.Traits;
using Assets.Resources.Ancible_Tools.Scripts.System.Items;
using Assets.Resources.Ancible_Tools.Scripts.System.SaveData;
using Assets.Resources.Ancible_Tools.Scripts.System.UI.Alerts;
using MessageBusLib;
using UnityEngine;

namespace Assets.Resources.Ancible_Tools.Scripts.System
{
    public class WorldHobblerManager : MonoBehaviour
    {
        public static List<GameObject> Roster { get; private set; }
        public static List<GameObject> All { get; private set; }
        public static List<GameObject> Unhappy { get; private set; }
        public static int PopulationLimit => _instance._maxPopulation;
        public static bool AvailablePopulation => All.Count < PopulationLimit;
        public static Transform Transform => _instance.transform;
        public static int RosterLimit => _instance._maxRoster;
        public static float ExileAmountPercent => _instance._exileAmountPercent;

        private static WorldHobblerManager _instance = null;

        [SerializeField] private int _rosterLimit = 3;
        [SerializeField] private int _populationLimit = 5;
        [SerializeField] private string _templateFolder = string.Empty;
        [SerializeField] private float _exileAmountPercent = .33f;
        [SerializeField] private Trait[] _applyOnExile = new Trait[0];
        
        private Dictionary<string, HobblerTemplate> _allHobblers = new Dictionary<string, HobblerTemplate>();

        private SetupHobblerFromDataMessage _setupHobblerFromDataMsg = new SetupHobblerFromDataMessage();
        private ApplyRosterStatusMessage _applyRosterStatusMsg = new ApplyRosterStatusMessage();

        private int _maxPopulation = 0;
        private int _maxRoster = 0;

        void Awake()
        {
            if (_instance)
            {
                Destroy(gameObject);
                return;
            }

            _instance = this;
            _maxPopulation = _populationLimit;
            _maxRoster = _rosterLimit;
            Roster = new List<GameObject>();
            All = new List<GameObject>();
            Unhappy = new List<GameObject>();
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
            if (All.Contains(unit) && !Roster.Contains(unit) && Roster.Count < _instance._maxRoster)
            {
                Roster.Add(unit);
                _instance._applyRosterStatusMsg.Roster = true;
                _instance.gameObject.SendMessageTo(_instance._applyRosterStatusMsg, unit);
                _instance.gameObject.SendMessage(WorldPopulationUpdatedMessage.INSTANCE);
            }
        }

        public static void RemoveHobblerFromRoster(GameObject unit)
        {
            if (All.Contains(unit) && Roster.Contains(unit))
            {
                Roster.Remove(unit);
                _instance._applyRosterStatusMsg.Roster = false;
                _instance.gameObject.SendMessageTo(_instance._applyRosterStatusMsg, unit);
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

        

        public static void ExileHobbler(GameObject hobbler)
        {
            HobblerData data = null;
            var queryHobblerMsg = MessageFactory.GenerateQueryHobblerDataMsg();
            queryHobblerMsg.DoAfter = hobblerData => data = hobblerData;
            _instance.gameObject.SendMessageTo(queryHobblerMsg, hobbler);
            MessageFactory.CacheMessage(queryHobblerMsg);

            var template = GetTemplateByName(data.Template);
            if (template)
            {
                _instance.gameObject.AddTraitsToUnit(_instance._applyOnExile, hobbler);
                UiAlertManager.ShowAlert($"{data.Name}", template.Sprite.Sprite, ColorFactoryController.ErrorAlertText);
                WorldStashController.AddGold((int)(template.Cost * ExileAmountPercent));
            }
            All.Remove(hobbler);
            Roster.Remove(hobbler);
            Destroy(hobbler);
            _instance.gameObject.SendMessage(WorldPopulationUpdatedMessage.INSTANCE);
        }

        public static int GetExileGold(int amount)
        {
            return (int) (amount * ExileAmountPercent);
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

        public static void SetMaxPopulation(int max)
        {
            _instance._maxPopulation = max;
            _instance.gameObject.SendMessage(WorldPopulationUpdatedMessage.INSTANCE);
        }

        public static void SetMaxRoster(int max)
        {
            _instance._maxRoster = max;
            _instance.gameObject.SendMessage(WorldPopulationUpdatedMessage.INSTANCE);
        }

        public static void IncreaseMaxPopulation(int amount)
        {
            _instance._maxPopulation += amount;
            _instance.gameObject.SendMessage(WorldPopulationUpdatedMessage.INSTANCE);
        }

        public static void IncreaseMaxRoster(int amount)
        {
            _instance._maxRoster += amount;
            _instance.gameObject.SendMessage(WorldPopulationUpdatedMessage.INSTANCE);
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

        public static GameObject[] GetAvailableRoster()
        {
            var hobblers = new List<GameObject>();
            var queryHappinessMsg = MessageFactory.GenerateQueryHappinessMsg();
            var state = HappinessState.Moderate;
            queryHappinessMsg.DoAfter = (happinessState) => {  state = happinessState; };
            for (var i = 0; i < Roster.Count; i++)
            {
                _instance.gameObject.SendMessageTo(queryHappinessMsg, Roster[i]);
                if (state != HappinessState.Unhappy)
                {
                    hobblers.Add(Roster[i]);
                }
            }

            return hobblers.ToArray();
        }

        public static void AddUnhappyHobbler(GameObject hobbler)
        {
            if (!Unhappy.Contains(hobbler))
            {
                Unhappy.Add(hobbler);
                _instance.gameObject.SendMessage(WorldPopulationUpdatedMessage.INSTANCE);
            }
        }

        public static void RemoveUnhappyHobbler(GameObject hobbler)
        {
            if (Unhappy.Contains(hobbler))
            {
                Unhappy.Remove(hobbler);
                _instance.gameObject.SendMessage(WorldPopulationUpdatedMessage.INSTANCE);
            }
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