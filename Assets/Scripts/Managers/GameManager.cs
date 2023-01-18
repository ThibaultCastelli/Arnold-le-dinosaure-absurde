using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class GameManager : MonoBehaviour
{
    [SerializeField] GameObject cactusSpawner;
    [SerializeField] GameObject mountainSpawner;

    private void Awake()
    {
        cactusSpawner.SetActive(false);
    }

    private void OnEnable()
    {
        Events.OnFirstDialoguePass += ActivateCactusSpawner;
        Events.OnGamePause += Pause;
        Events.OnGameOver += GameOver;
        Events.OnGameRestart += Restart;
    }

    private void OnDisable()
    {
        Events.OnFirstDialoguePass -= ActivateCactusSpawner;
        Events.OnGamePause -= Pause;
        Events.OnGameOver -= GameOver;
        Events.OnGameRestart -= Restart;
    }

    /// <summary>
    /// Pause the game
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
        cactusSpawner.GetComponent<Spawner>().StartSpawning();
    }

    /// <summary>
    /// Deactivate cactus and mountain spawner.
    /// </summary>
    private void GameOver()
    {
        cactusSpawner.GetComponent<Spawner>().StopSpawning();
        mountainSpawner.GetComponent<Spawner>().StopSpawning();
    }

    /// <summary>
    /// Re-activate mountain and cactus spawner.
    /// </summary>
    private void Restart()
    {
        mountainSpawner.GetComponent<Spawner>().StartSpawning();
        cactusSpawner.GetComponent<Spawner>().DespawnAll();
    }
}
