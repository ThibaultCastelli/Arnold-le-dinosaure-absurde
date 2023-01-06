using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Android;
using UnityEngine.InputSystem;
using UnityEngine.Rendering;

public class Player : MonoBehaviour
{
    [Header("Jump")]
    [SerializeField] [Range(0f, 10f)] float _jumpForce = 3f;
    [SerializeField] [Range(1f, 10f)] float fallMultiplier = 2.5f;
    [SerializeField] [Range(1f, 10f)] float lowJumpFallMultiplier = 2f;

    [Header("Collision Ground")]
    [SerializeField] LayerMask _groundLayerMask;
    [SerializeField] [Range(0, 5f)] float boxCastOffset = 1f;

    [Header("Collision Cactus")]
    [SerializeField][Range(0, 3f)] float timeFreezeCollision = 0.5f;
    [SerializeField][Range(0, 5f)] float speedMoveBack = 0.5f;

    [Header("Particles")]
    [SerializeField] ParticleSystem runParticles;
    [SerializeField] ParticleSystem jumpParticles;

    private Rigidbody2D _rb;
    private BoxCollider2D _collider;
    private Animator _animator;
    private PlayerInputActions _inputs;

    private bool _inFreeze = false;
    private bool _wasInAir = false;

    private ParticleSystem.EmissionModule _emissionRun;

    private void Awake()
    {
        // Initialize inputs
        _inputs = new PlayerInputActions();
        _inputs.Player.Enable();
        _inputs.Player.Jump.performed += Jump;

        // Get other components
        _rb = GetComponent<Rigidbody2D>();
        _collider = GetComponent<BoxCollider2D>();
        _animator = GetComponent<Animator>();

        // Particles
        _emissionRun = runParticles.emission;
        _emissionRun.enabled = false;

        // Subscribe to event
        Events.OnAcceleration += AccelerateRunAnimation;
    }

    private void Update()
    {
        // Jump animation
        if (IsOnGround())
        {
            _animator.SetBool("InAir", false);
        }
        else
        {
            _animator.SetBool("InAir", true);
        }

        // Particles
        if(IsOnGround() && !_inFreeze)
        {
            _emissionRun.enabled = true;
        }
        else
        {
            _emissionRun.enabled = false;
        }

        if (IsOnGround() && _wasInAir)
        {
            jumpParticles.Play();
            _wasInAir = false;
        }

        _wasInAir = !IsOnGround();
    }

    private void FixedUpdate()
    {
        // If falling
        if (_rb.velocity.y < 0)
        {
            _rb.gravityScale = fallMultiplier;
        }
        // If small jump
        else if (_rb.velocity.y > 0 && !_inputs.Player.Jump.IsPressed())
        {
            _rb.gravityScale = lowJumpFallMultiplier;
        }
        // Standard jump or idle
        else
        {
            _rb.gravityScale = 1;
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        Cactus cactus = collision.gameObject.GetComponent<Cactus>();
        if (cactus != null)
        {
            _animator.SetBool("Dead", true);
            _inputs.Player.Disable();
            Events.OnCactusHit?.Invoke();
            StartCoroutine(FreezeDeath());
        }
    }

    private IEnumerator FreezeDeath()
    {
        float timeCount = 0;
        _inFreeze = true;

        while (timeCount < timeFreezeCollision)
        {
            transform.Translate(Vector2.left * speedMoveBack * Time.deltaTime);
            yield return null;
            timeCount += Time.deltaTime;
        }

        _animator.SetBool("Dead", false);
        _inputs.Player.Enable();
        _inFreeze = false;
    }

    /// <summary>
    /// Trigger with an event, accelerate the running animation.
    /// </summary>
    /// <param name="osef">Not used.</param>
    private void AccelerateRunAnimation(float osef)
    {
        _animator.speed += 0.1f;
    }

    /// <summary>
    /// Return true if the player is on ground, otherwise return false.
    /// </summary>
    private bool IsOnGround()
    {
        RaycastHit2D raycastHit = Physics2D.BoxCast(_collider.bounds.center, _collider.size, 0f, Vector2.down, boxCastOffset, _groundLayerMask);
        return !(raycastHit.collider == null);
    }

    /// <summary>
    /// Make the player jump.
    /// </summary>
    /// <param name="ctx"></param>
    private void Jump(InputAction.CallbackContext ctx)
    {
        if (IsOnGround())
        {
            _rb.AddForce(Vector2.up * _jumpForce, ForceMode2D.Impulse);

            jumpParticles.Play();
        }
    }

    private void OnDrawGizmos()
    {
        if (_collider != null)
        {
            if (IsOnGround())
                Gizmos.color = Color.red;
            else
                Gizmos.color = Color.green;
            Gizmos.DrawWireCube(_collider.bounds.center, new Vector3(_collider.size.x, _collider.size.y + boxCastOffset));
        }
    }
}
