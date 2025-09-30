using UnityEngine;

[RequireComponent(typeof(Rigidbody2D), typeof(Collider2D))]
public class PlayerController2D : MonoBehaviour
{
    [Header("Move & Jump")]
    public float moveSpeed = 6f;
    public float jumpVelocity = 9f;

    [Header("Ground Check")]
    public Transform groundCheck;          // empty child at feet
    public float groundCheckRadius = 0.08f;
    public LayerMask groundMask;

    [Header("Flight Attack")]
    public float flightImpulse = 12f;      // press Space to start a flight burst
    public float sustainSeconds = 0.8f;    // hold Space to sustain lift
    public AnimationCurve liftCurve = AnimationCurve.EaseInOut(0, 1, 1, 0);
    public float maxSpeed = 10f;

    private Rigidbody2D rb;
    private bool isFlying;
    private float sustainTimer;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    void Update()
    {
        // Horizontal move
        float h = Input.GetAxisRaw("Horizontal");
        rb.velocity = new Vector2(Mathf.Clamp(h * moveSpeed, -maxSpeed, maxSpeed), rb.velocity.y);

        // Jump from ground (optional)
        if (IsGrounded() && Input.GetKeyDown(KeyCode.W))
        {
            rb.velocity = new Vector2(rb.velocity.x, jumpVelocity);
        }

        // Start/hold flight attack (Space)
        if (Input.GetKeyDown(KeyCode.Space))
        {
            EnterFlightAttack();
        }
        if (isFlying && Input.GetKey(KeyCode.Space))
        {
            SustainFlight();
        }

        // Cap vertical speed
        rb.velocity = new Vector2(rb.velocity.x, Mathf.Clamp(rb.velocity.y, -maxSpeed, maxSpeed));
    }

    // ===== Flight attack =====
    public void EnterFlightAttack()
    {
        isFlying = true;
        sustainTimer = 0f;
        rb.AddForce(Vector2.up * flightImpulse, ForceMode2D.Impulse);
    }

    void SustainFlight()
    {
        if (!isFlying) return;
        sustainTimer += Time.deltaTime;
        float t = Mathf.Clamp01(sustainTimer / sustainSeconds);
        float lift = liftCurve.Evaluate(t);
        rb.AddForce(Vector2.up * lift, ForceMode2D.Force);

        if (sustainTimer >= sustainSeconds) isFlying = false;
    }

    public void ExitFlightAttack()
    {
        isFlying = false;
    }

    bool IsGrounded()
    {
        return Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundMask);
    }

    // ===== Collisions =====
    void OnCollisionEnter2D(Collision2D col)
    {
        // Landing on ground ends flight
        if (col.collider.CompareTag("Ground"))
        {
            ExitFlightAttack();
        }

        // Hit enemy: airborne (flying) => enemy disappears; grounded => player disappears
        if (col.collider.CompareTag("Enemy"))
        {
            bool grounded = IsGrounded();
            if (!grounded && isFlying)
            {
                Destroy(col.collider.gameObject);  // enemy disappears
            }
            else if (grounded)
            {
                Destroy(gameObject);               // player disappears
            }
        }
    }

    void OnDrawGizmosSelected()
    {
        if (groundCheck)
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireSphere(groundCheck.position, groundCheckRadius);
        }
    }
}