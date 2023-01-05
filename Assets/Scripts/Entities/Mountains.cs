using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Mountains : MonoBehaviour, ISpawnable
{
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.name == "TriggerOutCam")
        {
            Despawn();
        }
    }

    public void Spawn(Vector3 position, Quaternion rotation)
    {
        transform.position = position;
        transform.rotation = rotation;
        gameObject.SetActive(true);
    }

    public void Despawn()
    {
        gameObject.SetActive(false);
    }
}
