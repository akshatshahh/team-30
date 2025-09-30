using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(Collider2D))]
public class Projectile2D : MonoBehaviour
{
    Rigidbody2D rb;
    SpriteRenderer sr;
    Collider2D col;

    Vector2 constantVel;
    bool maintainConstantVel;
    float timeLeft;

    [Header("Despawn")]
    public float extraKillSpeedClamp = 50f; // safety clamp
    public bool destroyOnGround = true;

    [Header("Hit Filters")]
    public LayerMask groundMask;  // <-- set this to Ground in the prefab
    public string enemyTag = "Enemy"; // keep using your Enemy tag
    public string groundTag = "Ground"; // optional, works with tag too

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        sr = GetComponent<SpriteRenderer>();
        col = GetComponent<Collider2D>();

        // We use trigger-based hits for reliability at high speed
        if (col) col.isTrigger = true;

        // Prevent tunneling
        rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
        rb.interpolation = RigidbodyInterpolation2D.Interpolate;
    }

    public void Init(ShapeConfig cfg, int dirX)
    {
        // Sprite (per-shape look)
        if (sr && cfg.bulletSprite) sr.sprite = cfg.bulletSprite;

        timeLeft = Mathf.Max(0.1f, cfg.bulletLifetime);

        switch (cfg.flightPath)
        {
            case FlightPathType.Straight:
                if (cfg.bulletZeroGravityDuringStraight) rb.gravityScale = 0f;
                constantVel = new Vector2(dirX * cfg.bulletStraightSpeed, 0f);
                rb.velocity = constantVel;
                maintainConstantVel = true;
                break;

            case FlightPathType.Parabola:
            {
                rb.gravityScale = cfg.gravityScale; // arc uses gravity
                float ang = cfg.launchAngleDegrees * Mathf.Deg2Rad;
                float vx = Mathf.Cos(ang) * cfg.bulletLaunchSpeed * dirX;
                float vy = Mathf.Sin(ang) * cfg.bulletLaunchSpeed;
                rb.velocity = new Vector2(vx, vy);
                maintainConstantVel = false;
                break;
            }

            case FlightPathType.AngleDown45:
            default:
                if (cfg.bulletZeroGravityDuringStraight) rb.gravityScale = 0f;
                const float c = 0.70710677f; // √2/2
                constantVel = new Vector2(dirX * c * cfg.bulletStraightSpeed, -c * cfg.bulletStraightSpeed);
                rb.velocity = constantVel;
                maintainConstantVel = true;
                break;
        }

        // Rotate sprite to flight direction (optional)
        if (sr && rb.velocity.sqrMagnitude > 0.001f)
        {
            float angDeg = Mathf.Atan2(rb.velocity.y, rb.velocity.x) * Mathf.Rad2Deg;
            transform.rotation = Quaternion.AngleAxis(angDeg, Vector3.forward);
        }
    }

    void FixedUpdate()
    {
        // keep straight-path bullets locked to constant velocity
        if (maintainConstantVel) rb.velocity = constantVel;

        // Safety (don’t let errant physics blow it up)
        rb.velocity = Vector2.ClampMagnitude(rb.velocity, extraKillSpeedClamp);

        timeLeft -= Time.fixedDeltaTime;
        if (timeLeft <= 0f) Destroy(gameObject);
    }

    // Trigger-based hits (works if either collider is trigger)
    void OnTriggerEnter2D(Collider2D other)
    {
        // Enemy?
        if (other.CompareTag(enemyTag))
        {
            Destroy(other.gameObject);
            Destroy(gameObject);
            return;
        }

        // Ground by layer
        if (((1 << other.gameObject.layer) & groundMask) != 0)
        {
            if (destroyOnGround) Destroy(gameObject);
            return;
        }

        // Ground by tag (in case you use tag instead of layer)
        if (other.CompareTag(groundTag))
        {
            if (destroyOnGround) Destroy(gameObject);
        }
    }
}