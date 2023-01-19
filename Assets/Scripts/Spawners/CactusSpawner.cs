using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Spawner))]
public class CactusSpawner : MonoBehaviour
{
    [Header("Spawn")]
    [SerializeField] [Range(0f, 5f)] float _strengthAcceleration = 0.2f;

    private Spawner _spawner;

    private float _currCactusSpeed;

    /// <summary>
    /// Current speed of all the cactus.
    /// </summary>
    public float CurrentCactusSpeed { get { return _currCactusSpeed;} }

    private void Awake()
    {
        // Get component
        _spawner = GetComponent<Spawner>();
    }

    private void OnEnable()
    {
        // Subscribe to event
        Events.OnAcceleration += AccelerateCactus;
    }

    private void OnDisable()
    {
        // Unsubscribe to event
        Events.OnAcceleration -= AccelerateCactus;
    }

    private void Start()
    {
        // Get the basic speed of the cactus
        _currCactusSpeed = transform.GetChild(0).GetChild(0).GetComponent<AutoSpeedHorizontal>().Speed;
    }

    /// <summary>
    /// Triggered by event, add speed to all the cactus.
    /// </summary>
    /// <param name="amount">Amount added to the speed of cactus.</param>
    private void AccelerateCactus(float amount)
    {
        // For each child (parent cactus)
        for (int i = 0; i < transform.childCount; i++)
        {
            Transform parentCactus = transform.GetChild(i);

            // Set the speed to the current speed and add the amount to each child cactus
            for (int j = 0; j < parentCactus.childCount; j++)
            {
                AutoSpeedHorizontal scroll = parentCactus.GetChild(j).GetComponent<AutoSpeedHorizontal>();
                scroll.Speed = _currCactusSpeed;
                scroll.Speed += amount;
            }
        }
        _currCactusSpeed += amount;

        _spawner.AddTimeOfSpawn(-_strengthAcceleration);
    }
}
