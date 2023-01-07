using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Cactus : MonoBehaviour, ISpawnable
{
    [SerializeField] [Range(0, 10f)] float explosionForce = 5f;

    private Rigidbody2D _rb;
    private BoxCollider2D _collider;
    private AutoSpeedHorizontal _horizontalMove;

    private Vector2 _explosionDir;
    private Vector2 _originExplosion;

    private void Awake()
    {
        _rb = GetComponent<Rigidbody2D>();
        _collider = GetComponent<BoxCollider2D>();
        _horizontalMove = GetComponent<AutoSpeedHorizontal>();
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.collider.name == "TriggerOutCactus")
        {
            Despawn();
        }
        else if (collision.collider.name == "Player")
        {
            // Get direction of the player on Y-axis
            Rigidbody2D rbPlayer = collision.gameObject.GetComponent<Rigidbody2D>();
            float playerVelocityDir = -Mathf.Sign(rbPlayer.velocity.y);

            // Get the direction of the explosion based on the position and direction of the player
            _explosionDir = new Vector2(
                _collider.bounds.center.x - collision.collider.bounds.center.x,
                (_collider.bounds.center.y - collision.collider.bounds.center.y) * playerVelocityDir)
                .normalized;

            // Get a random point of origin for the force to be applied on the cactus
            _originExplosion = new Vector2(
                Random.Range(_collider.bounds.min.x, _collider.bounds.center.x),
                Random.Range(_collider.bounds.center.y, _collider.bounds.max.y));

            // Prevent the cactus to retouch the player and continue to move left
            _collider.isTrigger = true;
            _horizontalMove.Move = false;

            // Apply the force to the cactus
            _rb.AddForceAtPosition(_explosionDir * explosionForce, _originExplosion, ForceMode2D.Impulse);
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.name == "TriggerOutCactus")
        {
            Despawn();
        }
    }

    public void Despawn()
    {
        gameObject.SetActive(false);
    }

    public void Spawn(Vector3 position, Quaternion rotation)
    {
        transform.position = position;
        transform.rotation = rotation;
        gameObject.SetActive(true);

        _collider.isTrigger = false;
        _horizontalMove.Move = true;
    }
}
