using System.Collections;
using System.Linq;
using UnityEngine;

public class BossBehavior : MonoBehaviour
{
    #region Static Variables
    [Header ("Boss Settings")]
    [SerializeField] private Transform dump;
    [SerializeField] private Transform target;
    [SerializeField] private float bossTerritory;

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
    [Range (0.1f, 100)] [SerializeField] private float throwCooldown;
    [Range (0.1f,  10)] [SerializeField] private float throwDistance;
    [Range (0.1f,  10)] [SerializeField] private float throwInterval;

    [Header ("Ultimate Settings")]
    [SerializeField] private SpawnManager ultimate;

    [Range (1, 50)]     [SerializeField] private int lightningQuantity;

    [Range (0.1f, 100)] [SerializeField] private float ultimateCooldown;
    [Range (0.1f, 100)] [SerializeField] private float ultimateRange;
    [Range (0.1f,  10)] [SerializeField] private float ultimateDelay;
    [Range (0.1f,  10)] [SerializeField] private float gap;

    #endregion

    #region Dynamic Variables

    private Vector2[] setPositions;

    private float currentHealth;

    private bool canMove;
    private bool canThrow;
    private bool canUlt;

    private float distance { get { return Vector2.Distance(transform.position, target.position); } }

    #endregion

    private void Awake()
    {
        canMove = true;
        canThrow = true;
        canUlt = false;
    }

    private void Start()
    {
        health.SetMaxHealth(maxHealth);
        currentHealth = maxHealth;
        health.gameObject.SetActive(false);
    }

    private void Update()
    {
        if (canThrow) StartCoroutine(Throw());
        if (canUlt) StartCoroutine(Ultimate());
        if (target == null) return;
        ShowHealthBar();
    }

    public void TakeDamage(float damage)
    {
        currentHealth -= damage;
        health.UpdateCurrentHealth(currentHealth);
    }

    private void ShowHealthBar()
    {
        if (distance > bossTerritory) return;
        health.gameObject.SetActive(true);
    }

    #region Throwing Mechanics

    private void SpawnInCircle(float radius, int quantity)
    {
        for (int current = 0; current < quantity; current++)
        {
            float thetaScale = (float)current / quantity;
            float theta = thetaScale * 2 * Mathf.PI;

            float x = Mathf.Cos(theta) * radius;
            float y = Mathf.Sin(theta) * radius;

            Vector3 newPosition = new Vector2(x, y);

            GameObject throwable = throwables.GetClone();
            if (throwable == null) return;
            SetInMotion(throwable, newPosition);
        }
    }

    private void SetInMotion(GameObject _object, Vector2 position)
    {
        _object.transform.parent = rotor;
        _object.transform.localPosition = position;
        Vector3 relativePosition = transform.position - _object.transform.position;
        _object.transform.localRotation = Quaternion.FromToRotation(Vector2.up, relativePosition);
        _object.SetActive(true);
        _object.GetComponent<Rigidbody2D>().AddRelativeForce(Vector2.down * 15, ForceMode2D.Impulse);
        _object.transform.parent = dump;
    }

    private IEnumerator Throw()
    {
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

        yield return new WaitForSeconds(throwCooldown);
        canThrow = true;
    }

    #endregion

    #region Ultimate Mechanics

    private IEnumerator Ultimate()
    {
        canUlt = false;
        canMove = false;
        setPositions = new Vector2[lightningQuantity];
        GenerateRandomPosition();

        GameObject[] lightning = new GameObject[lightningQuantity];
        for (int current = 0; current < lightning.Length; current++)
        { 
            lightning[current] = ultimate.GetClone();

            if (lightning[current] == null) yield return null;

            lightning[current].transform.parent = transform;
            lightning[current].SetActive(true);
            lightning[current].transform.position = setPositions[current];
            lightning[current].transform.parent = dump;

            yield return new WaitForSeconds(ultimateDelay);
        }

        canMove = true;

        yield return new WaitForSeconds(ultimateCooldown);
        canUlt = true;
    }

    private void GenerateRandomPosition()
    {
        setPositions[0] = transform.position;
        for (int current = 0; current < setPositions.Length; current++)
        {
            CheckPosition(current);
        }
    }

    private void CheckPosition(int count)
    {
        bool finished = false;
        Vector2 currentPosition;
        int attempt = 0;
        do
        {
            attempt++;
            int x = Mathf.RoundToInt(Random.Range(transform.position.x - ultimateRange, transform.position.x + ultimateRange));
            int y = Mathf.RoundToInt(Random.Range(transform.position.y - ultimateRange, transform.position.y + ultimateRange));

            currentPosition = new Vector2(x, y);

            if (setPositions.Contains(currentPosition)) continue;

            float offset = Vector2.Distance(currentPosition, transform.position);
            if (offset < gap) continue;

            for (int current = 0; current < setPositions.Length; current++)
            {
                float distance = Vector2.Distance(currentPosition, setPositions[current]);

                if (distance < gap) break;

                if (current == setPositions.Length - 1) finished = true;
            }

            if (!finished) continue;
            setPositions[count] = currentPosition;
            Debug.Log("Position: " + (count + 1) + "\tAttempts: " + attempt);

        } while (!finished);
    }

    #endregion
}
