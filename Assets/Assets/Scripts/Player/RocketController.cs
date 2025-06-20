using UnityEngine;
using System.Collections;

public class RocketController : MonoBehaviour {
    GameManager gm;

    [Header("Hit / Invincibility")]
    public float blinkDuration  = 1.0f;
    public float blinkInterval  = 0.05f;
    bool  invincible = false;

    [Header("Lives / Health")]
    public AK.Wwise.Event explosionSound;
    public int maxLives = 3;
    int currentLives;

    [Header("Animation")]
    SpriteRenderer[] renderers;
    SpriteRenderer flameRenderer;
    public RocketWobble wobbleScript;
    public Animator rocketAnimator;
    public Animator fireAnimator;
    Coroutine blinkCR;

    [Header("Movement")]
    public float moveSpeed = 5f;
    public float backwardSpeedMultiplier = 0.5f;

    private Rigidbody2D rb;
    private Vector3 initialLocalPos;
    private Vector2 inputDirection;
    private bool previousThrust;

    public float thrustAccelerationTime = 0.2f;
    private float currentThrustMultiplier = 0f;
    private float thrustAccelTimer = 0f;

    private RocketPowerModule powerModule;

    [Header("Bullet")]
    public GameObject bulletPrefab;
    public Transform[] shootPoints;
    public float shootCooldown = 0.2f;
    private float shootTimer = 0f;

    void Start() {
        initialLocalPos = transform.localPosition;
        currentLives = maxLives;

        Camera cam = Camera.main;
        float z = Mathf.Abs(cam.transform.position.z - transform.position.z);
        transform.position = cam.ViewportToWorldPoint(new Vector3(0.5f, 0.5f, z));
    }

    void Awake() {
        rb  = GetComponent<Rigidbody2D>();
        renderers = GetComponentsInChildren<SpriteRenderer>(true);
        flameRenderer = fireAnimator.GetComponent<SpriteRenderer>();
        gm = GameManager.Instance;

        powerModule = GetComponent<RocketPowerModule>();
    }

    void Update() {
        float moveX = Input.GetAxisRaw("Horizontal");
        float moveY = Input.GetAxisRaw("Vertical");
        inputDirection = new Vector2(moveX, moveY).normalized;

        bool thrust = inputDirection.y > 0;
        if (inputDirection.y > 0) {
            thrustAccelTimer += Time.deltaTime;
            float t = Mathf.Clamp01(thrustAccelTimer / thrustAccelerationTime);
            currentThrustMultiplier = Mathf.SmoothStep(0f, 1f, t);
        } else {
            thrustAccelTimer = 0f;
            currentThrustMultiplier = 1f;
        }

        bool shoot = Input.GetKey(KeyCode.Space);

        rocketAnimator.SetBool("isThrusting", thrust);
        rocketAnimator.SetBool("isShooting", shoot);
        rocketAnimator.SetFloat("horizontal", moveX);

        if (thrust && !previousThrust) {
            fireAnimator.SetTrigger("startThrust");
        } else if (!thrust && previousThrust) {
            fireAnimator.SetTrigger("stopThrust");
        }

        fireAnimator.SetBool("isThrusting", thrust);
        previousThrust = thrust;

        if (wobbleScript != null) {
            wobbleScript.inputDirection = inputDirection;
        }

        ExecutePowerMode();
    }

    void ExecutePowerMode() {
        bool shoot = Input.GetKey(KeyCode.Space);
        shootTimer -= Time.deltaTime;

        if (Input.GetKeyDown(KeyCode.Y)) {
            powerModule.TryActivateSuperMode();
        }

        if (shoot && shootTimer <= 0f && !powerModule.superModeActive) {
            Shoot();
            shootTimer = shootCooldown;
        } else if (shoot && shootTimer <= 0f && powerModule.superModeActive) {
            Shoot();
            shootTimer = shootCooldown - (shootCooldown * 0.5f);
        }
    }

