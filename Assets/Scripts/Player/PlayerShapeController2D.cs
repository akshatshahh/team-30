using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(Collider2D))]
public class PlayerShapeController2D : MonoBehaviour
{
    [Header("Shapes (match keys below)")]
    public ShapeConfig[] shapes;      // Drag Triangle/Square/Circle assets here
    public KeyCode[] shapeKeys;       // e.g., Alpha1, Alpha2, Alpha3

    [Header("Ground Check")]
    public Transform groundCheck;     // Child at feet
    public float groundCheckRadius = 0.1f;
    public LayerMask groundMask;      // Must include your Ground layer

    [Header("Shooting")]
    public KeyCode fireKey = KeyCode.F;
    public Transform firePoint;       // optional: child at the muzzle (if null, we use player position)
    public GameObject bulletPrefab;   // prefab with Projectile2D + Rigidbody2D + Collider2D

    [Header("UI")]
    public GameOverUI gameOverUI;     // Drag Canvas (with GameOverUI) here

    Rigidbody2D rb;
    SpriteRenderer sr;
    BoxCollider2D box;
    CapsuleCollider2D capsule;

    int shapeIndex = 0;
    bool isFlying = false;
    float sustainTimer = 0f;

    // Facing + restore gravity for straight-line flights
    int lastDirX = 1;                 // -1 = left, 1 = right
    float gravityBeforeFlight = 0f;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        sr = GetComponent<SpriteRenderer>();
        box = GetComponent<BoxCollider2D>();
        capsule = GetComponent<CapsuleCollider2D>();

        // Auto-create/attach GroundCheck so you never forget it
        if (!groundCheck)
        {
            var found = transform.Find("GroundCheck");
            if (found) groundCheck = found;
            else
            {
                var gc = new GameObject("GroundCheck").transform;
                gc.SetParent(transform);
                gc.localPosition = new Vector3(0f, -0.5f, 0f);
                groundCheck = gc;
            }
        }

        // Cache UI if not assigned (includes inactive objects)
        if (!gameOverUI) gameOverUI = FindObjectOfType<GameOverUI>(true);

