using UnityEngine;

/// <summary>One asteroid-style rock that can split into smaller ones.</summary>
[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(SpriteRenderer))]

public class Rock : MonoBehaviour {
    [Header("Sprites")]

    public Sprite[] rockSpritesNormal;
    public Sprite[] rockSpritesGlow;

    [Header("Splitting")]
    public int generation = 0;
    public int maxGenerations = 2;
    public float childScaleMul = 0.6f;
    public int baseHitsBiggest = 20;
    public int hitsFalloff = 10;
    int hitsLeft;

    [Header("Physics")]
    public float minSpeed = 0.5f;
    public float maxSpeed = 2.0f;
    public float maxSpin = 100f;

    SpriteRenderer sr;

    bool _preset = false;

    bool hasEnteredView = false;
    int baseValue = 1;

    void OnBecameVisible() {
        hasEnteredView = true;
    }

    void OnBecameInvisible() {
        if (hasEnteredView) {
            Destroy(gameObject);
        }
        return;
    }

    public void Initialize(int gen) {
        generation = gen;

        if (sr == null) sr = GetComponent<SpriteRenderer>();

        int spriteIndex = Random.Range(0, rockSpritesNormal.Length);
        sr.sprite = rockSpritesNormal[spriteIndex];

        float genScale = Mathf.Pow(childScaleMul, generation);
        float scaleRandom = Random.Range(0.9f, 1.1f);
        transform.localScale = Vector3.one * genScale * scaleRandom;

        hitsLeft = baseHitsBiggest - hitsFalloff * generation;
    }

    void Start() {
        sr = GetComponent<SpriteRenderer>();

        if (hitsLeft == 0) {
            Initialize(generation);
        }

        transform.Rotate(0, 0, Random.Range(0f, 360f));

        Rigidbody2D rb = GetComponent<Rigidbody2D>();
        rb.linearVelocity = Random.insideUnitCircle.normalized * Random.Range(minSpeed, maxSpeed);
        rb.angularVelocity = Random.Range(-maxSpin, maxSpin);
        rb.gravityScale = 0;
        rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;

        RebuildCollider();
        SetupLight();
    }

    void SetupLight() {
        Transform light = transform.Find("SmallRockLight");
        if (light && Random.value < 0.25f) {
            if (light.TryGetComponent<UnityEngine.Rendering.Universal.Light2D>(out var light2D)) {
                float scale = transform.localScale.x;

                light2D.pointLightOuterRadius = 1.2f * scale;
                light2D.pointLightInnerRadius = Mathf.Clamp(0.1f * scale, 0.05f, light2D.pointLightOuterRadius - 0.05f);
                light2D.intensity = Mathf.Lerp(1f, 3f, scale);
                light2D.color = Color.Lerp(Color.blue, Color.cyan, Random.value);
            }
            light.gameObject.SetActive(true);
        }
    }

    public bool TakeHit() {
        hitsLeft--;
        if (hitsLeft > 0) {
            Glow();
            return false;
        }

        AwardValue();
        Split();
        return true;
    }

    void AwardValue() {
        int value = baseValue * (maxGenerations - generation + 1);
        GameManager.Instance.AddRockValue(value);
    }

    public void Glow() {
        int idx = System.Array.IndexOf(rockSpritesNormal, sr.sprite);
        if (idx >= 0 && idx < rockSpritesGlow.Length)
        {
            sr.sprite = rockSpritesGlow[idx];
            Invoke(nameof(RevertToNormal), 0.2f);
        }
    }
    void RevertToNormal() => sr.sprite = rockSpritesNormal[
        System.Array.IndexOf(rockSpritesGlow, sr.sprite)];

    void Split() {
        if (generation >= maxGenerations) {
            Destroy(gameObject);
            return;
        }

        int pieces = Random.Range(2, 5);
        float parentSpd  = GetComponent<Rigidbody2D>().linearVelocity.magnitude;
        const float BOOST = 2f;

        for (int i = 0; i < pieces; i++) {
            Rock child = Instantiate(gameObject, transform.position, Quaternion.identity)
                         .GetComponent<Rock>();

            child.Initialize(generation + 1);

            child.RebuildCollider();

            Rigidbody2D rbChild = child.GetComponent<Rigidbody2D>();
            Vector2 dir = Random.insideUnitCircle.normalized;
            float speed = Mathf.Max(parentSpd * BOOST, minSpeed);
            rbChild.linearVelocity = dir * speed;                 // use velocity
            rbChild.angularVelocity = Random.Range(-maxSpin, maxSpin);
        }

        Destroy(gameObject);
    }

    void RebuildCollider() {
        PolygonCollider2D pc = GetComponent<PolygonCollider2D>();
        if (pc) Destroy(pc);
        gameObject.AddComponent<PolygonCollider2D>();
    }

    void Update() {
        if (!hasEnteredView) return;

        Vector3 v = Camera.main.WorldToViewportPoint(transform.position);
        if (v.x < -0.2f || v.x > 1.2f || v.y < -0.2f || v.y > 1.2f)
            Destroy(gameObject);
    }
}

