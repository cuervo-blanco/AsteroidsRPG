using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(Collider2D))]

public class Bullet : MonoBehaviour
{
    public float speed = 10f;
    public float lifetime = 2f;
    public AK.Wwise.Event fireEvent;
    public AK.Wwise.Event bulletHit;

    Rigidbody2D rb;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.gravityScale = 0;
        rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
    }

    public void Initialise(Vector2 dir)
    {
        rb.linearVelocity = dir.normalized * speed;
        fireEvent.Post(gameObject);
    }

    void Update()
    {
        lifetime -= Time.deltaTime;
        if (lifetime <= 0f) Destroy(gameObject);
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Rock")) {
            bulletHit.Post(gameObject);

            Rocket rocket = GameObject.FindObjectOfType<Rocket>();

            Rock rock = other.GetComponent<Rock>();

            if (rocket != null && rocket.superModeActive && rock != null) {
                rock.TakeHit();
            }
            Destroy(gameObject);
        }
    }

}

