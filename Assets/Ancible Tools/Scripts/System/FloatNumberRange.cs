using System;
using Random = UnityEngine.Random;

namespace Assets.Resources.Ancible_Tools.Scripts.System
{
    [Serializable]
    public struct FloatNumberRange
    {
        public static FloatNumberRange Zero => new FloatNumberRange(0f, 0f);

        public float Minimum;
        public float Maximum;

        public FloatNumberRange(float min, float max)
        {
            Minimum = min;
            Maximum = max;
        }


        public override string ToString()
        {
            if (Math.Abs(Maximum - Minimum) < .01f)
            {
                return $"{Maximum:N}";
            }
            return $"{Minimum:N}-{Maximum:N}";

        }
    }
}