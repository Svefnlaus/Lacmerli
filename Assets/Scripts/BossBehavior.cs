using System.Collections;
using System.Linq;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D), typeof(Animator))]
public class BossBehavior : MonoBehaviour
{
    #region Static Variables
    [Header ("Boss Settings")]
    [SerializeField] private Transform dump;
    [SerializeField] private Transform target;
    [SerializeField] private GameObject reward;

    [SerializeField] private Animator cutScene;

    [SerializeField] private float bossTerritory;
    [SerializeField] private float speed;

    private ParticleSystem particles;
    private Rigidbody2D controller;
    private AudioSource soundFX;
    private Animator animator;

    [Header ("Health Settings")]
    [SerializeField] private HealthBar health;
    [Range (10, 999)]   [SerializeField] private float maxHealth;

    [Header ("Rotor Settings")]
    [SerializeField] private Transform rotor;
    [Range (-100, 100)] [SerializeField] private float rotationSpeed;

    [Header ("Throw Settings")]
    [SerializeField] private SpawnManager throwables;

    [Range (1, 500)]    [SerializeField] private int throwQuantity;
    [Range (1,  50)]    [SerializeField] private int throwSet;

    [Range (0.1f, 100)] [SerializeField] private float throwForce;
    [Range (0.1f, 100)] [SerializeField] private float throwRange;
    [Range (0.1f, 100)] [SerializeField] private float throwDamage;
    [Range (0.1f, 100)] [SerializeField] private float throwCooldown;
    [Range (0.1f,  10)] [SerializeField] private float throwDistance;
    [Range (0.1f,  10)] [SerializeField] private float throwInterval;

    [Header ("Ultimate Settings")]
    [SerializeField] private SpawnManager ultimate;
    [SerializeField] private GameObject warning;

    [Range (1, 50)]     [SerializeField] private int lightningQuantity;

    [Range (0.1f, 100)] [SerializeField] private float ultimateCooldown;
    [Range (0.1f, 100)] [SerializeField] private float ultimateDamage;
    [Range (0.1f, 100)] [SerializeField] private float ultimateRange;
    [Range (0.01f, 10)] [SerializeField] private float ultimateDelay;
    [Range (0.01f, 10)] [SerializeField] private float quakeIntensity;
    [Range (0.01f, 10)] [SerializeField] private float quakeDuration;
    [Range (0.01f, 10)] [SerializeField] private float chargeTime;
    //[Range (0.01f, 10)] [SerializeField] private float gap;

    #endregion

    #region Dynamic Variables

    private Vector3 velocity;
    private Vector3 currentVelocity;

    private Vector2[] setPositions;

    private float currentHealth;

    private float gap;

    private bool canMove;
    private bool canThrow;
    private bool canUlt;

    private bool isDead;

    private bool bossFightStarted;

    private float distance { get { return Vector2.Distance(transform.position, target.position); } }

