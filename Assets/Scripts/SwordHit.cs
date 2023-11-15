using UnityEngine;

public class SwordHit : MonoBehaviour
{
    [Range (0.01f, 100)] [SerializeField] private float damage;

    private void OnTriggerEnter2D(Collider2D other)
    {
        other.TryGetComponent<EnemyBehavior>(out EnemyBehavior enemy);
        if (enemy == null) return;
        enemy.TakeDamage(damage);
    }
}
