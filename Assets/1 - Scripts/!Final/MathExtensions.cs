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
    
    public static float LegacyMaxValue (float[] floatArray)
    {
        var max = 0f;
        for (int i = 0; i < floatArray.Length; i++) 
        {
            if (floatArray[i] > max) 
            {
                max = floatArray[i];
            }
            else if (floatArray[i] * -1 > max)
            {
                max = floatArray[i];
            }
        }
        //Debug.Log($"MAX: {max}");
        return max;
    }

    public static float LegacyAverageValue(float[] floatArray)
    {
        var sum = 0f;
        for (int i = 0; i < floatArray.Length; i++)
        {
            sum += floatArray[i];
        }
        //Debug.Log(sum / floatArray.Length);
        return sum / floatArray.Length;
    }
}
