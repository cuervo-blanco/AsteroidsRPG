using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(Collider2D))]

public class Bullet : MonoBehaviour {
    public float speed = 10f;
    public float lifetime = 1f;
    public AK.Wwise.Event fireEvent;
    public AK.Wwise.Event getHitEvent;
    public AK.Wwise.Event bulletHit;

    [Header("Bullet Stats")]
    public int damage = 1;

    Rigidbody2D rb;

    void Awake() {
        AkUnitySoundEngine.RegisterGameObj(gameObject);
        rb = GetComponent<Rigidbody2D>();
        rb.gravityScale = 0;
        rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
    }

    public void Initialise(Vector2 dir) {
        rb.linearVelocity = dir.normalized * speed;
        fireEvent.Post(gameObject);
    }

    void Update() {
        lifetime -= Time.deltaTime;
        if (lifetime <= 0f) Destroy(gameObject);
    }

    void OnTriggerEnter2D(Collider2D other) {
        if (other.CompareTag("Rock")) {
            bulletHit.Post(gameObject);

            Rock rock = other.GetComponent<Rock>();

            if (rock != null) {
                rock.TakeHit(damage);
            }

            Destroy(gameObject);
        }
    }
}

