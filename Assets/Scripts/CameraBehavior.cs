using UnityEngine;

public class CameraBehavior : MonoBehaviour
{
    [SerializeField] private float speed;
    [SerializeField] private float maxSpeed;

    public static Transform target;
    
    public static float zoomSize;
    
    private float zoom;
    private float zoomVelocity;

    private void Awake()
    {
        zoomSize = 0.1f;
    }

    private void LateUpdate()
    {
        FollowPlayer();
        Zoom();
    }

    private void Zoom()
    {
        zoom = Mathf.SmoothDamp(zoom, zoomSize, ref zoomVelocity, 0.01f, maxSpeed);
        GetComponent<Camera>().orthographicSize = zoom;
    }

    private void FollowPlayer()
    {
        Vector3 targetPosition = target.position;
        targetPosition.z = transform.position.z;
        transform.position = Vector3.Lerp(transform.position, targetPosition, speed * Time.deltaTime);
    }
}
