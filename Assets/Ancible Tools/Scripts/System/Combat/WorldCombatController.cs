using UnityEngine;

namespace Assets.Resources.Ancible_Tools.Scripts.System.Combat
{
    public class WorldCombatController : MonoBehaviour
    {
        public static int MaxGeneticRoll => _instance._maxGeneticRoll;

        private static WorldCombatController _instance = null;

        [SerializeField] private float _physicalDamagePerStrength = 1f;
        [SerializeField] private float _moveSpeedPerAgility = 1f;
        [SerializeField] private float _attackSpeedPerAgility = 1f;
        [SerializeField] private float _castSpeedPerAgility = 1f;
        [SerializeField] private float _magicDamagePerMagic = 1f;
        [SerializeField] private float _physicalResistancePerArmor = 1f;
        [SerializeField] private float _magicalResistancePerFaith = 1f;
        [SerializeField] private float _magicalHealPerFaith = 1f;
        [SerializeField] private int _maxGeneticRoll = 32;

        void Awake()
        {
            if (_instance)
            {
                Destroy(gameObject);
                return;
            }

            _instance = this;
        }

        public static int CalculateDamage(CombatStats stats, DamageType type)
        {
            var bonus = 0;
            switch (type)
            {
                case DamageType.Physical:
                    bonus = (int) (_instance._physicalDamagePerStrength * stats.Strength);
                    break;
                case DamageType.Magical:
                    bonus = (int)(_instance._magicDamagePerMagic * stats.Magic);
                    break;
            }

            return bonus;
        }

        public static int CalculateResist(CombatStats stats, int damage, DamageType type)
        {
            var resistedAmount = 0;
            switch (type)
            {
                case DamageType.Physical:
                    resistedAmount = (int)(stats.Defense * _instance._physicalResistancePerArmor);
                    break;
                case DamageType.Magical:
                    resistedAmount = (int)(stats.Faith * _instance._magicalResistancePerFaith);
                    break;
            }

            return Mathf.Min(resistedAmount, damage);
        }

        public static float CalculateMoveSpeed(CombatStats stats)
        {
            return stats.Agility * _instance._moveSpeedPerAgility / TickController.OneSecond;
        }

        public static float CalculateAttackSpeed(CombatStats stats)
        {
            return stats.Agility * _instance._attackSpeedPerAgility / TickController.OneSecond;
        }

        public static float CalculateCastSpeed(CombatStats stats)
        {
            return (stats.Agility * _instance._castSpeedPerAgility) / TickController.OneSecond;
        }

        public static int CalculateHeal(CombatStats stats, DamageType type)
        {
            switch (type)
            {
                case DamageType.Physical:
                    return 0;
                case DamageType.Magical:
                    return Mathf.RoundToInt(stats.Faith * _instance._magicalHealPerFaith);
                default:
                    return 0;
            }
        }

    }
}