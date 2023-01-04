using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Clouds : MonoBehaviour, ISpawnable
{
    [SerializeField][Range(0f, 5f)] float minSpeed = 0.3f;
    [SerializeField][Range(0f, 5)] float maxSpeed = 1.5f;

    [SerializeField][Range(0f, 5f)] float minAnimationSpeed = 0.4f;
    [SerializeField][Range(0f, 5)] float maxAnimationSpeed = 1.5f;

    [SerializeField][Range(0f, 1f)] float minAnimationStrength = 0.1f;
    [SerializeField][Range(0f, 1)] float maxAnimationStrength = 0.6f;

    void Start()
    {
        FloatingAnimation anim = GetComponent<FloatingAnimation>();
        anim.animationSpeed = Random.Range(minAnimationSpeed, maxAnimationSpeed);
        anim.animationStrength = Random.Range(minAnimationStrength, maxAnimationStrength);

        AutoSpeedHorizontal autoSpeed = GetComponent<AutoSpeedHorizontal>();
        autoSpeed.speed = Random.Range(minSpeed, maxSpeed);
    }

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
