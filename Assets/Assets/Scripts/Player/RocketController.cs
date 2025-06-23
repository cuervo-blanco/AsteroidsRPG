using UnityEngine;
using System.Collections;

public class RocketController : MonoBehaviour {
    GameManager gm;

    [Header("Hit / Invincibility")]
    public float blinkDuration  = 1.0f;
    public float blinkInterval  = 0.05f;
    bool  invincible = false;
    private bool isGhost = false;

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
    private bool controlsInverted = false;

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
#if UNITY_ANDROID || UNITY_IOS
    Vector3 accel = Input.acceleration;

    float tiltX = Mathf.Clamp(accel.x, -1f, 1f);
    float tiltY = Mathf.Clamp(accel.y, -1f, 1f);

    if (controlsInverted) {
        tiltX *= -1f;
        tiltY *= -1f;
    }

    if (Mathf.Abs(tiltX) < 0.1f) tiltX = 0f;
    if (Mathf.Abs(tiltY) < 0.1f) tiltY = 0f;

    tiltY = Mathf.Clamp(tiltY, -0.5f, 1f);

    Vector2 rawTilt = new Vector2(tiltX, tiltY);
    inputDirection = Vector2.Lerp(inputDirection, rawTilt, Time.deltaTime * 5f);

    if (Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Began) {
        TryShoot();
    }

    if (Input.touchCount == 2 && Input.GetTouch(1).phase == TouchPhase.Began) {
        powerModule.TryActivateSuperMode();
    }
#else
        float moveX = Input.GetAxisRaw("Horizontal");
        float moveY = Input.GetAxisRaw("Vertical");
        if (controlsInverted) {
            moveX *= -1f;
            moveY *= -1f;
        }
        inputDirection = new Vector2(moveX, moveY).normalized;

        if (Input.GetKeyDown(KeyCode.Y)) {
            powerModule.TryActivateSuperMode();
        }
#endif
        bool thrust = inputDirection.y > 0;
        if (thrust) {
            thrustAccelTimer += Time.deltaTime;
            float t = Mathf.Clamp01(thrustAccelTimer / thrustAccelerationTime);
            currentThrustMultiplier = Mathf.SmoothStep(0f, 1f, t);
        } else {
            thrustAccelTimer = 0f;
            currentThrustMultiplier = 1f;
        }

        rocketAnimator.SetBool("isThrusting", thrust);
        rocketAnimator.SetBool("isShooting", shootTimer <= 0f);
        rocketAnimator.SetFloat("horizontal", inputDirection.x);

        if (thrust && !previousThrust) {
            fireAnimator.SetTrigger("startThrust");
        }
        else if (!thrust && previousThrust) {
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
        bool shoot = false;

#if UNITY_EDITOR || UNITY_STANDALONE
        shoot = Input.GetKey(KeyCode.Space);
#elif UNITY_ANDROID || UNITY_IOS
        shoot = Input.touchCount > 0;
#endif

        shootTimer -= Time.deltaTime;

#if UNITY_EDITOR || UNITY_STANDALONE
        if (Input.GetKeyDown(KeyCode.Y)) {
            powerModule.TryActivateSuperMode();
        }
#elif UNITY_ANDROID || UNITY_IOS
        // TODO: Add a touch gesture or button to activate super mode on mobile.
#endif
        if (shoot && shootTimer <= 0f) {
            Shoot();

            if (powerModule.superModeActive && powerModule.currentCoinType == MagicCoinType.FireRate) {
                shootTimer = shootCooldown * 0.5f;
            } else {
                shootTimer = shootCooldown;
            }
        }
    }

    void TryShoot() {
        if (shootTimer > 0f || isGhost) return;

        Shoot();

        if (powerModule.superModeActive && powerModule.currentCoinType == MagicCoinType.FireRate) {
            shootTimer = shootCooldown * 0.5f;
        } else {
            shootTimer = shootCooldown;
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
        if (isGhost == true) return;
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

    public void InvertControls(bool enabled) {
        controlsInverted = enabled;
    }
    public void SetGhostMode(bool enabled) {
        isGhost = enabled;

        Collider2D col = GetComponent<Collider2D>();
        if (col) col.enabled = !enabled;

        if (enabled) {
            foreach (var r in renderers) r.color = new Color(1f, 1f, 1f, 0.3f);
        } else {
            foreach (var r in renderers) r.color = new Color(1f, 1f, 1f, 1f);
        }
    }

    public void SetScale(float scale) {
        transform.localScale = new Vector3(scale, scale, 1f);
    }

    public void ResetShootTimer() {
        shootTimer = 0f;
    }
}

