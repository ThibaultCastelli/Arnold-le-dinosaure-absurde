using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Spawner : MonoBehaviour
{
    [SerializeField] List<GameObject> gameObjects = new List<GameObject>();
    [SerializeField][Range(0.1f, 10)] float timeBetweenSpawn = 1f;
    [SerializeField] bool spawnOnStart = false;
    [SerializeField][Range(1, 50)] int defaultPoolSize = 5;

    private List<GameObject> pool = new List<GameObject>();

    private float _timer = 0f;
    private int _rndIndex;

    private void Awake()
    {
        for(int i = 0; i < defaultPoolSize; i++)
        {
            AddToPool();
        }
    }

    private void Start()
    {
        if(spawnOnStart)
        {
            Spawn();
        }
    }

    private void Update()
    {
        _timer += Time.deltaTime;
        if (_timer >= timeBetweenSpawn)
        {
            _timer = 0f;
            Spawn();
        }
    }

    private GameObject AddToPool()
    {
        _rndIndex = Random.Range(0, gameObjects.Count);

        ISpawnable check = gameObjects[_rndIndex].GetComponent<ISpawnable>();
        if (check != null)
        {
            GameObject clone = Instantiate(gameObjects[_rndIndex], transform);
            clone.SetActive(false);
            pool.Add(clone);
            return clone;
        }
        else
        {
            Debug.LogError("The objects in the spawner must implement the ISpawnable interface.");
        }

        return null;
    }

    public void Spawn()
    {
        for(int i = 0; i < pool.Count; i++)
        {
            if (!pool[i].activeInHierarchy)
            {
                pool[i].GetComponent<ISpawnable>().Spawn(transform.position, transform.rotation);
                return;
            }
        }

        GameObject newSpawn = AddToPool();
        newSpawn.GetComponent<ISpawnable>().Spawn(transform.position, transform.rotation);
    }
}
