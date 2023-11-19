using System.Collections;
using TMPro;
using UnityEngine;

public class Objectives : MonoBehaviour
{
    public static int totalCoins;
    public static int coinsFound;
    private int enemyCount;

    public static int totalEnemies;
    public static int enemiesSlain;
    private int coinCount;

    [SerializeField] private TMP_Text objectivesBillboard;
    [SerializeField] private SceneLoader system;

    [SerializeField] private float loadDelay;

    private string gameMode;
    private bool objectiveCleared;

    private bool enemiesCleared { get { return enemiesSlain == totalEnemies; } }
    private bool coinsCollected { get { return coinsFound == totalCoins; } }
    private bool rewriteObjectiveBoard { get { return enemiesLeft == enemyCount && coinsLeft == coinCount ? false : true; } }
    private int coinsLeft { get { return totalCoins - coinsFound; } set { } }
    private int enemiesLeft { get { return totalEnemies - enemiesSlain; } set { } }

    private void Start()
    {
        if (system == null) return;
        coinsFound = 0;
        enemiesSlain = 0;
        enemyCount = totalEnemies;
        coinCount = totalCoins;
        objectiveCleared = false;
        gameMode = PlayerPrefs.GetString("GameMode");

        UpdateObjectivesBoard();
    }

    private void Update()
    {
        if (objectiveCleared) return;
        ClampValues();
        ArcadeMode();
        SurvivalMode();
        BossMap();
    }

    private void ClampValues()
    {
        if (coinsLeft <= 0) coinsLeft = 0;
        if (enemiesLeft <= 0) enemiesLeft = 0;
        if (coinsFound >= totalCoins) coinsFound = totalCoins;
        if (enemiesSlain >= totalEnemies) enemiesSlain = totalEnemies;
    }

    private void BossMap()
    {
        if (gameMode != "Boss" || !rewriteObjectiveBoard) return;

        enemyCount = enemiesLeft;
        coinCount = coinsLeft;

        UpdateObjectivesBoard();

        if (!coinsCollected || !enemiesCleared) return;
        objectiveCleared = true;

        // load credits when finished
        system.LoadScene(9);
    }

    private void ArcadeMode()
    {
        // prevent from looping
        if (gameMode != "Arcade" || !rewriteObjectiveBoard) return;

        enemyCount = enemiesLeft;
        coinCount = coinsLeft;

        UpdateObjectivesBoard();

        if (!coinsCollected || !enemiesCleared) return;
        objectiveCleared = true;

        int nextScene = PlayerPrefs.GetInt("PreviousScene") + 1;
        if (nextScene == 4) PlayerPrefs.SetString("GameMode", "Boss");

        StartCoroutine(SmoothTransition(nextScene));
    }

    private void SurvivalMode()
    {
        if (gameMode != "Survival" || !rewriteObjectiveBoard) return;

        enemyCount = enemiesLeft;
        coinCount = coinsLeft;

        UpdateObjectivesBoard();

        // prevent from looping
        if (!coinsCollected || !enemiesCleared) return;
        objectiveCleared = true;

        // ensure the set round is concatinated
        int round = PlayerPrefs.GetInt("Round") + 1;
        PlayerPrefs.SetInt("Round", round);
        int nextScene = round <= 10 ? 5 : 
            10 < round && round < 50 ? 6 : 
            50 < round && round <= 200 ? 7 : 9;

        SmoothTransition(nextScene);
    }

    private void UpdateObjectivesBoard()
    {
        objectivesBillboard.SetText(
            "Objectives:\n\n" +

            (enemiesLeft > 0 ? "Kill " + enemiesLeft +
            (enemiesLeft != totalEnemies ? " more " : "") +
            " enem" + (enemiesLeft > 1 ? "ies" : "y") + "\n" : "") +

            (coinsLeft > 0 ? "Collect " + coinsLeft +
            (coinsLeft != totalCoins ? " more " : "") +
            " coin" + (coinsLeft > 1 ? "s" : "") : ""));
    }

    private IEnumerator SmoothTransition(int nextScene)
    {
        CameraBehavior.zoomSize = 1;
        LightingBehavior.targetSize = 0;
        yield return new WaitForSeconds(loadDelay);

        system.LoadScene(nextScene);
    }
}