    void FixedUpdate() {
        Vector2 velocity = inputDirection * moveSpeed;

        if (inputDirection.y > 0) {
            velocity.y *= currentThrustMultiplier;
        } else if (inputDirection.y < 0) {
            velocity.y *= backwardSpeedMultiplier;
        }

        if (inputDirection.y < 0) {
            velocity.y *= backwardSpeedMultiplier;
        }

        rb.linearVelocity = velocity;
    }

    void Shoot() {
        foreach (Transform sp in shootPoints) {
            GameObject go = Instantiate(bulletPrefab, sp.position, Quaternion.identity);
            go.GetComponent<Bullet>().Initialise(sp.up);
        }
    }

    void OnTriggerEnter2D(Collider2D col) {
        if (invincible) return;
        if (col.CompareTag("Rock")) {
            Invoke(nameof(ClearHitFlag), 0.1f);
            TakeLife();
        } else if (col.CompareTag("Life")) {
            GiveLife();
        }
        return;
    }

    void ClearHitFlag() => rocketAnimator.SetBool("gotHit", false);

    void TakeLife() {
        rocketAnimator.SetTrigger("gotHit");

        currentLives = Mathf.Max(0, currentLives - 1);

        if (blinkCR != null) StopCoroutine(blinkCR);
        blinkCR = StartCoroutine(BlinkCoroutine());

        gm?.SetLifeScore(currentLives);

        if (currentLives == 0) {
            StartCoroutine(ExplodeAndHide());
        }
    }

    public void GiveLife() {
        currentLives = Mathf.Min(currentLives + 1, maxLives);
        gm?.SetLifeScore(currentLives);
    }

    public void SetDamageTolerance(float percentage) {
        invincible = (percentage == 100.0f);
    }

    System.Collections.IEnumerator BlinkCoroutine() {
        invincible = true;
        float timer = 0f;
        bool  on    = false;

        while (timer < blinkDuration) {
            on = !on;
            foreach (var r in renderers) r.enabled = on;
            if (flameRenderer) flameRenderer.enabled = on;
            yield return new WaitForSeconds(blinkInterval);
            timer += blinkInterval;
        }

        foreach (var r in renderers) r.enabled = true;
        if (flameRenderer) flameRenderer.enabled = true;
        invincible = false;
    }

    System.Collections.IEnumerator ExplodeAndGameOver() {
        this.enabled = false;
        rb.linearVelocity  = Vector2.zero;

        yield return new WaitForSeconds(0.5f);

        UnityEngine.SceneManagement.SceneManager.LoadScene(0);
    }

    public void EnableControl(bool on) {
        enabled = on;
        if (rb) rb.linearVelocity = Vector2.zero;
    }

    public void ResetLives() {
        gameObject.SetActive(true);

        foreach (var r in renderers) r.enabled = true;
        if (flameRenderer != null) {
            flameRenderer.enabled = true;
        }

        GetComponent<Collider2D>().enabled = true;
        fireAnimator.gameObject.SetActive(true);

        fireAnimator.Rebind();
        fireAnimator.Update(0f);

        rocketAnimator.Rebind();
        rocketAnimator.Update(0f);

        rb.linearVelocity = Vector2.zero;
        transform.localPosition = initialLocalPos;

        currentLives = maxLives;
        invincible = false;
        previousThrust = false;
        thrustAccelTimer = 0f;

        shootTimer = 0f;
    }

    IEnumerator ExplodeAndHide() {
        fireAnimator.Rebind();
        fireAnimator.gameObject.SetActive(false);

        explosionSound.Post(gameObject);
        rocketAnimator.SetTrigger("explode");
        EnableControl(false);
        GetComponent<Collider2D>().enabled = false;

        yield return new WaitForSeconds(0.8f);

        foreach (var r in renderers) r.enabled = false;
        gameObject.SetActive(false);
    }
}