        if (shapes != null && shapes.Length > 0) ApplyShape(0);
    }

    void Update()
    {
        var cfg = Current();
        if (cfg == null) return;

        // --- Shape switching (1/2/3 etc) ---
        for (int i = 0; i < shapes.Length && i < shapeKeys.Length; i++)
            if (Input.GetKeyDown(shapeKeys[i])) SwitchTo(i);

        // --- Facing & input ---
        float h = Input.GetAxisRaw("Horizontal");
        if (Mathf.Abs(h) > 0.01f)
        {
            lastDirX = h > 0 ? 1 : -1;
            if (sr) sr.flipX = lastDirX < 0; // optional visual flip
        }

        // --- Movement ---
        bool straightFlight = isFlying &&
                              (cfg.flightPath == FlightPathType.Straight ||
                               cfg.flightPath == FlightPathType.AngleDown45);

        if (!straightFlight) // don't overwrite constant-velocity flights
        {
            float speed = cfg.moveSpeed * (IsGrounded() ? 1f : cfg.airControlMultiplier);
            float targetVX = h * speed;
            rb.velocity = new Vector2(
                Mathf.MoveTowards(rb.velocity.x, targetVX, 100f * Time.deltaTime),
                Mathf.Clamp(rb.velocity.y, -cfg.maxSpeed, cfg.maxSpeed)
            );
        }
        else
        {
            // keep vertical clamped while flying straight
            rb.velocity = new Vector2(rb.velocity.x, Mathf.Clamp(rb.velocity.y, -cfg.maxSpeed, cfg.maxSpeed));
        }

        // --- Jump (W/Up) ---
        if (IsGrounded() && (Input.GetKeyDown(KeyCode.W) || Input.GetKeyDown(KeyCode.UpArrow)))
            rb.velocity = new Vector2(rb.velocity.x, cfg.jumpVelocity);

        // --- Flight (Space) ---
        if (Input.GetKeyDown(KeyCode.Space)) StartFlight(cfg);
        if (isFlying && Input.GetKey(KeyCode.Space)) SustainFlight(cfg);
        if (Input.GetKeyUp(KeyCode.Space)) EndFlight(cfg);

        // --- Shoot (F) ---
        if (Input.GetKeyDown(fireKey)) FireBullet();
    }

    ShapeConfig Current() =>
        (shapes != null && shapes.Length > 0) ? shapes[Mathf.Clamp(shapeIndex, 0, shapes.Length - 1)] : null;

    void SwitchTo(int idx)
    {
        if (idx < 0 || idx >= shapes.Length) return;
        shapeIndex = idx;
        ApplyShape(idx);
    }

    void ApplyShape(int idx)
    {
        var cfg = shapes[idx];

        // gravity + look
        rb.gravityScale = cfg.gravityScale;
        if (sr && cfg.sprite) sr.sprite = cfg.sprite;
        transform.localScale = cfg.localScale;

        // simple collider retune for Box/Capsule
        if (box)     { box.size = cfg.colliderSize;     box.offset = cfg.colliderOffset; }
        if (capsule) { capsule.size = cfg.colliderSize; capsule.offset = cfg.colliderOffset; }
    }

    // ====== Flight control with per-shape paths ======

    void StartFlight(ShapeConfig cfg)
    {
        isFlying = true;
        sustainTimer = 0f;

        switch (cfg.flightPath)
        {
            case FlightPathType.Straight:
                if (cfg.zeroGravityDuringStraight)
                {
                    gravityBeforeFlight = rb.gravityScale;
                    rb.gravityScale = 0f;
                }
                rb.velocity = new Vector2(lastDirX * cfg.straightSpeed, 0f);
                break;

            case FlightPathType.Parabola:
            {
                rb.gravityScale = cfg.gravityScale; // ensure gravity is on
                float angRad = cfg.launchAngleDegrees * Mathf.Deg2Rad;
                float vx = Mathf.Cos(angRad) * cfg.launchSpeed * lastDirX;
                float vy = Mathf.Sin(angRad) * cfg.launchSpeed;
                rb.velocity = new Vector2(vx, vy);
                break;
            }

            case FlightPathType.AngleDown45:
            {
                if (cfg.zeroGravityDuringStraight)
                {
                    gravityBeforeFlight = rb.gravityScale;
                    rb.gravityScale = 0f;
                }
                const float c = 0.70710677f; // sqrt(2)/2
                rb.velocity = new Vector2(lastDirX * c * cfg.straightSpeed, -c * cfg.straightSpeed);
                break;
            }
        }
    }

    void SustainFlight(ShapeConfig cfg)
    {
        sustainTimer += Time.deltaTime;

        switch (cfg.flightPath)
        {
            case FlightPathType.Straight:
                rb.velocity = new Vector2(lastDirX * cfg.straightSpeed, 0f);
                if (sustainTimer >= cfg.sustainSeconds) EndFlight(cfg);
                break;

            case FlightPathType.Parabola:
                if (sustainTimer >= cfg.sustainSeconds) EndFlight(cfg);
                break;

            case FlightPathType.AngleDown45:
            {
                const float c = 0.70710677f;
                rb.velocity = new Vector2(lastDirX * c * cfg.straightSpeed, -c * cfg.straightSpeed);
                if (sustainTimer >= cfg.sustainSeconds) EndFlight(cfg);
                break;
            }
        }
    }

    void EndFlight(ShapeConfig cfg)
    {
        if (!isFlying) return;
        isFlying = false;

        if ((cfg.flightPath == FlightPathType.Straight || cfg.flightPath == FlightPathType.AngleDown45)
            && cfg.zeroGravityDuringStraight)
        {
            rb.gravityScale = gravityBeforeFlight;
        }
    }

    // ====== Shooting ======

    void FireBullet()
    {
        var cfg = Current();
        if (cfg == null || bulletPrefab == null) return;

        Vector3 spawnPos = firePoint ? firePoint.position
                                     : transform.position + new Vector3(0.5f * lastDirX, 0.0f, 0f);
        var go = Instantiate(bulletPrefab, spawnPos, Quaternion.identity);

        // optional: flip bullet sprite
        var bsr = go.GetComponent<SpriteRenderer>();
        if (bsr) bsr.flipX = (lastDirX < 0);

        // initialize trajectory based on current shape
        var proj = go.GetComponent<Projectile2D>();
        if (proj != null) proj.Init(cfg, lastDirX);
        else
        {
            // Fallback: at least push it forward
            var rb2 = go.GetComponent<Rigidbody2D>();
            if (rb2)
            {
                rb2.gravityScale = 0f;
                rb2.velocity = new Vector2(lastDirX * 10f, 0f);
            }
        }
    }

    // ====== Game Over helpers ======

    void KillPlayer(string reason)
    {
        Debug.Log($"[Player] KillPlayer called. reason='{reason}', ui={(gameOverUI ? "OK" : "NULL")}");
        if (gameOverUI) gameOverUI.ShowGameOver(reason);
        else Debug.LogWarning("[Player] No GameOverUI found/assigned. Add GameOverUI to Canvas and assign it on the Player.");

        var col = GetComponent<Collider2D>(); if (col) col.enabled = false;
        if (sr) sr.enabled = false;
        if (rb) rb.simulated = false;
        enabled = false; // stop this controller
    }

    void HandleEnemyHit(GameObject enemyGO)
    {
        bool grounded = IsGrounded();
        bool airborneAttack = !grounded && isFlying;

        Debug.Log($"[Player] HandleEnemyHit grounded={grounded} isFlying={isFlying} -> airborneAttack={airborneAttack}");

        if (airborneAttack)
        {
            if (enemyGO) Destroy(enemyGO); // you win the clash in air
        }
        else
        {
            KillPlayer("Hit an enemy!");
        }
    }

    // Works if the collider you touched is on an enemy child or parent
    bool IsEnemyCollider(Component c, out GameObject enemyRoot)
    {
        enemyRoot = null;
        if (!c) return false;

        var go = c.gameObject;
        if (go.CompareTag("Enemy")) { enemyRoot = go; return true; }

        var root = go.transform.root;
        if (root.CompareTag("Enemy")) { enemyRoot = root.gameObject; return true; }

        // Optional: if your project has EnemyHealth, this also detects it on parents
        var eh = go.GetComponentInParent<EnemyHealth>();
        if (eh) { enemyRoot = eh.gameObject; return true; }

        return false;
    }

    // ====== Grounding & collisions ======

    bool IsGrounded()
    {
        if (!groundCheck) return false;
        return Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundMask);
    }

    void OnCollisionEnter2D(Collision2D col)
    {
        // Landing on Ground layer ends flight
        if (((1 << col.collider.gameObject.layer) & groundMask) != 0)
            EndFlight(Current());

        // Enemy contact (non-trigger)
        if (IsEnemyCollider(col.collider, out var enemyRoot))
        {
            Debug.Log($"[Player] Collision with enemy via {col.collider.name} (root: {enemyRoot.name})");
            HandleEnemyHit(enemyRoot);
        }
    }

    // Enemy contact (trigger enemies)
    void OnTriggerEnter2D(Collider2D other)
    {
        if (IsEnemyCollider(other, out var enemyRoot))
        {
            Debug.Log($"[Player] Trigger with enemy via {other.name} (root: {enemyRoot.name})");
            HandleEnemyHit(enemyRoot);
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