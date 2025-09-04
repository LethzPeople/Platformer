using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    // Components
    private Rigidbody2D rb;
    private Animator animator;
    private SpriteRenderer spriteRenderer; // New component to flip the sprite
    private bool isGrounded;
    private bool isChargingJump;

    // Movement configuration
    [SerializeField] private float moveSpeed = 1f;
    [SerializeField] private float minJumpForce = 5f;
    [SerializeField] private float maxJumpForce = 15f;
    [SerializeField] private float chargeSpeed = 7.5f;
    [SerializeField] private float groundCheckRadius = 0.2f;

    private float currentJumpForce;
    private float horizontalDirection;
    private Vector2 jumpDirection;

    // Gravity configuration
    [SerializeField] private float gravityScale = 1f;
    [SerializeField] private float fastFallGravityMult = 2f;
    [SerializeField] private float jumpCutGravityMult = 1.5f;
    [SerializeField] private float fallGravityMult = 1.2f;
    [SerializeField] private float maxFastFallSpeed = -10f;
    [SerializeField] private float maxFallSpeed = -7f;

    // Ground check
    [SerializeField] private LayerMask groundLayer;
    [SerializeField] private Transform groundCheck;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>(); // Initialize SpriteRenderer
        jumpDirection = Vector2.up;
        rb.gravityScale = gravityScale;
    }

    void Update()
    {
        isGrounded = Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundLayer);
        AdjustGravity();

        if (isGrounded && !isChargingJump)
        {
            horizontalDirection = Input.GetAxisRaw("Horizontal");

            // Smooth horizontal movement
            float targetSpeed = horizontalDirection * moveSpeed;
            rb.velocity = new Vector2(Mathf.Lerp(rb.velocity.x, targetSpeed, 0.1f), rb.velocity.y);

            // Flip sprite towards movement direction
            if (horizontalDirection != 0)
            {
                spriteRenderer.flipX = horizontalDirection < 0; // Turn left or right
            }
        }

        if (isChargingJump)
        {
            float inputDirection = Input.GetAxisRaw("Horizontal");
            if (inputDirection != 0)
            {
                float angle = Mathf.Atan2(1, inputDirection) * Mathf.Rad2Deg;
                jumpDirection = Quaternion.Euler(0, 0, angle) * Vector2.right;
            }
        }

        if (isGrounded)
        {
            if (Input.GetKeyDown(KeyCode.Space))
            {
                StartCharging();
            }
            else if (Input.GetKey(KeyCode.Space) && isChargingJump)
            {
                ChargeJump();
            }
            else if (Input.GetKeyUp(KeyCode.Space) && isChargingJump)
            {
                Jump();
            }
        }

        animator.SetFloat("xVelocity", Mathf.Abs(rb.velocity.x));
        animator.SetFloat("yVelocity", rb.velocity.y);
        animator.SetBool("isJumping", !isGrounded);
    }

    private void AdjustGravity()
    {
        if (rb.velocity.y < 0 && Input.GetAxisRaw("Vertical") < 0)
        {
            rb.gravityScale = gravityScale * fastFallGravityMult;
            rb.velocity = new Vector2(rb.velocity.x, Mathf.Max(rb.velocity.y, maxFastFallSpeed));
        }
        else if (isChargingJump && !Input.GetKey(KeyCode.Space))
        {
            rb.gravityScale = gravityScale * jumpCutGravityMult;
            rb.velocity = new Vector2(rb.velocity.x, Mathf.Max(rb.velocity.y, maxFallSpeed));
        }
        else if ((isChargingJump || isGrounded) && Mathf.Abs(rb.velocity.y) < 0.1f)
        {
            rb.gravityScale = gravityScale * 0.8f;
        }
        else if (rb.velocity.y < 0)
        {
            rb.gravityScale = gravityScale * fallGravityMult;
            rb.velocity = new Vector2(rb.velocity.x, Mathf.Max(rb.velocity.y, maxFallSpeed));
        }
        else
        {
            rb.gravityScale = gravityScale;
        }
    }

    private void StartCharging()
    {
        isChargingJump = true;
        currentJumpForce = minJumpForce;
        rb.velocity = Vector2.zero;
    }

    private void ChargeJump()
    {
        currentJumpForce = Mathf.Min(currentJumpForce + chargeSpeed * Time.deltaTime, maxJumpForce);
    }

    private void Jump()
    {
        isChargingJump = false;
        rb.velocity = jumpDirection * currentJumpForce;
    }

    void OnDrawGizmos()
    {
        if (groundCheck != null)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(groundCheck.position, groundCheckRadius);
        }
    }
}

