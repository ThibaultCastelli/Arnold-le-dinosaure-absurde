using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AutoSpeedHorizontal : MonoBehaviour
{
    [Range(0, 50)] public float _speed = 1f;
    [SerializeField] HorizontalDirection _direction;

    private Vector3 _dir;

    private void Awake()
    {
        switch(_direction)
        {
            case HorizontalDirection.Left:
                _dir = Vector3.left;
                break;
            case HorizontalDirection.Right:
                _dir = Vector3.right;
                break;
        }
    }

    private void Update()
    {
        transform.Translate(_dir * _speed * Time.deltaTime);
    }
}
