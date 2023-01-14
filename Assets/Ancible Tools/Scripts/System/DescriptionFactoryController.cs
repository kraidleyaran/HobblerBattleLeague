using Assets.Resources.Ancible_Tools.Scripts.System.Combat;
using UnityEngine;
using UnityEngine.UIElements;

namespace Assets.Resources.Ancible_Tools.Scripts.System
{
    public class DescriptionFactoryController : MonoBehaviour
    {
        public static string Happiness => _instance._happiness;

        private static DescriptionFactoryController _instance = null;


        [Header("Wellbeing Stats")]
        [SerializeField] [TextArea(1,3)] private string _happiness;
        [SerializeField] [TextArea(1, 3)] private string _hunger;
        [SerializeField] [TextArea(1, 3)] private string _fatigue;
        [SerializeField] [TextArea(1, 3)] private string _boredom;
        [SerializeField] [TextArea(1, 3)] private string _ignorance;

        [Header("Combat Stats")]
        [SerializeField] [TextArea(1, 3)] private string _health;
        [SerializeField] [TextArea(1, 3)] private string _mana;
        [SerializeField] [TextArea(1, 3)] private string _spirit;
        [SerializeField] [TextArea(1, 3)] private string _strength;
        [SerializeField] [TextArea(1, 3)] private string _agility;
        [SerializeField] [TextArea(1, 3)] private string _defense;
        [SerializeField] [TextArea(1, 3)] private string _magic;
        [SerializeField] [TextArea(1, 3)] private string _faith;


        void Awake()
        {
            if (_instance)
            {
                Destroy(gameObject);
                return;
            }

            _instance = this;
        }

        public static string GetWellbeingDescription(WellbeingStatType stat)
        {
            switch (stat)
            {
                case WellbeingStatType.Hunger:
                    return _instance._hunger;
                case WellbeingStatType.Boredom:
                    return _instance._boredom;
                case WellbeingStatType.Fatigue:
                    return _instance._fatigue;
                case WellbeingStatType.Ignorance:
                    return _instance._ignorance;
            }
            return string.Empty;
        }

        public static string GetCombatStatDescription(CombatStatType type)
        {
            switch (type)
            {
                case CombatStatType.Health:
                    return _instance._health;
                case CombatStatType.Spirit:
                    return _instance._spirit;
                case CombatStatType.Mana:
                    return _instance._mana;
                case CombatStatType.Strength:
                    return _instance._strength;
                case CombatStatType.Agility:
                    return _instance._agility;
                case CombatStatType.Defense:
                    return _instance._defense;
                case CombatStatType.Magic:
                    return _instance._magic;
                case CombatStatType.Faith:
                    return _instance._faith;
            }

            return string.Empty;
        }
    }
}