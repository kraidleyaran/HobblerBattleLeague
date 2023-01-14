using UnityEngine;

namespace Assets.Resources.Ancible_Tools.Scripts.System
{
    public class WellBeingController : MonoBehaviour
    {
        public const string Happiness = "Happiness";

        public static float HappinessPerHunger => _instance._happinesPerHunger;
        public static float HappinessPerBoredom => _instance._happinessPerBoredom;
        public static float HappinessPerFatigue => _instance._happinessPerFatigue;
        public static float HappinessPerIgnorance => _instance._happinessPerIgnorance;

        public static int TicksPerHungerEffect => _instance._ticksPerHungerEffect;
        public static int HungerPerEffect => _instance._hungerPerEffect;
        public static int TicksPerBoredomEffect => _instance._ticksPerBoredomEffect;
        public static int BoredomPerEffect => _instance._boredomPerEffect;
        public static int TicksPerFatigueEffect => _instance._ticksPerFatigueEffect;
        public static int FatiguePerEffect => _instance._fatiguePerEffect;


        public static Sprite HappinessIcon => _instance._happinessIcon;

        private static WellBeingController _instance = null;

        [Header("Happiness")]
        [SerializeField] private float _happinesPerHunger;
        [SerializeField] private float _happinessPerBoredom;
        [SerializeField] private float _happinessPerFatigue;
        [SerializeField] private float _happinessPerIgnorance;
        [SerializeField] private Sprite _happinessIcon;

        [Header("Well Being Timers")]
        [SerializeField] private int _ticksPerHungerEffect;
        [SerializeField] private int _hungerPerEffect;
        [SerializeField] private int _ticksPerBoredomEffect;
        [SerializeField] private int _boredomPerEffect;
        [SerializeField] private int _ticksPerFatigueEffect;
        [SerializeField] private int _fatiguePerEffect;

        [Header("Positive Names")]
        [SerializeField] private string _hungerPositive = "Food";
        [SerializeField] private string _fatiugePositive = "Stamina";
        [SerializeField] private string _boredomPositive = "Activity";
        [SerializeField] private string _ignorancePositive = "Knowledge";

        void Awake()
        {
            if (_instance)
            {
                Destroy(gameObject);
                return;
            }

            _instance = this;
        }

        public static string GetPositiveStatName(WellbeingStatType type)
        {
            switch (type)
            {
                case WellbeingStatType.Hunger:
                    return _instance._hungerPositive;
                case WellbeingStatType.Boredom:
                    return _instance._boredomPositive;
                case WellbeingStatType.Fatigue:
                    return _instance._fatiugePositive;
                case WellbeingStatType.Ignorance:
                    return _instance._ignorancePositive;
                default:
                    return $"{type}";
            }
        }
    }
}