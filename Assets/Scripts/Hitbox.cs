using UnityEngine;

public class Hitbox : MonoBehaviour
{
    public float damage;
    private void OnTriggerEnter2D(Collider2D other)
    {
        // try and get the said components, don't get just try to be more flexible
        other.TryGetComponent<EnemyBehavior>(out EnemyBehavior enemy);
        other.TryGetComponent<BossBehavior>(out BossBehavior boss);
        other.TryGetComponent<Player>(out Player player);

        // damage either the player or the enemy depending on what you got on the component reader
        if (enemy != null) DamageEnemy(enemy);
        if (player != null) DamagePlayer(player);
        if (boss != null) DamageBoss(boss);
    }

    private void DamageBoss(BossBehavior boss) { boss.TakeDamage(damage); }
    private void DamageEnemy(EnemyBehavior enemy) { enemy.TakeDamage(damage); }

    private void DamagePlayer(Player player)
    {
        if (CompareTag("Aura")) return;
        player.TakeDamage(damage);
    }
}
