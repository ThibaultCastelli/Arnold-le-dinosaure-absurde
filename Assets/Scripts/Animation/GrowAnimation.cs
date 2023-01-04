using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Animations;

public class GrowAnimation : MonoBehaviour
{
    [Range(0, 20)] public float animationSpeed = 0.7f;
    [Range(0, 5)] public float animationStrength = 0.1f;

    float _x = 0f;
    float _movement;
    float _startSize;

    public void Start()
    {
        _startSize = transform.localScale.x;
    }

    private void Update()
    {
        _x += Time.deltaTime;
        _movement = Mathf.Sin(_x * animationSpeed) * animationStrength;
        transform.localScale = new Vector3(_startSize + _movement, _startSize + _movement, _startSize + _movement);
    }
}
