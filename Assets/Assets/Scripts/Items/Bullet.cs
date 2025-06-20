using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(Collider2D))]

public class Bullet : MonoBehaviour {
    public float speed = 10f;
    public float lifetime = 2f;
    public AK.Wwise.Event fireEvent;
    public AK.Wwise.Event getHitEvent;
    public AK.Wwise.Event bulletHit;

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
                RocketPowerModule powerModule = GameObject.FindFirstObjectByType<RocketPowerModule>();

                bool destroyed = rock.TakeHit();

                if (powerModule != null && powerModule.superModeActive && !destroyed) {
                    rock.TakeHit();
                }
            }

            Destroy(gameObject);
        }
    }
}

