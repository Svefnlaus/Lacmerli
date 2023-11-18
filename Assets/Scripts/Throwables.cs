using System.Collections;
using UnityEngine;
using UnityEngine.UIElements;

public class Throwables : MonoBehaviour
{
    public static Vector2 origin;
    public static float maxDistance;
    public float damage;

    private void Update()
    {
        if (GetComponent<Rigidbody2D>().velocity.magnitude <= 0.1f) return;
        float distance = Vector2.Distance(transform.position, origin);
        if (distance > maxDistance) this.gameObject.SetActive(false);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        other.TryGetComponent<Player>(out Player player);
        if (player != null) player.TakeDamage(damage);

        other.TryGetComponent<EnemyBehavior>(out EnemyBehavior enemy);
        if (enemy != null) enemy.TakeDamage(damage);

        if (!other.CompareTag("Boss")) this.gameObject.SetActive(false);
    }
}
