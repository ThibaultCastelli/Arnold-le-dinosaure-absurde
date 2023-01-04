using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FloatingAnimation : MonoBehaviour
{
    [Range(0, 20)] public float animationSpeed = 1f;
    [Range(0, 5)] public float animationStrength = 0.5f;
    
    float _x = 0f;
    float _movement;
    float _startPos;

    public void Start()
    {
        _startPos = transform.position.y;
    }

    private void Update()
    {
        // Move the object using sin function
        _x += Time.deltaTime;
        _movement = Mathf.Sin(_x * animationSpeed) * animationStrength;
        transform.Translate(new Vector3(0, _movement * Time.deltaTime, 0));
    }
}