    private Vector3 direction
    {
        get
        {
            if (!canMove) return Vector2.zero;

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

    private bool isMoving
    {
        get
        {
            bool moving = direction.magnitude > 0.1 ? true : false;
            if (!moving) controller.velocity = Vector3.zero;
            animator.SetBool("IsWalking", moving);
            return moving;
        }
    }

    private bool playerIsDetected { get { return distance < bossTerritory; } }

    #endregion

    #region Unity Methods

    private void Awake()
    {
        particles = GetComponentInChildren<ParticleSystem>();
        controller = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        soundFX = GetComponent<AudioSource>();

        canMove = true;
        canThrow = true;
        canUlt = false;

        isDead = false;

        bossFightStarted = false;

        particles.Stop();
        soundFX.Stop();

        var charge = particles.main;
        charge.duration = chargeTime;
    }

    private void Start()
    {
        // always initialize health on start not on awake
        InitializeBossHealth();
        StartCoroutine(BossCutScene());
    }

    private void Update()
    {
        if (!playerIsDetected || isDead) return;

        BossReposition();
        InitializeBossBattle();

        if (canThrow) StartCoroutine(Throw());
        if (canUlt) StartCoroutine(Ultimate());
    }

    #endregion

    #region Miscellaneous

    public void TakeDamage(float damage)
    {
        currentHealth = Mathf.Clamp(currentHealth - damage, 0, maxHealth);
        health.UpdateCurrentHealth(currentHealth);
        Death();
    }

    private IEnumerator BossCutScene()
    {
        Player.finishLoadingScene = false;
        cutScene.SetTrigger("CutScene");
        yield return new WaitForSeconds(8.75f);
        Player.finishLoadingScene = true;
    }

    private void InitializeBossBattle()
    {
        if (!playerIsDetected || bossFightStarted) return;
        bossFightStarted = true;

        health.gameObject.SetActive(true);
        soundFX.Play();
    }

    private void InitializeBossHealth()
    {
        health.SetMaxHealth(maxHealth);
        currentHealth = maxHealth;
        health.gameObject.SetActive(false);
    }

    private void BossReposition()
    {
        if (!isMoving || !canMove) return;
        velocity = Vector3.SmoothDamp(velocity, direction, ref currentVelocity, 0.01f);
        controller.MovePosition(transform.position + velocity * speed * Time.fixedDeltaTime);
    }

    #endregion

    #region Throwing Mechanics

    private void SpawnInCircle(float radius, int quantity)
    {
        for (int current = 0; current < quantity; current++)
        {
            // calculate circumference progress
            float thetaScale = (float)current / quantity;

            // create a dot depending on where the circumference is currently at
            float theta = thetaScale * 2 * Mathf.PI;

            // give the generated dot a distance depending on the specified radius
            float x = Mathf.Cos(theta) * radius;
            float y = Mathf.Sin(theta) * radius;

            // generate a circumferencial coordinates using the dot
            Vector3 newPosition = new Vector2(x, y);

            // get a clone of an object from a spawner
            GameObject throwable = throwables.GetClone();
            if (throwable == null) continue;

            // if the clone is not empty, set the object in motion
            SetInMotion(throwable, newPosition);
        }
    }

    private void SetInMotion(GameObject _object, Vector2 position)
    {
        // parent to the rotor to move the positions according to the rotors orientation
        _object.transform.parent = rotor;

        // assign the generated circumferencial position to the object
        _object.transform.localPosition = position;

        // rotate the object to the relative position of spawner and said object
        Vector3 relativePosition = transform.position - _object.transform.position;
        _object.transform.localRotation = Quaternion.FromToRotation(Vector2.up, relativePosition);

        // get component of the object to freely set the damage
        _object.TryGetComponent<Throwables>(out Throwables thrownObj);
        if (thrownObj == null) return;
        thrownObj.damage = throwDamage;

        // activate the object and make it move down, relatively, to not be affected by world's down position
        _object.SetActive(true);
        _object.GetComponent<Rigidbody2D>().AddRelativeForce(Vector2.down * throwForce, ForceMode2D.Impulse);

        // throw it in the dump to keep thing tidy
        _object.transform.parent = dump;
    }

    private IEnumerator Throw()
    {
        if (distance > throwRange) yield break;
        canThrow = false;
        canMove = false;
        Throwables.origin = transform.position;
        Throwables.maxDistance = throwRange;

        int throwCount = 0;
        rotor.localEulerAngles = Vector3.zero;

        rotationSpeed *= -1;

        while (throwCount < throwSet)
        {
            rotor.Rotate(new Vector3(0, 0, rotationSpeed));
            yield return new WaitForSeconds(throwInterval);

            SpawnInCircle(throwDistance, throwQuantity);
            throwCount++;
            yield return null;
        }

        canMove = true;

        yield return new WaitForSeconds(ultimateCooldown);
        canUlt = true;
    }

    #endregion

    #region Ultimate Mechanics

    private IEnumerator Ultimate()
    {
        if (distance > ultimateRange) yield break;
        canUlt = false;
        canMove = false;
        setPositions = new Vector2[lightningQuantity];

        // initiate the charging particle and warning box, despawn after charging finished
        particles.Play();
        yield return new WaitForSeconds(chargeTime);

        // generate random positions beforehand to keep things smooth
        GenerateRandomPosition();

        GameObject[] lightning = new GameObject[lightningQuantity];
        for (int current = 0; current < lightning.Length; current++)
        { 
            lightning[current] = ultimate.GetClone();

            if (lightning[current] == null) continue;

            // assign the object on the generated position
            lightning[current].transform.parent = transform;

            // specify creeeping damage
            AbberatingThunder thunder = lightning[current].GetComponentInChildren<AbberatingThunder>();
            if (thunder == null) continue;
            thunder.creepingDamage = ultimateDamage;
            thunder.quakeIntensity = quakeIntensity;
            thunder.quakeDuration = quakeDuration;

            lightning[current].SetActive(true);
            lightning[current].transform.localPosition = setPositions[current];

            // put it in a dump to keep it tidy
            lightning[current].transform.parent = dump;

            yield return new WaitForSeconds(ultimateDelay);
        }

        canMove = true;

        yield return new WaitForSeconds(throwCooldown);
        canThrow = true;
    }

    private void GenerateRandomPosition()
    {
        gap = (ultimateRange - 1) / (lightningQuantity / ultimateRange) / 2;
        if (gap < 1) Debug.Log("Error Distance");
        if (gap < 1) return;
        Debug.Log("Gap: " + gap);

        setPositions[0] = transform.position;
        for (int current = 0; current < setPositions.Length; current++)
        {
            CheckPosition(current);
        }
    }

    private void CheckPosition(int count)
    {
        // initialize variables
        bool finished = false;

        // debug testing purposes
        int attempt = 0;

        do
        {
            attempt++;

            // randomly assign a coordinate
            float x = Random.Range(-ultimateRange, ultimateRange);
            float y = Random.Range(-ultimateRange, ultimateRange);

            Vector2 currentPosition = new Vector2(x, y);

            // check if the said position is already on the list
            if (setPositions.Contains(currentPosition)) continue;

            // if the said position is close to spawner then disregard
            float offset = Vector2.Distance(currentPosition, transform.position);
            if (offset < gap) continue;

            // check for the distance of every preset positions to prevent from dropping on the same place
            for (int current = 0; current < setPositions.Length; current++)
            {
                // assign a distance to serve as a gap for each summons
                float distance = Vector2.Distance(currentPosition, setPositions[current]);
                // if said gap was not attained, repeat the process
                if (distance < gap) break;

                // if that was true for the whole preset list, then finish the process
                if (current == setPositions.Length - 1) finished = true;
            }

            // if the process broke midway and was not finished, repeat the process
            if (!finished) continue;
            setPositions[count] = currentPosition;

            // debug checking
            Debug.Log("Position: " + (count + 1) + "\tAttempts: " + attempt + "\t:\t" + currentPosition);
        }
        while (!finished);
    }

    #endregion

    #region Boss Death

    private void Death()
    {
        if (health.percentage.value >= 0.1 || isDead) return;
        isDead = true;
        animator.SetTrigger("Death");
    }

    private void OnDisable()
    {
        // spawn a reward for killing the boss
        Instantiate(reward, transform.position, Quaternion.identity, null);
    }

    #endregion
}
