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

    private int coinsLeft { get { return totalCoins - coinsFound; } }
    private int enemiesLeft { get { return totalEnemies - enemiesSlain; } }

    private void Awake()
    {
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
        if (gameMode != "Arcade" || (enemiesLeft == enemyCount && coinsLeft == coinCount)) return;

        Debug.Log("test");

        objectivesBillboard.SetText(
            "Objectives:\n\n" +
            (enemiesLeft != 0 ? "Kill " + enemiesLeft  + " enemies\n" : "") +
            (coinsLeft != 0 ? "Collect " + coinsLeft + " coins" : ""));

        if (enemiesCleared && coinsCollected) system.LoadScene(0);

        enemyCount = enemiesLeft;
        coinCount = coinsLeft;
    }

    private void SurvivalMode()
    {
        if (gameMode != "Survival") return;

        objectivesBillboard.SetText(
            "Objectives:\n\n" +
            "Collect " + totalCoins + " coins");

        // prevent from looping
        if (!coinsCollected) return;
        objectiveCleared = true;

        // ensure the set round is concatinated
        int round = PlayerPrefs.GetInt("Round") + 1;
        PlayerPrefs.SetInt("Round", round);
        system.LoadScene(round <= 10 ? 3 : 10 < round && round < 50 ? 4 : 5);
    }
}
