using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Animator), typeof(Rigidbody2D))]
public class EnemyBehavior : MonoBehaviour
{
    #region Static Variables

    private Rigidbody2D controller;
    private Animator animator;

    [Space] [Header("Attack Settings")]
    public SpawnManager spawner;
    public Transform target;
    [Space]
    [SerializeField] private GameObject parent;
    [SerializeField] private Transform rotator;
    [SerializeField] private Transform caster;
    [Range(0.01f, 5)][SerializeField] private float attackTime;
    [Range(0.01f, 5)][SerializeField] private float attackDelay;
    [Range(0.1f, 10)][SerializeField] private float attackCooldown;
    [Range(0.1f, 50)][SerializeField] private float damageReceived;

    [Space] [Header("Health Settings")]
    [SerializeField] private HealthBar health;
    [Range(1, 120)][SerializeField] private float maxHealth;
    [Range(0.01f, 5)][SerializeField] private float showHealthDelay;

    [Space] [Header ("Range Settings")]
    [Range(0.1f, 30)][SerializeField] private float detectionRange;
    [Range(0.1f, 10)][SerializeField] private float attackRange;

    [Space] [Header ("Speed Settings")]
    [Range(0.1f, 10)][SerializeField] private float speed;
    [Range(0.01f, 1)][SerializeField] private float smoothSpeed;
    [Range(0.1f, 10)][SerializeField] private float rotationSpeed;

    #endregion

    #region Dynamic Variables

    private Vector3 velocity;
    private Vector3 currentVelocity;

    private bool canMove;
    private bool canAttack;
    private bool isAttacking;

    private float currentHealth;
    private float previousHealth;

    private Vector3 direction
    {
        get
        {
            if (!playerIsDetected || !canMove) return Vector2.zero;

            // find player position
            float x = transform.position.x < target.position.x ? 1 :
                transform.position.x > target.position.x ? -1 : 0;
            float y = transform.position.y < target.position.y ? 1 :
                transform.position.y > target.position.y ? -1 : 0;

            animator.SetFloat("X", x);
            animator.SetFloat("Y", y);

            return new Vector3(x, y, 0).normalized;
        }
    }

    private bool playerIsDetected
    {
        get
        {
            return Player.accessToMove && distance < detectionRange ? true : false;
        }
    }

    private bool readyToAttack
    {
        get
        {
            animator.SetBool("IsAttacking", isAttacking);
            return distance < attackRange && canAttack ? true : false;
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

    private float distance { get { return Vector2.Distance(transform.position, target.position); } }

    #endregion

    private void Awake()
    {
        // initialize variables
        controller = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();

        canAttack = true;
        canMove = true;
    }

    private void Start()
    {
        health.SetMaxHealth(maxHealth);
        currentHealth = maxHealth;
        previousHealth = currentHealth;
        health.gameObject.SetActive(false);
    }

    private void Update()
    {
        if (target == null || spawner == null) return;
        Death();
        Attack();
    }

    private void LateUpdate()
    {
        if (target == null || spawner == null) return;
        DistanceChecker();
    }

    #region Private Methods

    private void DistanceChecker()
    {
        if (!isMoving || !playerIsDetected || isAttacking || distance < attackRange) return;
        velocity = Vector3.SmoothDamp(velocity, direction, ref currentVelocity, smoothSpeed);
        controller.MovePosition(transform.position + velocity * speed * Time.fixedDeltaTime);
    }

    private void Aim()
    {
        Vector2 relativePosition = target.position - rotator.position;
        rotator.rotation = Quaternion.FromToRotation(Vector2.up, relativePosition);

        animator.SetFloat("X", relativePosition.normalized.x);
        animator.SetFloat("Y", relativePosition.normalized.y);
    }

    public void TakeDamage(float damage)
    {
        currentHealth -= damage;
        health.UpdateCurrentHealth(currentHealth);
        StartCoroutine(showHealthBar());
    }

    private void Death()
    {
        if (currentHealth > 0.1f) return;
        Objectives.enemiesSlain++;
        parent.SetActive(false);
    }

    private void Attack()
    {
        if (!readyToAttack) return;
        canMove = false;
        canAttack = false;
        StartCoroutine(attackPattern());
    }

    private IEnumerator attackPattern()
    {
        if (spawner == null) yield return null;
        isAttacking = true;
        yield return new WaitForSeconds(attackDelay);

        GameObject tempBullet = spawner.GetClone();

        if (tempBullet == null || gameObject == null) yield return null;

        Aim();

        tempBullet.transform.parent = parent.transform;
        tempBullet.transform.SetPositionAndRotation(caster.position, caster.rotation);
        tempBullet.SetActive(true);

        yield return new WaitForSeconds(attackTime);
        isAttacking = false;
        canMove = true;

        yield return new WaitForSeconds(attackCooldown);
        canAttack = true;
    }

    private IEnumerator showHealthBar()
    {
        health.gameObject.SetActive(true);
        yield return new WaitForSeconds(showHealthDelay);
        if (currentHealth == previousHealth) health.gameObject.SetActive(false);
        previousHealth = currentHealth;
        yield return null;
    }

    #endregion
}
