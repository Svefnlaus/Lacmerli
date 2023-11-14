using TMPro;
using UnityEngine;

public class LevelSpawner : SpawnManager
{
    #region Variables

    // clamp the rounds to a hundred
    public static int round
    { get { return PlayerPrefs.HasKey("Round") ? Mathf.Clamp(PlayerPrefs.GetInt("Round"), 1, 200) : 1; } }

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

    [Space] [Space] [Header ("Random Position Settings")] [Space]

    [SerializeField] private Vector2 minimumPlaneAxis;
    [SerializeField] private Vector2 maximumPlaneAxis;

    [Space] [Space] [Header("Distance Scaler Settings")] [Space]

    [Range(1, 500)] [SerializeField] private float scalingValue;
    [Range(0, 15)] [SerializeField] private float minimumRange;
    [Range(5, 45)] [SerializeField] private float maximumRange;
    private float minimumDistance;

    [Space] [Space] [Header ("Additional Settings")] [Space]

    [SerializeField] private GameObject[] enemies;
    [SerializeField] private GameObject altar;
    [SerializeField] private Transform player;

    [Space]

    [Space] [Space] [Header("Billboard Settings")] [Space]

    [SerializeField] private TMP_Text billBoard;

    private GameObject[] enemiesSpawned;
    private GameObject[] altarSpawned;

    private Vector2[] usedPositions;
    private Vector2 currentPosition;

    #endregion

    protected override void Awake()
    {
        // catch the error if ever the assigned axis is critical
        if (willCrash)
        {
            Debug.LogError("Double check the spawn axis then run again");
            return;
        }

        // scale the distancing between spawned objects depending on the spawn count
        minimumDistance = Mathf.Clamp(scalingValue / round, minimumRange, maximumRange);

        // set a length for the array of the positions
        usedPositions = new Vector2[round];
        usedPositions[0] = player.position;

        // initialize the array of spawned enemies
        enemiesSpawned = new GameObject[round];

        // match the numbers of spells to the number of enemies on board
        spawnCount = enemiesSpawned.Length;

        // call the base awak function for the spells
        base.Awake();


        for (int current = 0; current < enemiesSpawned.Length; current++)
        {
            // catch null if ever you forgot to input the spawnees
            if (enemies == null) return;

            // instantiate random enemy sprites
            enemiesSpawned[current] = Instantiate (enemies[Random.Range(0, enemies.Length)], this.transform);

            // initialize variables essential to the enemies
            EnemyBehavior enemy = enemiesSpawned[current].GetComponentInChildren<EnemyBehavior>();
            if (enemy == null) return;
            enemy.spawner = this;
            enemy.target = player;

            // generate random position for enemies to spawn into
            enemiesSpawned[current].transform.position = RandomVectorGenerator(current);
        }

        altarSpawned = new GameObject[round];

        for (int current = 0; current < altarSpawned.Length; current++)
        {
            altarSpawned[current] = Instantiate (altar, this.transform);
            altarSpawned[current].transform.position = RandomVectorGenerator(current);
        }

        Objectives.totalCoins = altarSpawned.Length;
        Objectives.totalEnemies = enemiesSpawned.Length;

        // print round number
        if (billBoard == null) return;
        billBoard.SetText(round < 200 ? ("Round " + round) : "Final Round");
    }

    private Vector2 RandomVectorGenerator(int current)
    {
        // initialize values
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

    /*private float ScaleWithLevel(float coordinate)
    {
        // --- fix later --- //

        int minX = (int)Mathf.Round(min.x - ScaleWithLevel(min.x));
        int minY = (int)Mathf.Round(min.y - ScaleWithLevel(min.y));
        int maxX = (int)Mathf.Round(max.x + ScaleWithLevel(max.x)) + 1;
        int maxY = (int)Mathf.Round(max.y) + 1;
        return coordinate > 0 ? level : -level;
    }*/
}
