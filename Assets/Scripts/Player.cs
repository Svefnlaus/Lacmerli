using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Player : MonoBehaviour
{
    #region Static Variables

    private Rigidbody2D controller;
    private Animator animator;
    private SpawnManager spawner;

    [Space] [Header ("Level Settings")]
    [SerializeField] private SceneLoader loader;
    [SerializeField] private GameObject gameOverScreen;
    [SerializeField] private GameObject coinDisplay;
    [SerializeField] private GameObject objectivesBoard;
    [SerializeField] private bool enemiesAlwaysMoving;

    [Space] [Header ("Sound FX")]
    [SerializeField] private AudioSource swing;
    [SerializeField] private AudioSource whoosh;
    [SerializeField] private AudioSource charge;

    [Space] [Header ("Slash Settings")]
    public static PolygonCollider2D swordRange;
    [Range (0.1f, 10)] [SerializeField] private float slashDuration;
    [Range (0.1f, 10)] [SerializeField] private float slashCooldown;

    [Space] [Header ("Attack Settings")]
    [SerializeField] private Transform rotator;
    [SerializeField] private Transform auraCaster;
    [SerializeField] private FillBar energyBar;
    [Range (0.1f, 50)] [SerializeField] private float attackDamage;
    [Range (0.1f, 10)] [SerializeField] private float attackCharge;
    [Range (0.1f, 10)] [SerializeField] private float attackCooldown;
    [Range (0.1f, 10)] [SerializeField] private float attackDuration;

    [Space] [Header("Health Settings")]
    [SerializeField] private HealthBar health;
    [Range (10, 1000)] [SerializeField] private float maxHealth;
    [Range (0.1f, 10)] [SerializeField] private float deathDelay;

    [Space] [Header ("Movement Settings")]
    [SerializeField] private FillBar dashBar;
    [Range (0.01f, 1)] [SerializeField] private float time;
    [Range (0.1f, 10)] [SerializeField] private float speed;
    [Range (0.1f, 50)] [SerializeField] private float dashForce;
    [Range (0.1f, 50)] [SerializeField] private float dashDuration;
    [Range (0.1f, 50)] [SerializeField] private float dashCooldown;

    [Space] [Header("Camera Settings")]
    [Range (0.1f, 50)] [SerializeField] private float zoomIn;
    [Range (10f, 100)] [SerializeField] private float zoomOut;

    [Space] [Header("Lights Settings")]
    [Range (0.1f, 90)] [SerializeField] private float litDown;
    [Range (10f, 900)] [SerializeField] private float litUp;
    private int currentScene { get { return SceneManager.GetActiveScene().buildIndex; } }

    #endregion

    #region Dynamic Variables

    private Vector3 velocity;
    private Vector3 currenVelocity;

    public static bool accessToMove;

    private bool previousAccess;

    public static bool finishLoadingScene;
    public static bool canMove;

    private bool canSlash;
    private bool canShoot;
    private bool canDash;

    private bool isAttacking;
    private bool isSlashing;
    private bool isDashing;

    public static bool isDead;

    private float currentHealth;
    private float currentEnergy;

    private Vector3 direction
    {
        get
        {
            // prevent animator from rotating when attacking
            if (isAttacking) return Vector2.zero;

            float x = Input.GetAxisRaw("Horizontal");
            float y = Input.GetAxisRaw("Vertical");

            animator.SetFloat("X", x);
            animator.SetFloat("Y", y);

            return new Vector3(x, y, 0).normalized;
        }
    }

    private Vector3 mousePosition
    {
        get
        {
            Vector3 mouse = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            mouse.z = 0;
            return mouse;
        }
    }

    private bool isMoving
    {
        get
        {
            bool moving = direction.magnitude > 0 ? true : false;
            if(!moving) controller.velocity = Vector3.zero;
            animator.SetBool("IsMoving", moving);

            return moving;
        }
    }

    private bool isPerformingAnAction { get { return isAttacking || isDashing || isMoving || isSlashing ? true : false; } }
    private bool isInvulnerable { get { return isDashing && controller.velocity.magnitude > 0 ? true : false; } }

    private bool shootActivated
    {
        get
        {
            canShoot = energyBar.slider.value == 1;
            return canShoot && Input.GetMouseButton(0) && Time.timeScale != 0 ? true : false;
        }
    }
    private bool dashActivated
    {
        get
        {
            canDash = dashBar.slider.value == 1;
            return canDash && Input.GetKeyDown(KeyCode.LeftShift) && !isDashing ? true : false;
        }
    }
    private bool slashActivated { get { return canSlash && Input.GetKeyDown(KeyCode.Space) ? true : false; } }

    private bool objectiveIsDone { get { return Objectives.coinsFound == Objectives.totalCoins
                && Objectives.enemiesSlain == Objectives.totalEnemies; } }


    #endregion

    private void Awake()
    {
        // initialize variables on awake
        swordRange  = GetComponentInChildren<PolygonCollider2D>();
        spawner     = GetComponentInChildren<SpawnManager>();

        controller  = GetComponent<Rigidbody2D>();
        animator    = GetComponent<Animator>();

        PlayerPrefs.SetInt("PreviousScene", currentScene);

    }

    private void Start()
    {
        StartCoroutine(InitializeVariables());
    }

    private void Update()
    {
        if (isDead) return;
        // grant access for enemies to act whenever you act
        AccessGranter();

        Attack();
        Death();
        Slash();
        StartCoroutine(Dash());
    }

    private void FixedUpdate()
    {
        if (isDead) return;
        Walk();
    }

    #region Methods

    #region Miscellaneous

    public void TakeDamage(float damage)
    {
        // adds invulnerability when dashing
        // if (isInvulnerable) return;
        currentHealth -= damage;
        health.UpdateCurrentHealth(currentHealth);
    }

    private IEnumerator InitializeVariables()
    {
        isDead = true;

        swordRange.gameObject.SetActive(false);

        CameraBehavior.target = transform;

        health.SetMaxHealth(maxHealth);
        currentHealth = maxHealth;

        canSlash = true;
        canShoot = true;
        canMove = true;
        canDash = true;

        isDead = false;
        previousAccess = !isPerformingAnAction;

        yield return null;
    }

    private void AccessGranter()
    {
        if (previousAccess == isPerformingAnAction || !finishLoadingScene || objectiveIsDone) return;

        accessToMove = isPerformingAnAction || enemiesAlwaysMoving ? true : false;

        LightingBehavior.targetSize = accessToMove ? litDown : litUp;
        CameraBehavior.zoomSize = accessToMove ? zoomIn : zoomOut;

        previousAccess = isPerformingAnAction;
    }

    private IEnumerator Cooldown(FillBar bar, float cooldown)
    {
        float elapsedTime = 0;
        while (elapsedTime != cooldown)
        {
            elapsedTime = Mathf.Clamp(elapsedTime + Time.deltaTime, 0, cooldown);
            float fill = elapsedTime / cooldown;
            bar.UpdateFillBar(fill);
            yield return new WaitForSeconds(Time.deltaTime);
        }
    }

    #endregion

    #region Move & Dash Mechanics

    private void Walk()
    {

        if (!isMoving || !canMove) return;

        velocity = Vector3.SmoothDamp(velocity, direction, ref currenVelocity, time);
        controller.MovePosition(transform.position + velocity * speed * Time.fixedDeltaTime);
    }

    private IEnumerator Dash()
    {
        if (!dashActivated) yield break;

        controller.velocity = direction * dashForce;
        if (controller.velocity.magnitude <= 0.1) yield break;

        dashBar.UpdateFillBar(0);
        isDashing = true;
        canMove = false;

        TryGetComponent<TrailRenderer>(out TrailRenderer trail);
        if (trail == null) yield break;
        trail.time = 0.25f;

        whoosh.Play();

        yield return new WaitForSeconds(dashDuration);
        controller.velocity = Vector2.zero;

        while(trail.time != 0)
        {
            trail.time = Mathf.MoveTowards(trail.time, 0, Time.deltaTime);
            yield return null;
        }

        isDashing = false;
        canMove = true;

        StartCoroutine(Cooldown(dashBar, dashCooldown));
    }

    #endregion

    #region Slash Mechanics

    private void Slash()
    {
        if (!slashActivated) return;
        StartCoroutine(PerformSlash());
    }

    private IEnumerator PerformSlash()
    {
        isSlashing = true;
        canSlash = false;
        animator.SetTrigger("Slice");
        swing.Play();
        yield return new WaitForSeconds(slashDuration);
        isSlashing = false;

        yield return new WaitForSeconds(slashCooldown);
        canSlash = true;
        canDash = true;
        yield return null;
    }

    #endregion

    #region Shoot Mechanics

    private void Attack()
    {
        if (!shootActivated) return;
        StartCoroutine(ShootAura());
    }

    private void Aim()
    {
        Vector3 target = mousePosition - transform.position;
        animator.SetFloat("AtkY", target.normalized.y);
        animator.SetFloat("AtkX", target.normalized.x);
        rotator.rotation = Quaternion.FromToRotation(Vector2.right, target);
    }

    private IEnumerator ShootAura()
    {
        if (!canShoot) yield break;
        // spawn a clone of the said spell
        GameObject tempBullet = spawner.GetClone();
        if (tempBullet == null) yield break;

        // access the magic behavior
        tempBullet.TryGetComponent<Magic>(out Magic tempMagic);
        if (tempMagic == null) yield break;

        // prevent from looping
        energyBar.UpdateFillBar(0);
        isAttacking = true;

        // stop player from moving and casting another spell
        canMove = false;
        canSlash = false;

        tempMagic.damage = attackDamage;
        tempMagic.processing = false;
        tempMagic.chargeTime = attackCharge;

        tempBullet.transform.parent = auraCaster;

        // set the position and orientation of the spell
        Aim();
        tempBullet.transform.SetPositionAndRotation(auraCaster.position, auraCaster.rotation);
        tempBullet.SetActive(true);

        charge.Play();
        yield return new WaitForSeconds(attackCharge);

        animator.SetTrigger("Aura");
        tempBullet.transform.parent = null;

        // despawn aura
        yield return new WaitForSeconds(attackDuration);
        isAttacking = false;
        canMove = true;
        canSlash = true;

        // cooldown
        StartCoroutine(Cooldown(energyBar, attackCooldown));
    }

    #endregion

    #region Player Death

    private void Death()
    {
        if (currentHealth > 0.1f) return;
        isDead = true;
        PlayerDeath();
    }

    private void PlayerDeath()
    {
        gameOverScreen.SetActive(true);
        CameraBehavior.zoomSize = 1;
        LightingBehavior.targetSize = 0;
        health.gameObject.SetActive(false);
        coinDisplay.SetActive(false);
        objectivesBoard.SetActive(false);
        accessToMove = false;
    }

    #endregion

    #endregion
}
