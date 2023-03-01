using System;
using UnityEngine;

namespace Assets.Resources.Ancible_Tools.Scripts.System.Combat
{
    [Serializable]
    public struct CombatStats
    {
        public static readonly CombatStats Zero = new CombatStats(0);

        private const string HEALTH = "Max Health";
        private const string MANA = "Max Mana";
        private const string STRENGTH = "Strength";
        private const string AGILITY = "Agility";
        private const string MAGIC = "Magic";
        private const string FAITH = "Faith";
        private const string DEFENSE = "Defense";
        private const string SPIRIT = "Spirit";

        public int Health;
        public int Mana;
        public int Strength;
        public int Agility;
        public int Magic;
        public int Faith;
        public int Defense;
        public int Spirit;

        public CombatStats(int allStats)
        {
            Health = allStats;
            Mana = allStats;
            Strength = allStats;
            Agility = allStats;
            Magic = allStats;
            Faith = allStats;
            Defense = allStats;
            Spirit = allStats;
        }

        public string GetDescription(bool unitDescription = false, bool equipment = false)
        {
            var description = string.Empty;
            description = ApplyStatValueToDescription(description, HEALTH, Health, unitDescription, equipment);
            description = ApplyStatValueToDescription(description, MANA, Mana, unitDescription, equipment);
            description = ApplyStatValueToDescription(description, STRENGTH, Strength, unitDescription, equipment);
            description = ApplyStatValueToDescription(description, AGILITY, Agility, unitDescription, equipment);
            description = ApplyStatValueToDescription(description, MAGIC, Magic, unitDescription, equipment);
            description = ApplyStatValueToDescription(description, FAITH, Faith, unitDescription, equipment);
            description = ApplyStatValueToDescription(description, DEFENSE, Defense, unitDescription, equipment);
            description = ApplyStatValueToDescription(description, SPIRIT, Spirit, unitDescription, equipment);
            return description;
        }

        public string GetRosterDescriptions(CombatStats bonus)
        {
            var description = string.Empty;
            description = $"{ApplyStatValueToDescription(description, HEALTH, Health, true, false)}{StaticMethods.ApplyColorToText(GetStat(bonus.Health), bonus.Health >= 0 ? ColorFactoryController.BonusStat : ColorFactoryController.NegativeStatColor)}";
            description = $"{ApplyStatValueToDescription(description, MANA, Mana, true, false)}{StaticMethods.ApplyColorToText(GetStat(bonus.Mana), bonus.Mana >= 0 ? ColorFactoryController.BonusStat : ColorFactoryController.NegativeStatColor)}";
            description = $"{ApplyStatValueToDescription(description, STRENGTH, Strength, true, false)}{StaticMethods.ApplyColorToText(GetStat(bonus.Strength), bonus.Strength >= 0 ? ColorFactoryController.BonusStat : ColorFactoryController.NegativeStatColor)}";
            description = $"{ApplyStatValueToDescription(description, AGILITY, Agility, true, false)}{StaticMethods.ApplyColorToText(GetStat(bonus.Agility), bonus.Agility >= 0 ? ColorFactoryController.BonusStat : ColorFactoryController.NegativeStatColor)}";
            description = $"{ApplyStatValueToDescription(description, MAGIC, Magic, true, false)}{StaticMethods.ApplyColorToText(GetStat(bonus.Magic), bonus.Magic >= 0 ? ColorFactoryController.BonusStat : ColorFactoryController.NegativeStatColor)}";
            description = $"{ApplyStatValueToDescription(description, FAITH, Faith, true, false)}{StaticMethods.ApplyColorToText(GetStat(bonus.Faith), bonus.Faith >= 0 ? ColorFactoryController.BonusStat : ColorFactoryController.NegativeStatColor)}";
            description = $"{ApplyStatValueToDescription(description, DEFENSE, Defense, true, false)}{StaticMethods.ApplyColorToText(GetStat(bonus.Defense), bonus.Defense >= 0 ? ColorFactoryController.BonusStat : ColorFactoryController.NegativeStatColor)}";
            description = $"{ApplyStatValueToDescription(description, SPIRIT, Spirit, true, false)}{StaticMethods.ApplyColorToText(GetStat(bonus.Spirit), bonus.Spirit >= 0 ? ColorFactoryController.BonusStat : ColorFactoryController.NegativeStatColor)}";
            return description;
        }

