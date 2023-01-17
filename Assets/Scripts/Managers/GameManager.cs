using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class GameManager : MonoBehaviour
{
    [SerializeField] GameObject cactusSpawner;

    private void Awake()
    {
        cactusSpawner.SetActive(false);
    }

    private void OnEnable()
    {
        Events.OnFirstDialoguePass += ActivateCactusSpawner;
        Events.OnGamePause += Pause;
    }

    private void OnDisable()
    {
        Events.OnFirstDialoguePass -= ActivateCactusSpawner;
        Events.OnGamePause -= Pause;
    }

    /// <summary>
    /// P�use the game
    /// </summary>
    /// <param name="isPaused">True if the game is paused, otherwise false.</param>
    private void Pause(bool isPaused)
    {
        if (isPaused)
        {
            Time.timeScale = 0;
        }
        else
        {
            Time.timeScale = 1;
        }
    }

    /// <summary>
    /// Activate the spawn of cactus.
    /// </summary>
    private void ActivateCactusSpawner()
    {
        cactusSpawner.SetActive(true);
    }
}