using Mono.Cecil;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Spawner))]
public class CactusSpawner : MonoBehaviour
{
    [Header("Spawn")]
    [SerializeField] [Range(0.1f, 10f)] float _timeBetweenAcceleration = 3;
    [SerializeField] [Range(0f, 5f)] float _strengthAcceleration = 0.2f;
    [SerializeField] [Range(0.1f, 10f)] float _maxSpawnSpeed = 0.5f;

    [Header("Cactus")]
    [SerializeField][Range(0f, 5f)] float _strengthAccelerationCactus = 0.2f;
    [SerializeField][Range(0.1f, 20f)] float _maxCactusSpeed = 10f;

    private Spawner spawner;
    private float _timeCount = 0;
    private float _currCactusSpeed;

    /// <summary>
    /// Current speed of all the cactus.
    /// </summary>
    public float CurrentCactusSpeed { get { return _currCactusSpeed;} }

    private void Awake()
    {
        // Get component
        spawner = GetComponent<Spawner>();
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

    private void Update()
    {
        _timeCount += Time.deltaTime;
        if (_timeCount >= _timeBetweenAcceleration)
        {
            // If not already at full speed of spawn
            if (spawner.TimeBetweenSpawn > _maxSpawnSpeed)
            {
                spawner.AddTimeOfSpawn(-_strengthAcceleration);
            }
            // If not already at full speed of cactus
            if (_currCactusSpeed < _maxCactusSpeed)
            {
                // Trigger the event so that the player, ground, sky, and cactus accelerate
                Events.OnAcceleration?.Invoke(_strengthAccelerationCactus);
            }
            _timeCount = 0;
        }
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
        _currCactusSpeed += _strengthAccelerationCactus;
    }
}
