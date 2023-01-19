using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Android;
using UnityEngine.InputSystem;
using UnityEngine.Rendering;

public class Player : MonoBehaviour
{
    [Header("Jump")]
    [SerializeField] [Range(0f, 100f)] float _jumpForce = 3f;
    [SerializeField] [Range(1f, 100f)] float fallMultiplier = 2.5f;
    [SerializeField] [Range(1f, 100f)] float lowJumpFallMultiplier = 2f;

    [Header("Collision Ground")]
    [SerializeField] LayerMask _groundLayerMask;
    [SerializeField] [Range(0, 5f)] float boxCastOffset = 1f;

    [Header("Collision Cactus")]
    [SerializeField][Range(0, 3f)] float timeFreezeCollision = 0.5f;
    [SerializeField][Range(0, 10f)] float speedMoveBack = 0.5f;

    [Header("Triggers")]
    [SerializeField] GameObject deathTrigger;

    [Header("Particles")]
    [SerializeField] ParticleSystem runParticles;
    [SerializeField] ParticleSystem jumpParticles;
    [SerializeField] ParticleSystem landingParticles;

    private Rigidbody2D _rb;
    private BoxCollider2D _collider;
    private Animator _animator;
    private PlayerInputActions _inputs;
    private AutoSpeedHorizontal _horizontalComponent;

    private ParticleSystem.EmissionModule _emissionRun;

    private bool _wasInAir = true;
    private float _runSoundSpeed = 0.3f;
    private float _startRunSoundSpeed = 0.3f;

    private void Awake()
    {
        // Get other components
        _rb = GetComponent<Rigidbody2D>();
        _collider = GetComponent<BoxCollider2D>();
        _animator = GetComponent<Animator>();
        _horizontalComponent = GetComponent<AutoSpeedHorizontal>();
        _horizontalComponent.enabled = false;

        // Particles
        _emissionRun = runParticles.emission;
        _emissionRun.enabled = false;
    }

    private void Start()
    {
        // Initialize inputs
        _inputs = InputManager.Instance.Inputs;
        _inputs.Player.Jump.performed += Jump;
    }

    private void OnEnable()
    {
        // Subscribe to event
        Events.OnAcceleration += AccelerateRunAnimation;
        Events.OnGameStart += StartGame;
        Events.OnGameRestart += Restart;
        Events.OnGameEnding += EndGame;
    }

    private void OnDisable()
    {
        // Unsubscribe to event
        Events.OnAcceleration -= AccelerateRunAnimation;
        Events.OnGameStart -= StartGame;
        Events.OnGameRestart -= Restart;
        Events.OnGameEnding -= EndGame;
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
            SoundManager.Instance.StopSfxControlLoop("run");
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
            landingParticles.Play();
            SoundManager.Instance.PlaySfx("landing", transform.position);
            SoundManager.Instance.PlaySfxControlLoop("run", _runSoundSpeed, transform);
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
            _rb.gravityScale = 2;
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        // Collision with trigger game over
        if (collision.name == "TriggerGameOver")
        {
            Events.OnGameOver?.Invoke();

            // Prevent moving and go out of screen
            _horizontalComponent.enabled = false;
            StopAllCoroutines();
            StartCoroutine(GameOverFreeze());

            // Desactivate death trigger to be able to reach start point on restart
            deathTrigger.SetActive(false);

            // Low pass the music
            SoundManager.Instance.MusicLowPass(true);
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        // Collision with a cactus
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
    /// Reset animation and sound of running to default values at the end of the game.
    /// </summary>
    private void EndGame()
    {
        _animator.speed = 1;
        _runSoundSpeed = _startRunSoundSpeed;

        StopAllCoroutines();
        _inputs.Player.Disable();
        deathTrigger.gameObject.SetActive(false);

        _inputs.Player.Jump.performed -= Jump;

        SoundManager.Instance.StopSfxControlLoop("run");
        SoundManager.Instance.PlaySfxControlLoop("run", _runSoundSpeed, transform);
    }

    /// <summary>
    /// Triggered with an event, setup the player to start the game.
    /// </summary>
    public void StartGame()
    {
        // Start moving horizontally
        _horizontalComponent.enabled = true;
        // Enable player controls after the out animation of main menu
        StartCoroutine(StartGameCoroutine());
    }
    private IEnumerator StartGameCoroutine()
    {
        yield return new WaitForSeconds(1);
        _inputs.Player.Enable();
    }

    /// <summary>
    /// Reset properties to restart the game.
    /// </summary>
    private void Restart()
    {
        // Move the player to start position
        LeanTween.moveX(gameObject, 0, 1f);

        // Enable jumping and stop death animation
        _animator.SetBool("Dead", false);
        _inputs.Player.Enable();
        SoundManager.Instance.PlaySfxControlLoop("run", _runSoundSpeed, transform);

        // Re-enable rigidbody
        _rb.isKinematic = false;

        // Re-enable auto horizontal move
        _horizontalComponent.enabled = true;

        // Reset death trigger after passing it
        StartCoroutine(ResetDeathTrigger());
    }
    private IEnumerator ResetDeathTrigger()
    {
        yield return new WaitForSeconds(1.5f);
        deathTrigger.SetActive(true);
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
    /// Remove the player from the screen when gameover.
    /// </summary>
    private IEnumerator GameOverFreeze()
    {
        _animator.SetBool("Dead", true);
        _inputs.Player.Disable();
        SoundManager.Instance.StopSfxControlLoop("run");

        // "Disable" the rigidbody while freeze and shaking
        _rb.isKinematic = true;
        _rb.velocity = Vector2.zero;

        while(transform.position.x > -5)
        {
            // Move the player to the left
            transform.Translate(Vector2.left * speedMoveBack * Time.deltaTime);
            yield return null;
        }
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
        SoundManager.Instance.StopSfxControlLoop("run");

        // "Disable" the rigidbody while freeze and shaking
        _rb.isKinematic = true;
        _rb.velocity = Vector2.zero;

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
        SoundManager.Instance.PlaySfxControlLoop("run", _runSoundSpeed, transform);

        // Re-enable rigidbody
        _rb.isKinematic = false;
    }

    /// <summary>
    /// Trigger with an event, accelerate the running animation.
    /// </summary>
    /// <param name="osef">Not used.</param>
    private void AccelerateRunAnimation(float osef)
    {
        _animator.speed += 0.1f;

        _runSoundSpeed -= 0.02f;
        if (_runSoundSpeed < 0.08f)
            _runSoundSpeed = 0.08f;
        SoundManager.Instance.StopFadeSfx("run", 0.05f);
        SoundManager.Instance.PlaySfxControlLoop("run", _runSoundSpeed, transform);
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
            // Reset gravity scale and velocity on y-axis
            _rb.gravityScale = 1;
            _rb.velocity = new Vector2(_rb.velocity.x, 0);

            // Jump
            _rb.AddForce(Vector2.up * _jumpForce, ForceMode2D.Impulse);

            // Play particles and shake the cam and play jump sound
            jumpParticles.Play();
            Events.OnCamShake?.Invoke(1, 0.05f, 0.05f);
            SoundManager.Instance.PlaySfx("Jump", transform.position);
        }
    }

    //private void OnDrawGizmos()
    //{
    //    if (_collider != null)
    //    {
    //        if (IsOnGround())
    //            Gizmos.color = Color.red;
    //        else
    //            Gizmos.color = Color.green;
    //        Gizmos.DrawWireCube(_collider.bounds.center, new Vector3(_collider.size.x, _collider.size.y + boxCastOffset));
    //    }
    //}
}
