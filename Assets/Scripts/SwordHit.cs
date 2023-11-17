using UnityEngine;

public class SwordHit : MonoBehaviour
{
    [Range (0.01f, 100)] [SerializeField] private float damage;

    private void OnTriggerEnter2D(Collider2D other)
    {
        other.TryGetComponent<EnemyBehavior>(out EnemyBehavior enemy);
        other.TryGetComponent<BossBehavior>(out BossBehavior boss);

        if (enemy != null) enemy.TakeDamage(damage);

        // slashes has triple damage on boss
        if (boss != null) boss.TakeDamage(damage * 3);
    }
}
