using Assets.Resources.Ancible_Tools.Scripts.System.Combat;
using Assets.Resources.Ancible_Tools.Scripts.System.Items;
using UnityEngine;

namespace Assets.Resources.Ancible_Tools.Scripts.System
{
    public class ColorFactoryController : MonoBehaviour
    {
        public static Color NegativeStatColor => _instance._negativeStatColor;
        public static Color Experience => _instance._experienceColor;
        public static Color HoveredItem => _instance._hoveredItemColor;
        public static Color SelectedItem => _instance._selectedItemColor;
        public static Color BonusStat => _instance._bonusStatColor;
        public static Color LeftSide => _instance._leftSideColor;
        public static Color RighSide => _instance._rightSideColor;
        public static Color Neutral => _instance._neutralColor;
        public static Color HealthBar => _instance._healthBarColor;
        public static Color ManaBar => _instance._manaBarColor;
        public static Color ManaText => _instance._manaTextColor;
        public static Color DefaultAlertText => _instance._defaultAlertTextColor;
        public static Color ErrorAlertText => _instance._errorAlertTextColor;
        public static Color CommonItemRarity => _instance._commonColor;
        public static Color UnCommonItemRarity => _instance._uncommonColor;
        public static Color RareItemRarity => _instance._rareColor;
        public static Color EpicItemRarity => _instance._epicColor;
        public static Color LegendaryItemRarity => _instance._legendaryColor;
        public static Color AncientItemRarity => _instance._ancientColor;
        public static Color Happiness => _instance._happinessColor;
        public static Color Moderate => _instance._moderateColor;
        public static Color Unhappiness => _instance._unhappinessColor;
        public static Color BasicQuality => _instance._basicColor;
        public static Color ImprovedQuality => _instance._improvedColor;
        public static Color OrnateQuality => _instance._ornateColor;
        public static Color AbilityRank => _instance._abilityRankColor;

        private static ColorFactoryController _instance = null;

        [SerializeField] private Color _negativeStatColor = Color.red;
        [SerializeField] private Color _experienceColor = Color.magenta;
        [SerializeField] private Color _hoveredItemColor = Color.yellow;
        [SerializeField] private Color _selectedItemColor = Color.green;
        [SerializeField] private Color _bonusStatColor = Color.green;
        [SerializeField] private Color _leftSideColor = Color.blue;
        [SerializeField] private Color _rightSideColor = Color.red;
        [SerializeField] private Color _neutralColor = Color.white;
        [SerializeField] private Color _healthBarColor = Color.red;
        [SerializeField] private Color _manaBarColor = Color.blue;
        [SerializeField] private Color _manaTextColor = Color.blue;
        [SerializeField] private Color _defaultAlertTextColor = Color.yellow;
        [SerializeField] private Color _errorAlertTextColor = Color.red;
        [SerializeField] private Color _happinessColor = Color.green;
        [SerializeField] private Color _moderateColor = Color.yellow;
        [SerializeField] private Color _unhappinessColor = Color.red;
        [SerializeField] private Color _abilityRankColor = Color.yellow;
        

        [Header("Status Effect Colors")]
        [SerializeField] private Color _stunColor = Color.yellow;
        [SerializeField] private Color _silenceColor = Color.blue;
        [SerializeField] private Color _rootColor = Color.yellow;
        [SerializeField] private Color _muteColor = Color.blue;
        [SerializeField] private Color _disarmColor = Color.red;

        [Header("Item Rarity Colors")]
        [SerializeField] private Color _commonColor = Color.white;
        [SerializeField] private Color _uncommonColor = Color.green;
        [SerializeField] private Color _rareColor = Color.blue;
        [SerializeField] private Color _epicColor = Color.red;
        [SerializeField] private Color _legendaryColor = Color.yellow;
        [SerializeField] private Color _ancientColor = Color.yellow;

        [Header("Item Quality Colors")]
        [SerializeField] private Color _basicColor = Color.white;
        [SerializeField] private Color _improvedColor = Color.blue;
        [SerializeField] private Color _ornateColor = Color.yellow;

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

        public static Color GetColorForItemQuality(ItemQuality quality)
        {
            switch (quality)
            {
                case ItemQuality.Improved:
                    return ImprovedQuality;
                case ItemQuality.Ornate:
                    return OrnateQuality;
                default:
                    return Color.white;
            }
        }
    }
}