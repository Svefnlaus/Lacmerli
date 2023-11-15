using UnityEngine;

public class Throwables : MonoBehaviour
{
    public static Vector2 origin;
    public static float maxDistance;

    private void Update()
    {
        if (GetComponent<Rigidbody2D>().velocity.magnitude <= 0.1f) return;
        float distance = Vector2.Distance(transform.position, origin);
        if (distance > maxDistance) this.gameObject.SetActive(false);
    }
}
