using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AutoHorizontalScrolling : MonoBehaviour
{
    [SerializeField][Range(0, 50)] float scrollSpeed = 1f;
    [SerializeField] HorizontalDirection scrollDirection;
    [SerializeField] GameObject spriteClone;

    private Vector3 _dir;
    private float _startPos;
    private float _spriteWidth;

    private float _deltaPos;
    
    void Awake()
    {
        // Get scroll direction
        switch(scrollDirection)
        {
            case HorizontalDirection.Left:
                _dir = Vector3.left;
                break;
            case HorizontalDirection.Right:
                _dir = Vector3.right;
                break;
        }

        // Get start position
        _startPos = transform.position.x;

        // Get sprite width
        SpriteRenderer spriteBlock = transform.GetChild(0).GetComponent<SpriteRenderer>();
        int spriteCount = transform.childCount;
        _spriteWidth = spriteBlock.bounds.size.x * spriteCount;

        // Duplicate the sprite
        Instantiate(spriteClone, new Vector3(_spriteWidth, transform.position.y, transform.position.z), transform.rotation, transform);
    }

    void Update()
    {
        // Move
        transform.position += _dir * scrollSpeed * Time.deltaTime;
        
        // Reset position
        _deltaPos = Mathf.Abs(_startPos - gameObject.transform.position.x);
        if (_deltaPos >= _spriteWidth)
        {
            transform.position = new Vector3(_startPos, transform.position.y, transform.position.z);
        }
    }

}
