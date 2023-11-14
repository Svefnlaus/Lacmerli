using TMPro;
using UnityEngine;

public class CoinCounter : MonoBehaviour
{
    private int currentCount;
    private TMP_Text billBoard;

    private void Awake()
    {
        billBoard = GetComponent<TMP_Text>();
        currentCount = 0;
    }

    private void Update()
    {
        if (billBoard == null || currentCount == Objectives.coinsFound) return;
        billBoard.SetText(Objectives.coinsFound + "");
        currentCount = Objectives.coinsFound;
    }
}
