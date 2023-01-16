using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FloatingAnimation : MonoBehaviour
{
    [Range(0, 20)] public float animationSpeed = 1f;
    [Range(0, 50)] public float animationStrength = 0.5f;
    
    float _x = 0f;
    float _movement;

    private RectTransform rectTransform;

    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
    }

    private void Update()
    {
        // Move the object using sin function
        _x += Time.deltaTime;
        _movement = Mathf.Sin(_x * animationSpeed) * animationStrength;

        if (rectTransform != null)
        {
            rectTransform.position += new Vector3(0, _movement * Time.deltaTime, 0);
        }
        else
        {
            transform.Translate(new Vector3(0, _movement * Time.deltaTime, 0));
        }
    }
}
