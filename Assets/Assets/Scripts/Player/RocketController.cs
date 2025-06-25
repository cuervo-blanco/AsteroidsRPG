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
    private Vector2 momentumVelocity = Vector2.zero;

    [SerializeField] private float defaultAcceleration = 20f;
    [SerializeField] private float defaultDrag = 2f;
    public float slipperyAcceleration = 20f;
    public float slipperyDrag = 2f;

    [Header("Bullet")]
    public GameObject bulletPrefab;
    public Transform[] shootPoints;
    public float shootCooldown = 0.2f;
    private float shootTimer = 0f;
    float baseShootCooldown;
    float baseMoveSpeed;
    float baseScale;

    private BoxCollider2D boxCollider;
    private Vector2 baseColliderSize;

    public PlayerStats stats;

    void Start() {
        initialLocalPos = transform.localPosition;
        currentLives = maxLives;

        Camera cam = Camera.main;
        float z = Mathf.Abs(cam.transform.position.z - transform.position.z);
        transform.position = cam.ViewportToWorldPoint(new Vector3(0.5f, 0.5f, z));
    }

    void Awake() {
        baseShootCooldown = shootCooldown;
        baseMoveSpeed = moveSpeed;
        baseScale = transform.localScale.x;

        rb  = GetComponent<Rigidbody2D>();
        renderers = GetComponentsInChildren<SpriteRenderer>(true);
        flameRenderer = fireAnimator.GetComponent<SpriteRenderer>();
        boxCollider = GetComponent<BoxCollider2D>();

        if (boxCollider != null) {
            baseColliderSize = boxCollider.size;
        }

        gm = GameManager.Instance;

        powerModule = GetComponent<RocketPowerModule>();
    }

    void Update () {
#if (UNITY_ANDROID || UNITY_IOS) && !UNITY_EDITOR
        ReadTouch();
#else
        ReadKeyboard();
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

        ApplyPowerModifiers();
        ExecutePowerMode();
    }

    void ReadKeyboard() {
        float moveX = Input.GetAxisRaw("Horizontal");
        float moveY = Input.GetAxisRaw("Vertical");

        if (controlsInverted) { moveX *= -1f; moveY *= -1f; }

        inputDirection = new Vector2(moveX, moveY).normalized;

        if (Input.GetKeyDown(KeyCode.Space)) TryShoot();
    }

    void ReadTouch() {
        inputDirection = Vector2.zero;

        if (Input.touchCount == 0) return;

        Touch t = Input.GetTouch(0);

        float halfW = Screen.width  * 0.5f;
        float halfH = Screen.height * 0.5f;

        float xDir = t.position.x < halfW ? -1f : 1f;
        float yDir = t.position.y < halfH ? -1f : 1f;

        if (Mathf.Abs(t.position.x - halfW) < 20f) xDir = 0f;   // dead-zone
        if (Mathf.Abs(t.position.y - halfH) < 20f) yDir = 0f;

        inputDirection = new Vector2(xDir, yDir);

        if (controlsInverted) inputDirection = -inputDirection;

        if (t.phase == TouchPhase.Began) TryShoot();
    }


    void ApplyPowerModifiers() {
        float finalShootCooldown = baseShootCooldown;
        float finalMoveSpeed = baseMoveSpeed;
        float finalScale = baseScale;

        if (powerModule.superModeActive) {
            if (powerModule.activePowers.Contains(MagicCoinType.FireRate)) {
                finalShootCooldown *= 0.5f;
            }
            if (powerModule.activePowers.Contains(MagicCoinType.Overheat)) {
                finalShootCooldown *= 2f;
            }
            if (powerModule.activePowers.Contains(MagicCoinType.SpeedBoost)) {
                finalMoveSpeed *= 2f;
            }
            if (powerModule.activePowers.Contains(MagicCoinType.MiniMode)) {
                finalScale *= 0.5f;
            }
            if (powerModule.activePowers.Contains(MagicCoinType.Ghost)) {
                SetGhostMode(true);
            }
            if (powerModule.activePowers.Contains(MagicCoinType.Ghost)) {
                SetGhostMode(true);
            }
            if (powerModule.activePowers.Contains(MagicCoinType.SlipperyBoost)) {
                SetSlipperyMode(true);
            }
            invincible = powerModule.activePowers.Contains(MagicCoinType.Shield);
            InvertControls(powerModule.activePowers.Contains(MagicCoinType.Confusion));
        } else {
            invincible = false;
            InvertControls(false);
            SetGhostMode(false);
            SetSlipperyMode(false);
        }

        shootCooldown = finalShootCooldown;
        moveSpeed = finalMoveSpeed;
        SetScale(finalScale);
    }

    public void SetSlipperyMode(bool enable) {
        if (enable) {
            slipperyAcceleration = 25f;
            slipperyDrag = 0.5f;
        } else {
            slipperyAcceleration = defaultAcceleration;
            slipperyDrag = defaultDrag;
        }
    }

    void ExecutePowerMode() {
        shootTimer -= Time.deltaTime;
#if UNITY_EDITOR || UNITY_STANDALONE
        bool shoot = Input.GetKey(KeyCode.Space);
#elif UNITY_ANDROID || UNITY_IOS
        bool shoot = Input.touchCount > 0;
#endif
        if (shoot && shootTimer <= 0f) {
            TryShoot(); // centralize logic
        }
    }

    void TryShoot() {
        if (shootTimer > 0f || isGhost) return;

        Shoot();

        if (powerModule.superModeActive && powerModule.activePowers.Contains(MagicCoinType.FireRate)) {
            shootTimer = shootCooldown * 0.5f;
        } else {
            shootTimer = shootCooldown;
        }
    }

    void FixedUpdate() {
        Vector2 push = inputDirection * slipperyAcceleration * Time.fixedDeltaTime;
        momentumVelocity += push;

        momentumVelocity = Vector2.Lerp(momentumVelocity, Vector2.zero, slipperyDrag * Time.fixedDeltaTime);

        rb.linearVelocity = momentumVelocity;
    }

    void Shoot() {
        if (isGhost == true) return;
        foreach (Transform sp in shootPoints) {
            GameObject go = Instantiate(bulletPrefab, sp.position, Quaternion.identity);
            Bullet bullet = go.GetComponent<Bullet>();
            bullet.Initialise(sp.up);
            bullet.damage = stats.GetPlayerBulletDamage();
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
        if (boxCollider != null) {
            boxCollider.size = baseColliderSize * scale;
        }
    }

    public void ResetShootTimer() {
        shootTimer = 0f;
    }
}

