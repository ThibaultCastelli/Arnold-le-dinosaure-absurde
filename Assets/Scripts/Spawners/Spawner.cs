using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Spawner : MonoBehaviour
{
    [SerializeField][Range(1, 50)] int _defaultPoolSize = 5;
    [SerializeField][Range(0.1f, 10)] float _timeBetweenSpawn = 1f;
    [SerializeField] bool _spawnOnStart = false;
    [SerializeField] List<GameObject> _gameObjects = new List<GameObject>();

    private List<GameObject> _pool = new List<GameObject>();
    private float _timer = 0f;  // Used to spawn at regular intervals
    private int _rndIndex = -1;      // Used to get a random GameObject from the list of GameObject

    private List<GameObject> _objectsToAdd; // Used to spawn each gameobject at least one

    private bool _activateSpawn = true;

    /// <summary>
    /// Time in seconds between each spawn.
    /// </summary>
    public float TimeBetweenSpawn { get { return _timeBetweenSpawn; } }

    private void Awake()
    {
        // Copy the game objects to the temp list
        _objectsToAdd = new List<GameObject>(_gameObjects);

        for (int i = 0; i < _defaultPoolSize; i++)
        {
            AddToPool();
        }
    }

    private void Start()
    {
        if(_spawnOnStart)
        {
            Spawn();
        }
    }

    private void OnEnable()
    {
        Events.OnGameEnding += StopSpawning;
    }
    private void OnDisable()
    {
        Events.OnGameEnding -= StopSpawning;
    }

    private void Update()
    {
        _timer += Time.deltaTime;
        if (_timer >= _timeBetweenSpawn && _activateSpawn)
        {
            _timer = 0f;
            Spawn();
        }
    }

    public void StartSpawning()
    {
        _activateSpawn = true;
    }

    public void StopSpawning()
    {
        _activateSpawn = false;
    }

    /// <summary>
    /// Add or subtract time between spawns.
    /// </summary>
    /// <param name="addTime">The time to add or subtract to the time between spawns.</param>
    public void AddTimeOfSpawn(float addTime)
    {
        _timeBetweenSpawn += addTime;

        if (_timeBetweenSpawn <= 0)
            _timeBetweenSpawn = 0.1f;
    }

    /// <summary>
    /// Add a random GameObject from the list gameObjects to the pool.
    /// </summary>
    /// <returns>The new added GameObject.</returns>
    private GameObject AddToPool()
    {
        // If all the objects have been spawn one time, reset the list of objects to spawn
        if (_objectsToAdd.Count == 0)
        {
            _objectsToAdd = new List<GameObject>(_gameObjects);
        }

        // Get random object
        _rndIndex = Random.Range(0, _objectsToAdd.Count);

        // The object to add must implement the interface ISpawnable
        ISpawnable check = _objectsToAdd[_rndIndex].GetComponent<ISpawnable>();
        if (check != null)
        {
            // Create a new instance of that GameObject and desactivate it
            GameObject clone = Instantiate(_objectsToAdd[_rndIndex], transform);
            clone.SetActive(false);
            _pool.Add(clone);

            // Remove the spawned object from the list
            _objectsToAdd.RemoveAt(_rndIndex);

            return clone;
        }
        else
        {
            Debug.LogError("The objects in the spawner must implement the ISpawnable interface.");
        }

        return null;
    }

    /// <summary>
    /// Activate the first GameObject that is inactive in the pool.
    /// </summary>
    public void Spawn()
    {
        // Prevent errors if the pool is empty
        if (_pool.Count == 0)
        {
            Debug.LogError($"The pool of {gameObject.name} must contains at least one GameObject.");
            return;
        }

        for(int i = 0; i < _pool.Count; i++)
        {
            if (!_pool[i].activeInHierarchy)
            {
                _pool[i].GetComponent<ISpawnable>().Spawn(transform.position, transform.rotation);
                return;
            }
        }

        // If all the GameObject are active in the pool, add a new one to the pool and activate it
        GameObject newSpawn = AddToPool();
        newSpawn.GetComponent<ISpawnable>().Spawn(transform.position, transform.rotation);
    }

    /// <summary>
    /// Despawn all game objects in the pool.
    /// </summary>
    public void DespawnAll()
    {
        foreach(GameObject obj in _pool)
        {
            obj.GetComponent<ISpawnable>().Despawn();
        }
    }
}
