// Patrols between leftPoint and rightPoint with a small wait at edges.
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class EnemyPatrolWaypoints : MonoBehaviour
{
    public Transform leftPoint;
    public Transform rightPoint;
    public float speed = 2f;
    public float waitAtEdge = 0.2f;

    private Rigidbody2D rb;
    private int dir = 1; // 1 = right, -1 = left
    private float waitTimer;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.gravityScale = 0f;
        rb.constraints = RigidbodyConstraints2D.FreezeRotation;
    }

    void Update()
    {
        if (!leftPoint || !rightPoint) return;

        if (waitTimer > 0f)
        {
            waitTimer -= Time.deltaTime;
            rb.velocity = new Vector2(0f, 0f);
            return;
        }

        float targetX = dir > 0 ? rightPoint.position.x : leftPoint.position.x;
        float delta = targetX - transform.position.x;

        if (Mathf.Abs(delta) < 0.05f)
        {
            dir *= -1;
            waitTimer = waitAtEdge;
            FlipSprite();
            return;
        }

        rb.velocity = new Vector2(Mathf.Sign(delta) * speed, 0f);
    }

    void FlipSprite()
    {
        var sr = GetComponent<SpriteRenderer>();
        if (sr) sr.flipX = dir < 0;
    }

    void OnDrawGizmosSelected()
    {
        if (leftPoint && rightPoint)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawLine(leftPoint.position, rightPoint.position);
        }
    }
}