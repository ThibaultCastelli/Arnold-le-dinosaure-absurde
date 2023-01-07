using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class MyMath
{
    public static float Round(float value, int digits)
    {
        float x = Mathf.Pow(10, digits);
        return Mathf.Round(value * x) / x;
    }
}
