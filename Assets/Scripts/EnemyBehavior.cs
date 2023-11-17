using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Animator), typeof(Rigidbody2D))]
public class EnemyBehavior : MonoBehaviour
{
    #region Static Variables

    private Rigidbody2D controller;
    private Animator animator;
    private AudioSource soundFX;

    [Space] [Header("Attack Settings")]
    public SpawnManager spawner;
    public Transform target;

    [Space]
    [SerializeField] private GameObject parent;
    [SerializeField] private Transform rotator;
    [SerializeField] private Transform caster;
    [Range(0.01f, 5)][SerializeField] private float attackTime;
    [Range(0.01f, 5)][SerializeField] private float attackDelay;
    [Range(0.01f, 5)][SerializeField] private float attackCooldown;
    [Range(0.1f, 50)][SerializeField] private float attackDamage;

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

    #endregion

    #region Dynamic Variables

    private Vector3 velocity;
    private Vector3 currentVelocity;

    private bool canMove;
    private bool canAttack;
    private bool isAttacking;

    private bool isDead;

    private float currentHealth;
    private float previousHealth;

    private float displayHealthTime;

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
            return distance < attackRange && canAttack && !isAttacking ? true : false;
        }
    }

    private bool isMoving
    {
        get
        {
            bool moving = direction.magnitude > 0 ? true : false;
            if (!moving) controller.velocity = Vector3.zero;
            animator.SetBool("IsMoving", moving);
            return moving;
        }
    }

    private float distance { get { return Vector2.Distance(transform.position, target.position); } }

    private bool willError
    { 
        get
        {
            bool errorCaught = target == null || spawner == null || isDead || Player.isDead || this == null ? true : false;
            if (errorCaught) controller.velocity = Vector2.zero;
            return errorCaught;
        }
    }

    #endregion

    private void Awake()
    {
        // initialize variables
        controller = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        soundFX = GetComponent<AudioSource>();

        canAttack = true;
        canMove = true;
    }

    private void Start()
    {
        // set health on start not on awake since enemies are spawned on awake
        health.SetMaxHealth(maxHealth);
        currentHealth = maxHealth;
        previousHealth = currentHealth;
        health.gameObject.SetActive(false);
    }

    private void Update()
    {
        if (willError) return;
        Attack();

        // was put here to prevent dying in pratice mode
        Death();
    }

    private void LateUpdate()
    {
        if (willError) return;
        EnemyMove();
    }

    public void TakeDamage(float damage)
    {
        health.gameObject.SetActive(true);
        currentHealth -= damage;
        health.UpdateCurrentHealth(currentHealth);

        // show health bar when damaged
        StartCoroutine(showHealthBar());
    }

    #region Private Methods

    private IEnumerator showHealthBar()
    {
        displayHealthTime += showHealthDelay;
        while (displayHealthTime > 0.1)
        {
            displayHealthTime -= Time.deltaTime;
            yield return null;
        }
        if (displayHealthTime < 0.1) health.gameObject.SetActive(false);
    }

    private void Death()
    {
        if (currentHealth > 0.1f) return;
        isDead = true;
        Objectives.enemiesSlain++;
        parent.SetActive(false);
    }

    private void EnemyMove()
    {
        // walk towards the player when the conditions are met
        if (!isMoving || !playerIsDetected || isAttacking || distance < attackRange) return;
        velocity = Vector3.SmoothDamp(velocity, direction, ref currentVelocity, smoothSpeed);
        controller.MovePosition(transform.position + velocity * speed * Time.fixedDeltaTime);
    }

    #region Basic Attack Mechanics

    private void Aim()
    {
        Vector2 relativePosition = target.position - rotator.position;
        rotator.rotation = Quaternion.FromToRotation(Vector2.up, relativePosition);

        animator.SetFloat("X", relativePosition.normalized.x);
        animator.SetFloat("Y", relativePosition.normalized.y);
    }

    private void Attack()
    {
        if (!readyToAttack) return;
        StartCoroutine(attackPattern());
    }

    private IEnumerator attackPattern()
    {
        if (spawner == null || this == null) yield break;

        GameObject tempBullet = spawner.GetClone();

        if (tempBullet == null || gameObject == null) yield break;

        tempBullet.TryGetComponent<Magic>(out Magic tempMagic);

        if (tempMagic == null) yield break;

        canAttack = false;
        canMove = false;

        tempMagic.damage = attackDamage;
        tempMagic.chargeTime = attackDelay;

        tempMagic.processing = false;

        Aim();

        tempBullet.transform.parent = parent.transform;
        tempBullet.transform.SetPositionAndRotation(caster.position, caster.rotation);
        tempBullet.SetActive(true);

        soundFX.Play();
        yield return new WaitForSeconds(attackDelay);
        isAttacking = true;

        yield return new WaitForSeconds(attackTime);
        isAttacking = false;
        canMove = true;

        yield return new WaitForSeconds(attackCooldown);
        canAttack = true;
    }

    #endregion

    #endregion
}
