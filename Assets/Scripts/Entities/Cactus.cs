using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Cactus : MonoBehaviour, ISpawnable
{
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.name == "TriggerOutCactus")
        {
            Despawn();
        }
    }

    public void Despawn()
    {
        gameObject.SetActive(false);
    }

    public void Spawn(Vector3 position, Quaternion rotation)
    {
        transform.position = position;
        transform.rotation = rotation;
        gameObject.SetActive(true);
    }
}
