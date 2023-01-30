using Assets.Resources.Ancible_Tools.Scripts.System.Combat;
using UnityEngine;

namespace Assets.Resources.Ancible_Tools.Scripts.System
{
    public class ColorFactoryController : MonoBehaviour
    {
        public static Color NegativeStatColor => _instance._negativeStatColor;
        public static Color Experience => _instance._experienceColor;
        public static Color HoveredItem => _instance._hoveredItemColor;
        public static Color BonusStat => _instance._bonusStatColor;
        public static Color LeftSide => _instance._leftSideColor;
        public static Color RighSide => _instance._rightSideColor;
        public static Color Neutral => _instance._neutralColor;
        public static Color HealthBar => _instance._healthBarColor;
        public static Color ManaBar => _instance._manaBarColor;

        private static ColorFactoryController _instance = null;

        [SerializeField] private Color _negativeStatColor = Color.red;
        [SerializeField] private Color _experienceColor = Color.magenta;
        [SerializeField] private Color _hoveredItemColor = Color.yellow;
        [SerializeField] private Color _bonusStatColor = Color.green;
        [SerializeField] private Color _leftSideColor = Color.blue;
        [SerializeField] private Color _rightSideColor = Color.red;
        [SerializeField] private Color _neutralColor = Color.white;
        [SerializeField] private Color _healthBarColor = Color.red;
        [SerializeField] private Color _manaBarColor = Color.blue;

        [Header("Status Effect Colors")]
        [SerializeField] private Color _stunColor = Color.yellow;
        [SerializeField] private Color _silenceColor = Color.blue;
        [SerializeField] private Color _rootColor = Color.yellow;
        [SerializeField] private Color _muteColor = Color.blue;
        [SerializeField] private Color _disarmColor = Color.red;

        void Awake()
        {
            if (_instance)
            {
                Destroy(gameObject);
                return;
            }

            _instance = this;
        }

        public static Color GetColorForStatusEffect(StatusEffectType type)
        {
            switch (type)
            {
                case StatusEffectType.Stun:
                    return _instance._stunColor;
                case StatusEffectType.Silence:
                    return _instance._silenceColor;
                case StatusEffectType.Root:
                    return _instance._rootColor;
                case StatusEffectType.Mute:
                    return _instance._muteColor;
                case StatusEffectType.Disarm:
                    return _instance._disarmColor;
                default:
                    return Color.white;
            }
        }
    }
}