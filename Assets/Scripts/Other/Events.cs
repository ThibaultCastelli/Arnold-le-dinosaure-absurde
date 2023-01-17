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

    /// <summary>
    /// Event triggered when the camera needs to shake.
    /// </summary>
    public static Action<int, float, float> OnCamShake;

    /// <summary>
    /// Event triggered when the first dialogue is passed.
    /// </summary>
    public static Action OnFirstDialoguePass;

    /// <summary>
    /// Event triggered when the game is set to pause or unpause;
    /// </summary>
    public static Action<bool> OnGamePause;

    /// <summary>
    /// Event triggered when the global volume is changed in the menu.
    /// </summary>
    public static Action<float> OnVolumeChange;
}
