using TMPro;
using UnityEngine;

public class PracticeMode : SpawnManager
{
    #region Variables

    private bool willCrash
    {
        get
        {
            Vector2 min = minimumPlaneAxis;
            Vector2 max = maximumPlaneAxis;
            return min == max ||
                max.x <= min.x ||
                max.y <= min.y ||
                min == Vector2.zero ||
                max == Vector2.zero ? true :
                false;
        }
    }

    [Space]
    [Space]
    [Header("Random Position Settings")]
    [Space]

    [SerializeField] private int numberOfEnemies;
    [SerializeField] private Vector2 minimumPlaneAxis;
    [SerializeField] private Vector2 maximumPlaneAxis;

    [Space]
    [Space]
    [Header("Distance Scaler Settings")]
    [Space]

    [Range(1, 500)][SerializeField] private float scalingValue;
    [Range(0, 15)][SerializeField] private float minimumRange;
    [Range(5, 45)][SerializeField] private float maximumRange;
    private float minimumDistance;

    [Space]
    [Space]
    [Header("Additional Settings")]
    [Space]

    [SerializeField] private GameObject[] enemies;
    [SerializeField] private Transform player;

    private GameObject[] enemiesSpawned;

    private Vector2[] usedPositions;
    private Vector2 currentPosition;

    #endregion

    protected override void Awake()
    {
        PlayerPrefs.SetString("GameMode", "Practice");

        // catch the error if ever the assigned axis is critical
        if (willCrash)
        {
            Debug.LogError("Double check the spawn axis then run again");
            return;
        }

        // scale the distancing between spawned objects depending on the spawn count
        minimumDistance = Mathf.Clamp(scalingValue / numberOfEnemies, minimumRange, maximumRange);

        // set a length for the array of the positions
        usedPositions = new Vector2[numberOfEnemies];
        usedPositions[0] = transform.position;

        // initialize the array of spawned enemies
        enemiesSpawned = new GameObject[numberOfEnemies];


        for (int current = 0; current < enemiesSpawned.Length; current++)
        {
            // catch null if ever you forgot to input the spawnees
            if (enemies == null) return;

            // instantiate random enemy sprites
            enemiesSpawned[current] = Instantiate(enemies[Random.Range(0, enemies.Length)], this.transform);

            // initialize variables essential to the enemies
            EnemyBehavior enemy = enemiesSpawned[current].GetComponentInChildren<EnemyBehavior>();

            // catch null
            if (enemy == null) return;
            enemy.spawner = this;

            // generate random position for enemies to spawn into
            enemiesSpawned[current].transform.position = RandomVectorGenerator(current);
        }
    }

    private Vector2 RandomVectorGenerator(int current)
    {
        bool vectorIsAccepted = false;
        int attempt = 0;

        Vector2 min = minimumPlaneAxis;
        Vector2 max = maximumPlaneAxis;

        do
        {
            // generate random coordinates based on the spawn orientation set
            int x = Random.Range((int)min.x, (int)max.x + 1);
            int y = Random.Range((int)min.y, (int)max.y + 1);

            // generate a position using the said coordinates
            currentPosition = new Vector2(x, y);

            attempt++;

            // check every position in the array of set positions
            for (int count = 0; count < usedPositions.Length; count++)
            {
                // break the loop if the conditions aren't met
                if (currentPosition == usedPositions[count] || currentPosition == Vector2.zero) break;

                // determine the distance between the current position and the positions in the array
                float distance = Vector2.Distance(currentPosition, usedPositions[count]);

                // break the loop if the distance is shorter than the minimum distance set
                if (distance < minimumDistance) break;

                // occur only on last position in the array
                if (count == (usedPositions.Length - 1))
                {
                    // if the generated position is met all the conditions of all the positions in the array then accept the vector
                    vectorIsAccepted = true;
                    Debug.Log("Position " + (current + 1) + ", Attempts: " + attempt);
                }
            }

        } while (!vectorIsAccepted);

        // add the generated position to the array
        usedPositions[current] = currentPosition;
        return currentPosition;
    }

}
