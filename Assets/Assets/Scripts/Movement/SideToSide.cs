using UnityEngine;

/// <summary>Moves the object back-and-forth along a chosen axis.</summary>
[ExecuteAlways]
[RequireComponent(typeof(Rigidbody2D))]
public class SideToSide : MonoBehaviour {
    [Header("Motion")]
    public float speed = 1.0f;
    public Vector3 axis = Vector3.right;
    public float amplitude = 1f;

    [Tooltip("Oscillations per second.")]
    public float frequency = 1f;

    [Header("Phase")]
    [Tooltip("Start offset so multiple copies don’t sync up.")]
    public float phaseOffset = 0f;

    Rigidbody2D rb;
    float baseX;

    void Awake() {
        rb = GetComponent<Rigidbody2D>();
        baseX = transform.position.x;
    }

    void FixedUpdate() {
        Vector2 velocity = rb.linearVelocity;
        velocity.x = Mathf.Sin(Time.time * frequency * 2f * Mathf.PI) * speed;
        rb.linearVelocity = velocity;
    }
}

