using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AutoHorizontalScrolling : MonoBehaviour
{
    [SerializeField][Range(0, 50)] float _scrollSpeed = 1f;
    [SerializeField] HorizontalDirection _scrollDirection;
    [SerializeField] bool _accelerate = true;

    private Vector3 _dir;

    private float _startPos; 
    private float _deltaPos;

    private float _spriteWidth;

    private float _defaultSpeed;
    private float _startSpeed;
    
    void Awake()
    {
        // Get scroll direction
        switch(_scrollDirection)
        {
            case HorizontalDirection.Left:
                _dir = Vector3.left;
                break;
            case HorizontalDirection.Right:
                _dir = Vector3.right;
                break;
        }

        _defaultSpeed = _scrollSpeed;
        _startSpeed = _scrollSpeed;

        // Get start position
        _startPos = transform.position.x;

        // Get sprite width
        SpriteRenderer spriteBlock = transform.GetChild(0).GetComponent<SpriteRenderer>();
        int spriteCount = transform.childCount;
        _spriteWidth = spriteBlock.bounds.size.x * spriteCount;

        // Duplicate the sprite
        GameObject clone = new GameObject("Clone");
        clone.transform.position = new Vector3(_spriteWidth, transform.position.y, transform.position.z);
        clone.transform.parent = transform;
        // Populate with sprites
        for(int i = 0; i < transform.childCount; i++)
        {
            Instantiate(transform.GetChild(i), clone.transform);
        }
    }

    private void OnEnable()
    {
        // Subscribe to event
        if (_accelerate)
        {
            Events.OnAcceleration += Accelerate;
        }

        Events.OnGameOver += StopScrolling;
        Events.OnGameRestart += ResetScrolling;
        Events.OnGameEnding += DefaultScrollingSpeed;
    }

    private void OnDisable()
    {
        // Unsubscribe to event
        if (_accelerate)
        {
            Events.OnAcceleration -= Accelerate;
        }

        Events.OnGameOver -= StopScrolling;
        Events.OnGameRestart -= ResetScrolling;
        Events.OnGameEnding -= DefaultScrollingSpeed;
    }

    void Update()
    {
        // Move
        transform.Translate(_dir * _scrollSpeed * Time.deltaTime);
        
        // Reset position
        _deltaPos = Mathf.Abs(_startPos - gameObject.transform.position.x);
        if (_deltaPos >= _spriteWidth)
        {
            transform.position = new Vector3(_startPos, transform.position.y, transform.position.z);
        }
    }

    private void StopScrolling()
    {
        LeanTween.value(_scrollSpeed, 0, 1).setOnUpdate((float value) => _scrollSpeed = value);
    }

    private void ResetScrolling()
    {
        _scrollSpeed = _defaultSpeed;
    }

    /// <summary>
    /// Triggered by event, accelerate the scrolling speed.
    /// </summary>
    /// <param name="amount"></param>
    private void Accelerate(float amount)
    {
        _scrollSpeed += amount;
        _defaultSpeed += amount;
    }

    private void DefaultScrollingSpeed()
    {
        _scrollSpeed = _startSpeed;
    }
}
