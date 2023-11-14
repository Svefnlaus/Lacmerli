using System.Collections;
using UnityEngine;

public class Magic : MonoBehaviour
{
    [SerializeField] private GameObject sprite;
    [SerializeField] private GameObject hitbox;
    [SerializeField] private GameObject secondWave;

    [SerializeField] private Transform[] points;
    
    [Range(1, 500)]     [SerializeField] private float maxSpeed;
    [Range(0.01f, 1)]   [SerializeField] private float smoothTime;
    [Range(0.01f, 1)]   [SerializeField] private float shootDelay;
    [Range(0.01f, 1)]   [SerializeField] private float hitboxDelay;

    private Vector2 currentVelocity;
    private bool isMoving;

    private void Update()
    {
        if (isMoving) return;
        StartCoroutine(Motion());
    }

    private IEnumerator Motion()
    {
        if (secondWave != null) secondWave.SetActive(true);
        hitbox.SetActive(false);
        sprite.SetActive(false);
        isMoving = true;

        // appear only after the effects is done
        yield return new WaitForSeconds(shootDelay);
        sprite.SetActive(true);

        // turn collider on once it is supposed to move
        yield return new WaitForSeconds(hitboxDelay);
        hitbox.SetActive(true);
        hitbox.transform.position = points[0].position;

        while (hitbox.transform.position != points[1].position)
        {
            hitbox.transform.position = Vector2.SmoothDamp(hitbox.transform.position, points[1].position, ref currentVelocity, smoothTime, maxSpeed);
            yield return null;
        }

        isMoving = false;
        gameObject.SetActive(false);

        yield return null;
    }
}
