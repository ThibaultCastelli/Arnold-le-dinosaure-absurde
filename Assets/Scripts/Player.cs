using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class Player : MonoBehaviour
{
    [Header("Jump")]
    [SerializeField] [Range(0f, 10f)] float _jumpForce = 3f;
    [SerializeField] [Range(1f, 10f)] float fallMultiplier = 2.5f;
    [SerializeField] [Range(1f, 10f)] float lowJumpFallMultiplier = 2f;

    [Header("Collision")]
    [SerializeField] LayerMask _groundLayerMask;
    [SerializeField] [Range(0, 5f)] float boxCastOffset = 1f;

    private Rigidbody2D _rb;
    private BoxCollider2D _collider;
    private Animator _animator;
    private PlayerInputActions _inputs;

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
