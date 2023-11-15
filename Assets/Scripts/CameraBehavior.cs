using UnityEngine;

public class CameraBehavior : MonoBehaviour
{
    [SerializeField] private float followSpeed;
    [SerializeField] private float zoomSpeed;

    public static Transform target;
    
    public static float zoomSize;
    
    private float zoom;
    private float zoomVelocity;

    private void Awake() { zoomSize = 0.1f; }

    private void LateUpdate()
    {
        if (target == null) return;
        FollowPlayer();
        Zoom();
    }

    private void Zoom()
    {
        if (zoomSize == zoom) return;
        zoom = Mathf.SmoothDamp(zoom, zoomSize, ref zoomVelocity, 0.01f, zoomSpeed * (zoom > zoomSize ? 5 : 1));
        GetComponent<Camera>().orthographicSize = zoom;
    }

    private void FollowPlayer()
    {
        if (target.position == transform.position) return;
        Vector3 targetPosition = target.position;
        targetPosition.z = transform.position.z;
        transform.position = Vector3.Lerp(transform.position, targetPosition, followSpeed * Time.deltaTime);
    }
}
