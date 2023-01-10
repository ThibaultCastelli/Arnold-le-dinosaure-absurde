using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Cactus : MonoBehaviour, ISpawnable
{
    [SerializeField] [Range(0, 10f)] float explosionForce = 2f;
    [SerializeField][Range(0, 10f)] float maxExplosionForce = 7f;
    [SerializeField][Range(0, 100)] int explosionAngleForce = 10;

    private Rigidbody2D _rb;
    private BoxCollider2D _collider;
    private AutoSpeedHorizontal _horizontalMove;

    private Vector2 _explosionDir;

    private void Awake()
    {
        _rb = GetComponent<Rigidbody2D>();
        _collider = GetComponent<BoxCollider2D>();
        _horizontalMove = GetComponent<AutoSpeedHorizontal>();

        Events.OnAcceleration += AddForceExplosion;
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

            // Prevent the cactus to retouch the player and continue to move left
            _collider.isTrigger = true;
            _horizontalMove.Move = false;

            // Apply the force to the cactus
            _rb.AddForce(_explosionDir * explosionForce, ForceMode2D.Impulse);
            _rb.AddTorque(-Random.Range(50, 100) * explosionAngleForce);

            // If the cactus goes up, add gravity scale to look better
            if (_explosionDir.y > 0) 
            {
                _rb.gravityScale = 3;
            }
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.name == "TriggerOutCactus")
        {
            Despawn();
        }
    }

    private void AddForceExplosion(float amount)
    {
        if (explosionForce < maxExplosionForce)
        {
            explosionForce += amount;
        }
    }

    public void Despawn()
    {
        gameObject.SetActive(false);

        // Ask the parent to check if all its children are not active, if so, despawn the parent
        transform.parent.GetComponent<ParentCactus>().CheckDespawn();
    }

    public void Spawn(Vector3 position, Quaternion rotation)
    {
        transform.position = position;
        transform.rotation = rotation;
        gameObject.SetActive(true);

        _collider.isTrigger = false;
        _horizontalMove.Move = true;
        _rb.gravityScale = 1;
    }
}
