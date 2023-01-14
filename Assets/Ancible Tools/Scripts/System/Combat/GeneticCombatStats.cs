using System;

namespace Assets.Resources.Ancible_Tools.Scripts.System.Combat
{
    [Serializable]
    public struct GeneticCombatStats
    {
        public static readonly GeneticCombatStats Zero = new GeneticCombatStats(0f);

        public float Health;
        public float Mana;
        public float Strength;
        public float Agility;
        public float Magic;
        public float Faith;
        public float Armor;
        public float Spirit;

        public GeneticCombatStats(float allStats)
        {
            Health = allStats;
            Mana = allStats;
            Strength = allStats;
            Agility = allStats;
            Magic = allStats;
            Faith = allStats;
            Armor = allStats;
            Spirit = allStats;
        }

        public static GeneticCombatStats operator +(GeneticCombatStats a) => a;

        public static GeneticCombatStats operator +(GeneticCombatStats a, GeneticCombatStats b)
        {
            return new GeneticCombatStats
            {
                Health = a.Health + b.Health,
                Mana = a.Mana + b.Mana,
                Strength = a.Strength + b.Strength,
                Agility = a.Agility + b.Agility,
                Magic = a.Magic + b.Magic,
                Faith = a.Faith + b.Faith,
                Armor = a.Armor + b.Armor,
                Spirit = a.Spirit + b.Spirit
            };
        }

        public static GeneticCombatStats operator -(GeneticCombatStats a)
        {
            return new GeneticCombatStats
            {
                Health = a.Health * -1,
                Mana = a.Mana * 1,
                Strength = a.Strength * -1,
                Agility = a.Agility * -1,
                Magic = a.Magic * -1,
                Faith = a.Faith * -1,
                Armor = a.Armor * -1,
                Spirit = a.Spirit * -1
            };
        }

        public static GeneticCombatStats operator -(GeneticCombatStats a, GeneticCombatStats b)
        {
            return new GeneticCombatStats
            {
                Health = a.Health - b.Health,
                Mana = a.Mana - b.Mana,
                Strength = a.Strength - b.Strength,
                Agility = a.Agility - b.Agility,
                Magic = a.Magic - b.Magic,
                Faith = a.Faith - b.Faith,
                Armor = a.Armor - b.Armor,
                Spirit = a.Spirit - b.Spirit
            };
        }

        public static GeneticCombatStats operator *(GeneticCombatStats a, float multiplier)
        {
            return new GeneticCombatStats
            {
                Health = (int)(a.Health * multiplier),
                Mana = (int)(a.Mana * multiplier),
                Strength = (int)(a.Strength * multiplier),
                Agility = (int)(a.Agility * multiplier),
                Magic = (int)(a.Magic * multiplier),
                Faith = (int)(a.Faith * multiplier),
                Armor = (int)(a.Armor * multiplier),
                Spirit = (int)(a.Spirit * multiplier),

            };
        }

        public static GeneticCombatStats operator /(GeneticCombatStats a, float multiplier)
        {
            return new GeneticCombatStats
            {
                Health = (int)(a.Health / multiplier),
                Mana = (int)(a.Mana / multiplier),
                Strength = (int)(a.Strength / multiplier),
                Agility = (int)(a.Agility / multiplier),
                Magic = (int)(a.Magic / multiplier),
                Faith = (int)(a.Faith / multiplier),
                Armor = (int)(a.Armor / multiplier),
                Spirit = (int)(a.Spirit / multiplier),
            };
        }
    }
}