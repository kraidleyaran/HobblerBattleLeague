using System;
using UnityEngine;

namespace Assets.Resources.Ancible_Tools.Scripts.System
{
    [Serializable]
    public struct WellbeingStats
    {
        public static WellbeingStats Zero => new WellbeingStats();

        public int Hunger;
        public int Boredom;
        public int Fatigue;
        public int Ignorance;

        public void ApplyMinimum(WellbeingStats min)
        {
            Hunger = Mathf.Max(Hunger, min.Hunger);
            Boredom = Mathf.Max(Boredom, min.Boredom);
            Fatigue = Mathf.Max(Fatigue, min.Fatigue);
            Ignorance = Mathf.Max(Ignorance, min.Ignorance);
        }

        public void ApplyMaximum(WellbeingStats max)
        {
            Hunger = Mathf.Min(Hunger, max.Hunger);
            Boredom = Mathf.Min(Boredom, max.Boredom);
            Fatigue = Mathf.Min(Fatigue, max.Fatigue);
            Ignorance = Mathf.Min(Ignorance, max.Ignorance);
        }

        public void ApplyLimits(WellbeingStats min, WellbeingStats max)
        {
            ApplyMinimum(min);
            ApplyMaximum(max);
        }

        public static WellbeingStats operator +(WellbeingStats a) => a;

        public static WellbeingStats operator -(WellbeingStats a) => new WellbeingStats
        {
            Hunger = a.Hunger * -1,
            Boredom = a.Boredom * -1,
            Fatigue = a.Fatigue * -1,
            Ignorance = a.Ignorance * -1
        };

        public static WellbeingStats operator +(WellbeingStats a, WellbeingStats b)
        {
            return new WellbeingStats
            {
                Hunger = a.Hunger + b.Hunger,
                Boredom = a.Boredom + b.Boredom,
                Fatigue = a.Fatigue + b.Fatigue,
                Ignorance = a.Ignorance + b.Ignorance
            };
        }

        public static WellbeingStats operator -(WellbeingStats a, WellbeingStats b)
        {
            return new WellbeingStats
            {
                Hunger = a.Hunger - b.Hunger,
                Boredom = a.Boredom - b.Boredom,
                Fatigue = a.Fatigue - b.Fatigue,
                Ignorance = a.Ignorance - b.Ignorance
            };
        }

        public static WellbeingStats operator *(WellbeingStats a, WellbeingStats b)
        {
            return new WellbeingStats
            {
                Hunger = a.Hunger * b.Hunger,
                Boredom = a.Boredom * b.Boredom,
                Fatigue = a.Fatigue * b.Fatigue,
                Ignorance = a.Ignorance * b.Ignorance
            };
        }

        public static WellbeingStats operator /(WellbeingStats a, WellbeingStats b)
        {
            return new WellbeingStats
            {
                Hunger = a.Hunger / b.Hunger,
                Boredom = a.Boredom / b.Boredom,
                Fatigue = a.Fatigue / b.Fatigue,
                Ignorance = a.Ignorance / b.Ignorance
            };
        }

    }
}