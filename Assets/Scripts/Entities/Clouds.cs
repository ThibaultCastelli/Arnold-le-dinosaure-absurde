using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Clouds : MonoBehaviour, ISpawnable
{
    [SerializeField][Range(0f, 5f)] float _minSpeed = 0.3f;
    [SerializeField][Range(0f, 5)] float _maxSpeed = 1.5f;

    [SerializeField][Range(0f, 5f)] float _minAnimationSpeed = 0.4f;
    [SerializeField][Range(0f, 5)] float _maxAnimationSpeed = 1.5f;

    [SerializeField][Range(0f, 1f)] float _minAnimationStrength = 0.1f;
    [SerializeField][Range(0f, 1)] float _maxAnimationStrength = 0.6f;

    void Start()
    {
        // Set random floating animation values
        FloatingAnimation anim = GetComponent<FloatingAnimation>();
        anim.animationSpeed = Random.Range(_minAnimationSpeed, _maxAnimationSpeed);
        anim.animationStrength = Random.Range(_minAnimationStrength, _maxAnimationStrength);

        // Set random speed
        AutoSpeedHorizontal autoSpeed = GetComponent<AutoSpeedHorizontal>();
        autoSpeed.Speed = Random.Range(_minSpeed, _maxSpeed);
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
