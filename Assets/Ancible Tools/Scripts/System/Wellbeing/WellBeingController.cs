using UnityEngine;

namespace Assets.Ancible_Tools.Scripts.System.Wellbeing
{
    public class WellBeingController : MonoBehaviour
    {
        public const string Happiness = "Happiness";

        public static int TicksPerHungerEffect => _instance._ticksPerHungerEffect;
        public static int HungerPerEffect => _instance._hungerPerEffect;
        public static int TicksPerBoredomEffect => _instance._ticksPerBoredomEffect;
        public static int BoredomPerEffect => _instance._boredomPerEffect;
        public static int TicksPerFatigueEffect => _instance._ticksPerFatigueEffect;
        public static int FatiguePerEffect => _instance._fatiguePerEffect;

        public static float WarningPercent => _instance._wellbeingWarningPerecent;

        public static float IgnorancePerExperience => _instance._ignorancePerExperience;

        public static Sprite HappinessIcon => _instance._happinessIcon;

        private static WellBeingController _instance = null;

        [SerializeField] [Range(0f, 1f)] private float _wellbeingWarningPerecent = .25f;
        [SerializeField] private float _ignorancePerExperience = 1f;

        [Header("Happiness")]
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