using System.Collections.Generic;
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


    }
}