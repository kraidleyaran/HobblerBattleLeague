using System;
using UnityEngine;

namespace Assets.Ancible_Tools.Scripts.System.Wellbeing
{
    [Serializable]
    public struct WellbeingStats
    {
        public static WellbeingStats Zero => new WellbeingStats();

        public float Hunger;
        public float Boredom;
        public float Fatigue;
        public float Ignorance;

        public void ApplyMinimum()
        {
            Hunger = Mathf.Max(Hunger, 0);
            Boredom = Mathf.Max(Boredom, 0);
            Fatigue = Mathf.Max(Fatigue, 0);
            Ignorance = Mathf.Max(Ignorance, 0);
        }

        public void ApplyMaximum(WellbeingStats max)
        {
            Hunger = Mathf.Min(Hunger, max.Hunger);
            Boredom = Mathf.Min(Boredom, max.Boredom);
            Fatigue = Mathf.Min(Fatigue, max.Fatigue);
            Ignorance = Mathf.Min(Ignorance, max.Ignorance);
        }

        public void ApplyLimits(WellbeingStats max)
        {
            ApplyMinimum();
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