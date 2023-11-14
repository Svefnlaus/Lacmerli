using System.Collections;
using UnityEngine;

public class TutorialPlayer : MonoBehaviour
{
    #region Static Variables

    private Rigidbody2D controller;
    private Animator animator;

    [Space]
    [Header("Attack Settings")]
    [SerializeField] private SpawnManager spawner;
    [SerializeField] private Transform rotator;
    [SerializeField] private Transform auraCaster;
    [SerializeField] private Transform cameraPan;
    [Range(0.1f, 10)][SerializeField] private float attackDuration;
    [Range(0.1f, 10)][SerializeField] private float attackCooldown;

    [Space]
    [Header("Movement Settings")]
    [Range(0.01f, 1)][SerializeField] private float time;
    [Range(0.1f, 10)][SerializeField] private float speed;
    [Range(0.1f, 50)][SerializeField] private float dashForce;
    [Range(0.1f, 50)][SerializeField] private float dashDuration;
    [Range(0.1f, 50)][SerializeField] private float dashCooldown;


    #endregion

    #region Dynamic Variables

    public static bool accessToMove;

    private Vector3 velocity;
    private Vector3 currenVelocity;

    private bool canSpawn;
    private bool canMove;
    private bool canDash;

    private bool isAttacking;
    private bool isDashing;

    private Vector3 direction
    {
        get
        {
            // prevent animator from rotating when attacking
            animator.SetBool("IsAttacking", isAttacking);
            if (isAttacking) return Vector2.zero;

            float x = Input.GetAxisRaw("Horizontal");
            float y = Input.GetAxisRaw("Vertical");

            animator.SetFloat("X", x);
            animator.SetFloat("Y", y);

            return new Vector3(x, y, 0).normalized;
        }
    }

    private bool attack
    {
        get
        {
            return canSpawn && Input.GetKeyDown(KeyCode.Space) ? true : false;
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
        controller = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        canSpawn = true;
        canMove = true;
        canDash = true;
        CameraBehavior.target = transform;
    }

    private void Update()
    {
        // grant access for enemies to act whenever you act
        AccessGranter();
    }

    private void FixedUpdate()
    {
        Walk();
        Attack();
    }

    #region Private Methods

    private void Walk()
    {
        if (canDash && Input.GetKeyDown(KeyCode.LeftShift)) StartCoroutine(Dash());
        if (!isMoving || !canMove || isDashing || isAttacking) return;
        velocity = Vector3.SmoothDamp(velocity, direction, ref currenVelocity, time);
        controller.MovePosition(transform.position + velocity * speed * Time.fixedDeltaTime);
    }

    private void Attack()
    {
        Aim();

        if (isAttacking || !attack) return;
        StartCoroutine(spawnAura());
    }

    private void Aim()
    {
        if (!Input.anyKey || isAttacking) return;

        // place the caster to where the player is facing
        rotator.localEulerAngles = direction.y > 0 ? new Vector3(0, 0, 90) :
            direction.x > 0 ? new Vector3(0, 0, 0) :
            direction.x < 0 ? new Vector3(0, 0, 180) :
            new Vector3(0, 0, -90);

        // --- can adjust to look for mouse position instead --- //
    }

    private void AccessGranter()
    {
        if (!SceneLoader.finishLoading) return;
        accessToMove = isAttacking || isMoving ? true : false;
        CameraBehavior.zoomSize = accessToMove ? 10 : 35;
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

        while (trail.time != 0)
        {
            trail.time = Mathf.MoveTowards(trail.time, 0, Time.deltaTime);
            yield return null;
        }

        yield return new WaitForSeconds(dashCooldown);
        canDash = true;
    }


    private IEnumerator spawnAura()
    {
        isAttacking = true;

        // stop player from moving and casting another spell
        canSpawn = false;
        canMove = false;

        // spawn a clone of the said spell
        GameObject tempBullet = spawner.GetClone();

        // null catcher
        if (tempBullet == null) yield return null;

        // set the position and orientation of the spell
        tempBullet.transform.position = auraCaster.position;
        tempBullet.transform.rotation = auraCaster.rotation;
        tempBullet.SetActive(true);

        CameraBehavior.target = cameraPan;

        // despawn aura
        yield return new WaitForSeconds(attackDuration);
        isAttacking = false;
        canMove = true;

        CameraBehavior.target = transform;

        // cooldown
        yield return new WaitForSeconds(attackCooldown);
        canSpawn = true;
    }

    #endregion
}
