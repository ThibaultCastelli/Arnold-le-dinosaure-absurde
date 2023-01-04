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
    private int _rndIndex;      // Used to get a random GameObject from the list of GameObject

    private void Awake()
    {
        for(int i = 0; i < _defaultPoolSize; i++)
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

    private void Update()
    {
        _timer += Time.deltaTime;
        if (_timer >= _timeBetweenSpawn)
        {
            _timer = 0f;
            Spawn();
        }
    }

    /// <summary>
    /// Add a random GameObject from the list gameObjects to the pool.
    /// </summary>
    /// <returns>The new added GameObject.</returns>
    private GameObject AddToPool()
    {
        _rndIndex = Random.Range(0, _gameObjects.Count);

        // The object to add must implement the interface ISpawnable
        ISpawnable check = _gameObjects[_rndIndex].GetComponent<ISpawnable>();
        if (check != null)
        {
            // Create a new instance of that GameObject and desactivate it
            GameObject clone = Instantiate(_gameObjects[_rndIndex], transform);
            clone.SetActive(false);
            _pool.Add(clone);
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
}