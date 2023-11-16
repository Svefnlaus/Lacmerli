using System.Collections;
using UnityEditor.Recorder.Encoder;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Player : MonoBehaviour
{
    #region Static Variables

    private Rigidbody2D controller;
    private Animator animator;
    private SpawnManager spawner;

    [Space]

    [SerializeField] private SceneLoader loader;
    [SerializeField] private bool enemiesAlwaysMoving;

    [Space] [Header ("Slash Settings")]
    public static PolygonCollider2D swordRange;
    [Range (0.1f, 10)] [SerializeField] private float slashDuration;
    [Range (0.1f, 10)] [SerializeField] private float slashCooldown;

    [Space] [Header ("Attack Settings")]
    [SerializeField] private Transform rotator;
    [SerializeField] private Transform auraCaster;
    [Range (0.1f, 50)] [SerializeField] private float attackDamage;
    [Range (0.1f, 10)] [SerializeField] private float attackCharge;
    [Range (0.1f, 10)] [SerializeField] private float attackCooldown;
    [Range (0.1f, 10)] [SerializeField] private float attackDuration;

    [Space] [Header("Health Settings")]
    [SerializeField] private HealthBar health;
    [Range (10, 1000)] [SerializeField] private float maxHealth;
    [Range (0.1f, 10)] [SerializeField] private float deathDelay;

    [Space] [Header ("Movement Settings")]
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

    public static bool canMove;

    private bool canSlash;
    private bool canShoot;
    private bool canDash;

    private bool isAttacking;
    private bool isSlashing;
    private bool isDashing;
    private bool isDead;

    private float currentHealth;

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

    private bool dashActivated { get { return canDash && Input.GetKeyDown(KeyCode.LeftShift) ? true : false; } }
    private bool slashActivated { get { return canSlash && Input.GetKeyDown(KeyCode.Space) ? true : false; } }
    private bool shootActivated { get { return canShoot && Input.GetMouseButtonDown(0) ? true : false; } }


    #endregion

    private void Awake()
    {
        // initialize variables on awake
        swordRange  = GetComponentInChildren<PolygonCollider2D>();
        spawner     = GetComponentInChildren<SpawnManager>();

        controller  = GetComponent<Rigidbody2D>();
        animator    = GetComponent<Animator>();

        PlayerPrefs.SetInt("PreviousScene", currentScene);

        StartCoroutine(InitializeVariables());
    }

    private void Update()
    {
        // grant access for enemies to act whenever you act
        if (isDead) return;
        AccessGranter();
        Attack();
        Death();
        Slash();
    }

    private void FixedUpdate()
    {
        if (isDead) return;
        Walk();
    }

    public void TakeDamage(float damage)
    {
        // adds invulnerability when dashing
        if (isDashing) return;
        currentHealth -= damage;
        health.UpdateCurrentHealth(currentHealth);
    }

    #region Private Methods

    private void AccessGranter()
    {
        if (previousAccess == isPerformingAnAction) return;

        accessToMove = isPerformingAnAction || enemiesAlwaysMoving ? true : false;

        LightingBehavior.targetSize = accessToMove ? litDown : litUp;
        CameraBehavior.zoomSize = accessToMove ? zoomIn : zoomOut;

        previousAccess = isPerformingAnAction;
    }

    private IEnumerator InitializeVariables()
    {
        isDead = true;

        while (!SceneLoader.finishLoading)
        { 
            previousAccess = !isPerformingAnAction;
            yield return null;
        }

        swordRange.gameObject.SetActive(false);

        CameraBehavior.target = transform;

        health.SetMaxHealth(maxHealth);
        currentHealth = maxHealth;

        canSlash = true;
        canShoot = true;
        canMove = true;
        canDash = true;

        isDead = false;

        yield return null;
    }

    #region Move & Dash Mechanics

    private void Walk()
    {
        if (dashActivated) StartCoroutine(Dash());

        if (!isMoving || !canMove) return;

        velocity = Vector3.SmoothDamp(velocity, direction, ref currenVelocity, time);
        controller.MovePosition(transform.position + velocity * speed * Time.fixedDeltaTime);
    }

    private IEnumerator Dash()
    {
        isDashing = true;

        canDash = false;
        canShoot = false;
        canMove = false;

        TrailRenderer trail = GetComponent<TrailRenderer>();

        trail.time = 0.25f;
        controller.velocity = direction * dashForce;

        yield return new WaitForSeconds(dashDuration);
        controller.velocity = Vector2.zero;

        isDashing = false;
        canShoot = true;
        canMove = true;

        while(trail.time != 0)
        {
            trail.time = Mathf.MoveTowards(trail.time, 0, Time.deltaTime);
            yield return null;
        }

        yield return new WaitForSeconds(dashCooldown);
        canDash = true;
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
        yield return new WaitForSeconds(slashDuration);
        isSlashing = false;

        yield return new WaitForSeconds(slashCooldown);
        canSlash = true;
        yield return null;
    }

    #endregion

    #region Shoot Mechanics

    private void Attack()
    {
        if (!shootActivated) return;
        StartCoroutine(SpawnAura());
    }

    private void Aim()
    {
        Vector3 target = mousePosition - transform.position;
        animator.SetFloat("AtkY", target.normalized.y);
        animator.SetFloat("AtkX", target.normalized.x);
        rotator.rotation = Quaternion.FromToRotation(Vector2.right, target);
    }

    private IEnumerator SpawnAura()
    {
        isAttacking = true;

        // stop player from moving and casting another spell
        canShoot = false;
        canMove = false;
        canSlash = false;

        // spawn a clone of the said spell
        GameObject tempBullet = spawner.GetClone();

        // null catcher
        if (tempBullet == null) yield return null;

        Magic tempMagic = tempBullet.GetComponent<Magic>();
        tempMagic.damage = attackDamage;
        tempMagic.chargeTime = attackCharge;
        tempMagic.processing = false;

        tempBullet.transform.parent = auraCaster;

        // set the position and orientation of the spell
        Aim();
        tempBullet.transform.SetPositionAndRotation(auraCaster.position, auraCaster.rotation);
        tempBullet.SetActive(true);

        animator.SetTrigger("Aura");
        tempBullet.transform.parent = null;

        // despawn aura
        yield return new WaitForSeconds(attackDuration);
        isAttacking = false;
        canMove = true;
        canSlash = true;

        // cooldown
        yield return new WaitForSeconds(attackCooldown);
        canShoot = true;
    }

    #endregion

    #region Player Death

    private void Death()
    {
        if (currentHealth > 0.1f) return;
        isDead = true;
        StartCoroutine(PlayerDeath());
    }

    private IEnumerator PlayerDeath()
    {
        accessToMove = false;
        yield return new WaitForSeconds(deathDelay);
        SceneManager.LoadScene(8);
    }

    #endregion

    #endregion
}
