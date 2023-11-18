using System.Collections;
using UnityEngine;

public class AbberatingThunder : MonoBehaviour
{
    public float creepingDamage;
    public float quakeDuration;
    public float quakeIntensity;

    private void OnTriggerStay2D(Collider2D other)
    {
        other.TryGetComponent<Player>(out Player player);
        if (player != null) player.TakeDamage(creepingDamage);

        other.TryGetComponent<EnemyBehavior>(out EnemyBehavior enemy);
        if (enemy != null) enemy.TakeDamage(creepingDamage);
    }

    public void QuakeOnDrop()
    {
        StartCoroutine(CameraBehavior.CameraShake(quakeDuration, quakeIntensity));
    }
}
