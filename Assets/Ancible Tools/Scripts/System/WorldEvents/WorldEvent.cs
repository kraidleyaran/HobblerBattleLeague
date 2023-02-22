using UnityEngine;

namespace Assets.Resources.Ancible_Tools.Scripts.System.WorldEvents
{
    [CreateAssetMenu(fileName = "World Event", menuName = "Ancible Tools/World Event")]
    public class WorldEvent : ScriptableObject
    {
        public const string FILTER = "WORLD_EVENT-";

        public bool Save;
        [HideInInspector] public string SaveId;
        public int MaxTriggerCount = 1;

        public string GenerateFilter()
        {
            return WorldEventManager.GenerateFilter(this);
        }
    }
}