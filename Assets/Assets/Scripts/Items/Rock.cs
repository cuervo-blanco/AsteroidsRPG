using UnityEngine;

/// <summary>One asteroid-style rock that can split into smaller ones.</summary>
[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(SpriteRenderer))]

public class Rock : MonoBehaviour
{
    [Header("Sprites")]
    public Sprite[] rockSpritesNormal;
    public Sprite[] rockSpritesGlow;

    [Header("Splitting")]
    public int   generation      = 0;
    public int   maxGenerations  = 2;
    public float childScaleMul   = 0.6f;
    public int   baseHitsBiggest = 20;
    public int   hitsFalloff     = 10;

    [Header("Physics")]
    public float minSpeed   = 0.5f;
    public float maxSpeed   = 2.0f;
    public float maxSpin    = 100f;

    SpriteRenderer sr;
    int hitsLeft;
    bool _preset = false;

    bool hasEnteredView = false;

    void OnBecameVisible()
    {
        hasEnteredView = true;
    }

    void OnBecameInvisible()
    {
        if (hasEnteredView) {
            Destroy(gameObject);
        }
    }


    void Start()
    {
        if (_preset) return;
        sr = GetComponent<SpriteRenderer>();

        int spriteIndex = Random.Range(0, rockSpritesNormal.Length);
        sr.sprite = rockSpritesNormal[spriteIndex];

        float genScale = Mathf.Pow(childScaleMul, generation);
        float scaleRandom = Random.Range(0.9f, 1.1f);
        transform.localScale = Vector3.one * genScale * scaleRandom;

        transform.Rotate(0, 0, Random.Range(0f, 360f));

        hitsLeft = baseHitsBiggest - hitsFalloff * generation;

        Rigidbody2D rb = GetComponent<Rigidbody2D>();
        rb.linearVelocity = Random.insideUnitCircle.normalized *
                             Random.Range(minSpeed, maxSpeed);
        rb.angularVelocity = Random.Range(-maxSpin, maxSpin);
        rb.gravityScale = 0;
        rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;

        RebuildCollider();
    }

    public bool TakeHit()
    {
        hitsLeft--;
        if (hitsLeft > 0)
        {
            Glow();
            return false;
        }

        Split();
        return true;
    }

    public void Glow()
    {
        int idx = System.Array.IndexOf(rockSpritesNormal, sr.sprite);
        if (idx >= 0 && idx < rockSpritesGlow.Length)
        {
            sr.sprite = rockSpritesGlow[idx];
            Invoke(nameof(RevertToNormal), 0.2f);
        }
    }
    void RevertToNormal() => sr.sprite = rockSpritesNormal[
        System.Array.IndexOf(rockSpritesGlow, sr.sprite)];

    void Split()
    {
        if (generation >= maxGenerations)
        {
            Destroy(gameObject);
            return;
        }

        int   pieces = Random.Range(2, 5);
        float parentSpd  = GetComponent<Rigidbody2D>().linearVelocity.magnitude;
        const float BOOST = 2f;

        for (int i = 0; i < pieces; i++)
        {
            Rock child = Instantiate(gameObject, transform.position, Quaternion.identity)
                         .GetComponent<Rock>();

            child._preset = true;
            child.generation = generation + 1;

            float jitter = Random.Range(0.9f, 1.05f);
            child.transform.localScale = transform.localScale * childScaleMul * jitter;

            child.RebuildCollider();

            Rigidbody2D rbChild = child.GetComponent<Rigidbody2D>();
            Vector2 dir = Random.insideUnitCircle.normalized;
            float speed = Mathf.Max(parentSpd * BOOST, minSpeed);
            rbChild.linearVelocity = dir * speed;                 // use velocity
            rbChild.angularVelocity = Random.Range(-maxSpin, maxSpin);
        }

        Destroy(gameObject);
    }

    void RebuildCollider()
    {
        PolygonCollider2D pc = GetComponent<PolygonCollider2D>();
        if (pc) Destroy(pc);
        gameObject.AddComponent<PolygonCollider2D>();
    }

    void Update()
    {
        if (!hasEnteredView) return;

        Vector3 v = Camera.main.WorldToViewportPoint(transform.position);
        if (v.x < -0.2f || v.x > 1.2f || v.y < -0.2f || v.y > 1.2f)
            Destroy(gameObject);
    }
}

