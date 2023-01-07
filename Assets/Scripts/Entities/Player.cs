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
    [SerializeField][Range(0, 10f)] float speedMoveBack = 0.5f;

    [Header("Particles")]
    [SerializeField] ParticleSystem runParticles;
    [SerializeField] ParticleSystem jumpParticles;

    private Rigidbody2D _rb;
    private BoxCollider2D _collider;
    private Animator _animator;
    private PlayerInputActions _inputs;

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
        if(IsOnGround() && !_animator.GetBool("Dead"))
        {
            _emissionRun.enabled = true;
        }
        else
        {
            _emissionRun.enabled = false;
        }

        // When touch the ground after a jump
        if (IsOnGround() && _wasInAir)
        {
            jumpParticles.Play();
            Events.OnCamShake?.Invoke(1, 0.05f, 0.05f);

            _wasInAir = false;
        }

        // Track if was on air last frame
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

    private void OnCollisionEnter2D(Collision2D collision)
    {
        Cactus cactus = collision.gameObject.GetComponent<Cactus>();
        if (cactus != null)
        {
            Events.OnCactusHit?.Invoke();

            StopAllCoroutines();
            StartCoroutine(FreezeDeath());
            StartCoroutine(ShakeCoroutine(4, 0.1f, 0.05f));
        }
    }

    /// <summary>
    /// Shake the player when he hits a cactus.
    /// </summary>
    /// <param name="shakeIterations">Number of shakes.</param>
    /// <param name="shakeStrength">Strength of each shake.</param>
    /// <param name="shakeIntervals">Time between each shake.</param>
    /// <returns></returns>
    private IEnumerator ShakeCoroutine(int shakeIterations, float shakeStrength, float shakeIntervals)
    {
        // Initialize variables
        Vector3 originalPos = transform.position;
        float shakeCount = 0;
        float xShake;
        float yShake;

        while (shakeCount < shakeIterations)
        {
            // Wait a frame on original position
            yield return null;

            // Translate to a random position
            xShake = Random.Range(-shakeStrength, shakeStrength);
            yShake = Random.Range(-shakeStrength, shakeStrength);
            transform.Translate(new Vector3(xShake, yShake, transform.position.z));

            yield return new WaitForSeconds(shakeIntervals);

            // Reset position
            transform.position = originalPos;

            yield return new WaitForSeconds(shakeIntervals);

            shakeCount++;
            // Actualize originalPos
            originalPos = transform.position;
        }

        // Reset position
        transform.position = originalPos;
    }

    /// <summary>
    /// Prevent the player from jumping when touch by a cactus for a given time.
    /// </summary>
    private IEnumerator FreezeDeath()
    {
        // Start the timer, prevent player from jumping and play death animation
        float timeCount = 0;
        _animator.SetBool("Dead", true);
        _inputs.Player.Disable();

        while (timeCount < timeFreezeCollision)
        {
            // Move the player to the left
            transform.Translate(Vector2.left * speedMoveBack * Time.deltaTime);
            yield return null;
            timeCount += Time.deltaTime;
        }

        // Enable jumping and stop death animation
        _animator.SetBool("Dead", false);
        _inputs.Player.Enable();
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
            // Jump
            _rb.AddForce(Vector2.up * _jumpForce, ForceMode2D.Impulse);

            // Play particles and shake the cam
            jumpParticles.Play();
            Events.OnCamShake?.Invoke(1, 0.05f, 0.05f);
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
