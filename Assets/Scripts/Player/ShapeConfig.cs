using UnityEngine;

public enum FlightPathType
{
    Straight,      // constant-velocity straight line (no gravity during flight)
    Parabola,      // projectile arc (uses gravity)
    AngleDown45    // constant-velocity at -45° forward (no gravity during flight)
}

[CreateAssetMenu(menuName = "Configs/Shape")]
public class ShapeConfig : ScriptableObject
{
    [Header("Look")]
    public string shapeName = "Triangle";
    public Sprite sprite;
    public Vector3 localScale = Vector3.one;

    [Header("Collider (Box/Capsule)")]
    public Vector2 colliderSize = new Vector2(0.9f, 1.0f);
    public Vector2 colliderOffset = Vector2.zero;

    [Header("Movement")]
    public float moveSpeed = 6f;
    public float airControlMultiplier = 0.6f;  // weaker control in air
    public float jumpVelocity = 9f;
    public float gravityScale = 3f;

    // (Kept for compatibility with your current controller; can still be used)
    [Header("Flight (Space) - Legacy Burst")]
    public float flightImpulse = 12f;      // burst at key down
    public float sustainSeconds = 0.8f;    // keep holding to sustain
    public AnimationCurve liftCurve = AnimationCurve.EaseInOut(0, 1, 1, 0);
    public float maxSpeed = 12f;           // vertical cap


    // Add near the bottom of the class
[Header("Bullet")]
public Sprite bulletSprite;                 // optional per-shape look
public float bulletStraightSpeed = 12f;     // used for Straight & AngleDown45
public float bulletLaunchSpeed = 12f;       // used for Parabola
public float bulletLifetime = 3f;           // seconds before auto-destroy
public bool bulletZeroGravityDuringStraight = true; // 0 gravity for Straight/AngleDown45

    // NEW: per-shape flight path settings
    [Header("Flight Path")]
    public FlightPathType flightPath = FlightPathType.Parabola;

    [Tooltip("For Straight and AngleDown45 paths: constant speed while flying.")]
    public float straightSpeed = 10f;

    [Tooltip("For Parabola: initial launch speed (m/s).")]
    public float launchSpeed = 10f;

    [Tooltip("For Parabola: launch angle (0=flat forward, 90=straight up).")]
    [Range(0f, 90f)] public float launchAngleDegrees = 60f;

    [Tooltip("If true, gravity is set to 0 during Straight/AngleDown45 flight and restored after.")]
    public bool zeroGravityDuringStraight = true;
}