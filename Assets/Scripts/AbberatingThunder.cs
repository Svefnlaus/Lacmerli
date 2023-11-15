using System.Collections;
using UnityEngine;

public class AbberatingThunder : MonoBehaviour
{
    [Range(0.1f, 50)][SerializeField] private float creepingDamage;

    private void OnTriggerStay2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;
        other.TryGetComponent<Player>(out Player player);
        if (player == null) return;
        player.TakeDamage(creepingDamage);
    }
}
