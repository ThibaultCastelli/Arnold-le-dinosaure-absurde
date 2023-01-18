using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AutoSpeedHorizontal : MonoBehaviour
{
    [SerializeField] [Range(0, 50)] float _speed = 1f;
    [SerializeField] HorizontalDirection _direction;
    [SerializeField] bool continueMovingAfterGameOver = false;

    private Vector3 _dir;
    private Rigidbody2D _rb;

    private bool _move = true;

    private float _defaultSpeed;

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

    /// <summary>
    /// Enable or disable the horizontal movement.
    /// </summary>
    public bool Move
    {
        get { return _move; }
        set { _move = value; }
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

        _rb = GetComponent<Rigidbody2D>();

        _defaultSpeed = _speed;
    }

    private void OnEnable()
    {
        if (!continueMovingAfterGameOver)
        {
            Events.OnGameOver += StopSpeed;
            Events.OnGameRestart += ResetSpeed;
        }
    }

    private void OnDisable()
    {
        if (!continueMovingAfterGameOver)
        {
            Events.OnGameOver -= StopSpeed;
            Events.OnGameRestart += ResetSpeed;
        }
    }

    private void Update()
    {
        // Move the transform if there is no rigidbody on the game object
        if (_move && _rb == null)
        {
            transform.Translate(_dir * _speed * Time.deltaTime);
        }
    }

    private void FixedUpdate()
    {
        // Move the rigidbody if there is one on the game object
        if(_move && _rb != null)
        {
            _rb.position += new Vector2(_dir.x, _dir.y) * _speed * Time.fixedDeltaTime;
        }
    }

    /// <summary>
    /// Slowly stop the horizontal movement.
    /// </summary>
    private void StopSpeed()
    {
        LeanTween.value(_speed, 0, 1).setOnUpdate((float value) => _speed = value);
    }

    /// <summary>
    /// Restart the horizontal movement.
    /// </summary>
    private void ResetSpeed()
    {
        _speed = _defaultSpeed;
    }
}
