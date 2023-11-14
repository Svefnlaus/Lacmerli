using UnityEngine;

public class Coins : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;
        Objectives.coinsFound++;
        gameObject.SetActive(false);
    }
}
