using System;
using PhotoMode;

namespace PhotoMode
{
    // Custom serializable class
    [Serializable]
    public class MinMax
    {
        public float min = 0.0f;
        public float max = 1.0f;

        public MinMax(float minValue, float maxValue)
        {
            min = minValue;
            max = maxValue;
        }
    }
}