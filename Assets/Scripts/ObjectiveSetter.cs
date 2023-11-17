using UnityEngine;

public class ObjectiveSetter : MonoBehaviour
{
    [SerializeField] private int numberOfEnemiesToDefeat;
    [SerializeField] private int numberOfCoinsToCollect;

    private void Awake()
    {
        Objectives.totalEnemies = numberOfEnemiesToDefeat;
        Objectives.totalCoins = numberOfCoinsToCollect;
    }
}
