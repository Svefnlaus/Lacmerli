using TMPro;
using Unity.VisualScripting;
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

    private string gameMode;
    private bool objectiveCleared;

    private bool enemiesCleared { get { return enemiesSlain == totalEnemies; } }
    private bool coinsCollected { get { return coinsFound == totalCoins; } }
    private bool rewriteObjectiveBoard { get { return enemiesLeft == enemyCount && coinsLeft == coinCount ? false : true; } }
    private int coinsLeft { get { return totalCoins - coinsFound; } }
    private int enemiesLeft { get { return totalEnemies - enemiesSlain; } }

    private void Awake()
    {
        if (system == null) return;
        coinsFound = 0;
        enemiesSlain = 0;
        enemyCount = totalEnemies;
        coinCount = totalCoins;
        objectiveCleared = false;
        gameMode = PlayerPrefs.GetString("GameMode");
    }

    private void Update()
    {
        ArcadeMode();
        if (objectiveCleared) return;
        SurvivalMode();   
    }

    private void ArcadeMode()
    {
        // prevent from looping
        if (gameMode != "Arcade" || !rewriteObjectiveBoard) return;

        enemyCount = enemiesLeft;
        coinCount = coinsLeft;

        UpdateObjectivesBoard();

        if (!coinsCollected || !enemiesCleared) return;
        int nextScene = PlayerPrefs.GetInt("PreviousScene") + 1;
        if (nextScene == 4) nextScene = 10;
        system.LoadScene(nextScene);
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
        system.LoadScene(nextScene);
    }

    private void UpdateObjectivesBoard()
    {
        objectivesBillboard.SetText(
            "Objectives:\n\n" +

            (enemiesLeft != 0 ? "Kill " + enemiesLeft +
            (enemiesLeft != totalEnemies ? " more " : "") +
            " enem" + (enemiesLeft > 1 ? "ies" : "y") + "\n" : "") +

            (coinsLeft != 0 ? "Collect " + coinsLeft +
            (coinsLeft != totalCoins ? " more " : "") +
            " coin" + (coinsLeft > 1 ? "s" : "") : ""));
    }
}
