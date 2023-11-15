using UnityEngine;

public class Throwables : MonoBehaviour
{
    public static Vector2 origin;
    public static float maxDistance;
    private int damage;

    private void Update()
    {
        if (GetComponent<Rigidbody2D>().velocity.magnitude <= 0.1f) return;
        float distance = Vector2.Distance(transform.position, origin);
        if (distance > maxDistance) this.gameObject.SetActive(false);
    }

    private void OnCollisionEnter2D(Collision2D other)
    {
        if (other.gameObject.CompareTag("Player")) DamagePlayer(other);
        this.gameObject.SetActive(false);
    }

    private void DamagePlayer(Collision2D other)
    {
        other.gameObject.TryGetComponent<Player>(out Player player);
        if (player == null) return;
        damage = Random.Range(-10, 50);
        player.TakeDamage(damage);
    }
}
