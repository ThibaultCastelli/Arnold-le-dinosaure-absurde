using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FollowTarget : MonoBehaviour
{
    [SerializeField] GameObject _target;
    [SerializeField] bool _freezeX;
    [SerializeField] bool _freezeY;

    Vector2 _originalPos;
    float _xPos;
    float _yPos;

    private void Start()
    {
        _originalPos = transform.position;
    }

    private void Update()
    {
        _xPos = _freezeX ? _originalPos.x : _target.transform.position.x;
        _yPos = _freezeY ? _originalPos.y : _target.transform.position.y;
        transform.position = new Vector2(_xPos, _yPos);
    }
}
