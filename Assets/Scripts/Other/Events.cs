using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Events
{
    /// <summary>
    /// Event triggered when the speed of the cactus is increased.
    /// </summary>
    public static Action<float> OnAcceleration;

    /// <summary>
    /// Event triggered when the player hits a cactus.
    /// </summary>
    public static Action OnCactusHit;
}
