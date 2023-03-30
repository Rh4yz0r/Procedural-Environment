using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public static class MathExtensions
{
    public static float MaxValue (float[] floatArray)
    {
        return floatArray.Prepend(0f).Max();
    }

    public static float AverageValue(float[] floatArray)
    {
        float sum = floatArray.Sum();
        return sum / floatArray.Length;
    }
}