        private string GetStat(int stat)
        {
            if (stat > 0)
            {
                return $"+{stat}";
            }

            if (stat < 0)
            {
                return $"{stat}";
            }

            return string.Empty;
        }

        private string ApplyStatValueToDescription(string description, string stat, int value, bool unitDescription, bool equipment)
        {
            if (unitDescription)
            {
                description = string.IsNullOrWhiteSpace(description) ? $"{stat}:{value}" : $"{description}{Environment.NewLine}{stat}:{value}";
            }
            else if (value > 0 || value < 0)
            {
                if (equipment)
                {
                    description = string.IsNullOrWhiteSpace(description) ? $"{GetStat(value)} {stat}" : $"{description}{Environment.NewLine}{GetStat(value)} {stat}";
                }
                else
                {
                    description = string.IsNullOrEmpty(description) ? $"{GetStat(value)} {stat}" : $"{description}, {GetStat(value)} {stat}";
                }
            }
            return description;

        }

        public static CombatStats operator +(CombatStats a) => a;

        public static CombatStats operator +(CombatStats a, CombatStats b)
        {
            return new CombatStats
            {
                Health = a.Health + b.Health,
                Mana = a.Mana + b.Mana,
                Strength = a.Strength + b.Strength,
                Agility = a.Agility + b.Agility,
                Magic = a.Magic + b.Magic,
                Faith = a.Faith + b.Faith,
                Defense = a.Defense + b.Defense,
                Spirit = a.Spirit + b.Spirit
            };
        }

        public static CombatStats operator -(CombatStats a)
        {
            return new CombatStats
            {
                Health = a.Health * -1,
                Mana = a.Mana * 1,
                Strength = a.Strength * -1,
                Agility = a.Agility * -1,
                Magic = a.Magic * -1,
                Faith = a.Faith * -1,
                Defense = a.Defense * -1,
                Spirit = a.Spirit * -1
            };
        }

        public static CombatStats operator -(CombatStats a, CombatStats b)
        {
            return new CombatStats
            {
                Health = a.Health - b.Health,
                Mana = a.Mana - b.Mana,
                Strength = a.Strength - b.Strength,
                Agility = a.Agility - b.Agility,
                Magic = a.Magic - b.Magic,
                Faith = a.Faith - b.Faith,
                Defense = a.Defense - b.Defense,
                Spirit = a.Spirit - b.Spirit
            };
        }

        public static CombatStats operator *(CombatStats a, float multiplier)
        {
            return new CombatStats
            {
                Health = (int)(a.Health * multiplier),
                Mana = (int)(a.Mana * multiplier),
                Strength = (int)(a.Strength * multiplier),
                Agility = (int)(a.Agility * multiplier),
                Magic = (int)(a.Magic * multiplier),
                Faith = (int)(a.Faith * multiplier),
                Defense = (int)(a.Defense * multiplier),
                Spirit = (int)(a.Spirit * multiplier),

            };
        }

        public static CombatStats operator /(CombatStats a, float multiplier)
        {
            return new CombatStats
            {
                Health = (int)(a.Health / multiplier),
                Mana = (int)(a.Mana / multiplier),
                Strength = (int)(a.Strength / multiplier),
                Agility = (int)(a.Agility / multiplier),
                Magic = (int)(a.Magic / multiplier),
                Faith = (int)(a.Faith / multiplier),
                Defense = (int)(a.Defense / multiplier),
                Spirit = (int)(a.Spirit / multiplier),
            };
        }

        public static CombatStats operator +(CombatStats a, GeneticCombatStats g)
        {
            return new CombatStats
            {
                Health = (int) (a.Health + g.Health),
                Mana = (int) (a.Mana + g.Mana),
                Strength = (int) (a.Strength + g.Strength),
                Agility = (int)(a.Agility + g.Agility),
                Magic = (int) (a.Magic + g.Magic),
                Faith = (int) (a.Faith + g.Faith),
                Defense = (int) (a.Defense + g.Armor),
                Spirit = (int) (a.Spirit + g.Spirit)
            };
        }
    }
}