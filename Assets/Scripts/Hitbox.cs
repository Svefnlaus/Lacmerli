using UnityEngine;

public class Hitbox : MonoBehaviour
{
    [SerializeField] private float damage;
    private void OnTriggerStay2D(Collider2D other)
    {
        // try and get the said components, don't get just try to be more flexible
        other.TryGetComponent<EnemyBehavior>(out EnemyBehavior enemy);
        other.TryGetComponent<Player>(out Player player);

        // damage either the player or the enemy depending on what you got on the component reader
        DamageEnemy(enemy);
        DamagePlayer(player);
    }

    private void DamageEnemy(EnemyBehavior enemy)
    {
        if (enemy == null) return;
        enemy.TakeDamage(damage);
    }

    private void DamagePlayer(Player player)
    {
        if (player == null || CompareTag("Aura")) return;
        player.TakeDamage(damage);
    }
}
