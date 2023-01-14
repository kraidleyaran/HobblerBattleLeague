using System;

namespace Assets.Resources.Ancible_Tools.Scripts.System
{
    [Serializable]
    public struct IntNumberRange
    {
        public static IntNumberRange One => new IntNumberRange {Minimum = 1, Maximum = 1};

        public int Minimum;
        public int Maximum;

        public override string ToString()
        {
            return $"{Minimum}-{Maximum}";
        }
    }
}