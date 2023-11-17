using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class TutorialPlayer : MonoBehaviour
{
    #region Static Variables

    private Rigidbody2D controller;
    private Animator animator;

    [SerializeField] private SceneLoader loader;
    [SerializeField] private bool enemiesAlwaysMoving;

    [Space]
    [Header("Sound FX")]
    [SerializeField] private AudioSource swing;
    [SerializeField] private AudioSource whoosh;

    [Space]
    [Header("Slash Settings")]
    public static PolygonCollider2D swordRange;
    [Range(0.1f, 10)][SerializeField] private float slashDuration;
    [Range(0.1f, 10)][SerializeField] private float slashCooldown;

    [Space]
    [Header("Attack Settings")]
    [SerializeField] private SpawnManager spawner;
    [SerializeField] private Transform rotator;
    [SerializeField] private Transform auraCaster;
    [Range(0.1f, 50)][SerializeField] private float attackDamage;
    [Range(0.1f, 10)][SerializeField] private float attackCharge;
    [Range(0.1f, 10)][SerializeField] private float attackDuration;
    [Range(0.1f, 10)][SerializeField] private float attackCooldown;

    [Space]
    [Header("Movement Settings")]
    [Range(0.01f, 1)][SerializeField] private float time;
    [Range(0.1f, 10)][SerializeField] private float speed;
    [Range(0.1f, 50)][SerializeField] private float dashForce;
    [Range(0.1f, 50)][SerializeField] private float dashDuration;
    [Range(0.1f, 50)][SerializeField] private float dashCooldown;

    [Space]
    [Header("Camera Settings")]
    [Range(0.1f, 50)][SerializeField] private float zoomIn;
    [Range(10f, 100)][SerializeField] private float zoomOut;

    [Space]
    [Header("Lights Settings")]
    [Range(0.1f, 90)][SerializeField] private float litDown;
    [Range(10f, 900)][SerializeField] private float litUp;
    private int currentScene { get { return SceneManager.GetActiveScene().buildIndex; } }

    #endregion

    #region Dynamic Variables

    public static bool accessToMove;

    private Vector3 velocity;
    private Vector3 currenVelocity;

    public static bool canMove;
    private bool canSpawn;
    private bool canDash;
    private bool canSlash;

    private bool isAttacking;
    private bool isDashing;
    private bool isSlashing;

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
        canSlash = true;
        CameraBehavior.target = transform;

        swordRange.gameObject.SetActive(false);

        PlayerPrefs.SetInt("PreviousScene", currentScene);

        Player.isDead = false;
    }

    private void Update()
    {
        // grant access for enemies to act whenever you act
        AccessGranter();
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
        Player.accessToMove = false;
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
        if (!canSlash || !Input.GetKeyDown(KeyCode.Space)) return;
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
        yield return null;
    }

    private void AccessGranter()
    {
        accessToMove = isAttacking || isDashing || isMoving || enemiesAlwaysMoving ? true : false;
        LightingBehavior.targetSize = accessToMove ? litDown : litUp;
        CameraBehavior.zoomSize = accessToMove ? zoomIn : zoomOut;
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

        if (controller.velocity.magnitude > 0.1) whoosh.Play();
        else canDash = true;

        yield return new WaitForSeconds(dashDuration);
        controller.velocity = Vector2.zero;

        isDashing = false;
        canSpawn = true;
        canMove = true;

        while (trail.time != 0)
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

        Magic tempMagic = tempBullet.GetComponent<Magic>();
        tempMagic.damage = attackDamage;
        tempMagic.chargeTime = attackCharge;
        tempMagic.processing = false;

        // set the position and orientation of the spell
        tempBullet.transform.SetPositionAndRotation(auraCaster.position, auraCaster.rotation);
        tempBullet.SetActive(true);

        animator.SetTrigger("Aura");
        yield return new WaitForSeconds(attackCharge);

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
