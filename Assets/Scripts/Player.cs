using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Player : MonoBehaviour
{
    #region Static Variables

    private Rigidbody2D controller;
    private Animator animator;

    [SerializeField] private SceneLoader loader;
    [SerializeField] private bool enemiesAlwaysMoving;

    [Space] [Header ("Attack Settings")]
    [SerializeField] private SpawnManager spawner;
    [SerializeField] private Transform rotator;
    [SerializeField] private Transform auraCaster;
    [Range(0.1f, 10)][SerializeField] private float attackDuration;
    [Range(0.1f, 10)][SerializeField] private float attackCooldown;

    [Space] [Header("Health Settings")]
    [SerializeField] private HealthBar health;
    [Range(1, 120)][SerializeField] private float maxHealth;

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
    public static PolygonCollider2D swordRange;

    #endregion

    #region Dynamic Variables

    public static bool accessToMove;

    private Vector3 velocity;
    private Vector3 currenVelocity;

    public static bool canMove;
    private bool canSpawn;
    private bool canDash;

    private bool isAttacking;
    private bool isDashing;

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

    private bool attack
    {
        get
        {
            return canSpawn && Input.GetMouseButtonDown(0) ? true : false;
        }
    }

    private bool isMoving
    {
        get
        {
            bool moving = direction.magnitude > 0 ? true : false;
            animator.SetBool("IsMoving", moving);

            return moving;
        }
    }

    #endregion

    private void Awake()
    {
        // initialize variables on awake
        swordRange = GetComponentInChildren<PolygonCollider2D>();
        controller = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        canSpawn = true;
        canMove = true;
        canDash = true;
        CameraBehavior.target = transform;

        health.SetMaxHealth(maxHealth);
        currentHealth = maxHealth;

        swordRange.gameObject.SetActive(false);

        PlayerPrefs.SetInt("PreviousScene", currentScene);
    }

    private void Update()
    {
        // grant access for enemies to act whenever you act
        AccessGranter();
        Death();
        Attack();
        Slice();
    }

    private void FixedUpdate()
    {
        Walk();
    }

    #region Private Methods

    private void Walk()
    {
        if (canDash && Input.GetKeyDown(KeyCode.LeftShift)) StartCoroutine(Dash());

        if (!isMoving || !canMove) return;

        velocity = Vector3.SmoothDamp(velocity, direction, ref currenVelocity, time);
        controller.MovePosition(transform.position + velocity * speed * Time.fixedDeltaTime);
    }

    private void Attack()
    {
        if (isAttacking || isDashing || !attack) return;
        StartCoroutine(SpawnAura());
    }

    private void Aim()
    {
        Vector3 target = mousePosition - transform.position;
        animator.SetFloat("AtkY", target.normalized.y);
        animator.SetFloat("AtkX", target.normalized.x);
        rotator.rotation = Quaternion.FromToRotation(Vector2.right, target);
    }

    private void Slice()
    {
        if (!Input.GetKeyDown(KeyCode.Space) || isAttacking) return;
        animator.SetTrigger("Slice");
    }

    public void TakeDamage(float damage)
    {
        // adds invulnerability when dashing
        if (isDashing) return;
        currentHealth -= damage;
        health.UpdateCurrentHealth(currentHealth);
    }

    private void AccessGranter()
    {
        if (!SceneLoader.finishLoading) return;
        accessToMove = isAttacking || isDashing || isMoving || enemiesAlwaysMoving ? true : false;
        LightingBehavior.targetSize = accessToMove ? litDown : litUp;
        CameraBehavior.zoomSize = accessToMove ? zoomIn : zoomOut;
    }

    private void Death()
    {
        if (currentHealth > 0.1f) return;
        loader.LoadScene(6);
    }

    private IEnumerator Dash()
    {
        isDashing = true;

        canDash = false;
        canSpawn = false;
        canMove = false;

        TrailRenderer trail = GetComponent<TrailRenderer>();

        trail.time = 0.25f;
        controller.velocity = direction * dashForce;

        yield return new WaitForSeconds(dashDuration);
        controller.velocity = Vector2.zero;

        isDashing = false;
        canSpawn = true;
        canMove = true;

        while(trail.time != 0)
        {
            trail.time = Mathf.MoveTowards(trail.time, 0, Time.deltaTime);
            yield return null;
        }

        yield return new WaitForSeconds(dashCooldown);
        canDash = true;
    }

    private IEnumerator SpawnAura()
    {
        Aim();
        isAttacking = true;

        // stop player from moving and casting another spell
        canSpawn = false;
        canMove = false;

        // spawn a clone of the said spell
        GameObject tempBullet = spawner.GetClone();

        // null catcher
        if (tempBullet == null) yield return null;

        // set the position and orientation of the spell
        tempBullet.transform.SetPositionAndRotation(auraCaster.position, auraCaster.rotation);
        tempBullet.SetActive(true);
        animator.SetTrigger("Aura");

        // despawn aura
        yield return new WaitForSeconds(attackDuration);
        isAttacking = false;
        canMove = true;

        // cooldown
        yield return new WaitForSeconds(attackCooldown);
        canSpawn = true;
    }

    #endregion
}
