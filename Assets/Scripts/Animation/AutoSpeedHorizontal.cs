using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AutoSpeedHorizontal : MonoBehaviour
{
    [SerializeField] [Range(0, 50)] float _speed = 1f;
    [SerializeField] HorizontalDirection _direction;

    private Vector3 _dir;

    /// <summary>
    /// Horizontal speed.
    /// </summary>
    public float Speed
    {
        get { return _speed; }
        set
        {
            if (value < 0)
                _speed = 0;
            else
                _speed = value;
        }
    }

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
