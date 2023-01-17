using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEditor.PlayerSettings;

public static class MyMath
{
    public static float Round(float value, int digits)
    {
        float x = Mathf.Pow(10, digits);
        return Mathf.Round(value * x) / x;
    }

    public static float CustomRange(float currValue, float oldMin, float oldMax, float newMin, float newMax)
    {
        float oldRange = (oldMax - oldMin);
        float newRange = (newMax - newMin);
        return (((currValue - oldMin) * newRange) / oldRange) + newMin;
    }
}
