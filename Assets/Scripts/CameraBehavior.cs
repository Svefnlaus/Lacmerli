using Cinemachine;
using System.Collections;
using UnityEngine;
using static Cinemachine.CinemachineTriggerAction.ActionSettings;

public class CameraBehavior : MonoBehaviour
{
    [SerializeField] private float followSpeed;
    [SerializeField] private float zoomSpeed;

    public static Transform target;
    public static float zoomSize;

    public static bool canMove;
    public static CinemachineBrain cmBrain;

    private float zoom;
    private float zoomVelocity;

    private Vector3 followVelocity;

    private void Awake()
    {
        TryGetComponent<CinemachineBrain>(out CinemachineBrain brain);
        canMove = brain == null;
        if (brain != null) cmBrain = brain;
        zoomSize = 0.1f;
    }

    private void LateUpdate()
    {
        if (target == null || !canMove) return;
        if (cmBrain != null) canMove = !cmBrain.enabled;

        FollowPlayer();
        Zoom();
    }

    private void Zoom()
    {
        if (zoomSize == zoom) return;
        zoom = Mathf.SmoothDamp(zoom, zoomSize, ref zoomVelocity, 0.01f, zoomSpeed * (zoom > zoomSize ? 3 : 1));
        GetComponent<Camera>().orthographicSize = zoom;
    }

    private void FollowPlayer()
    {
        if (target.position == transform.position) return;
        Vector3 targetPosition = target.position;
        targetPosition.z = transform.position.z;
        transform.position = Vector3.SmoothDamp(transform.position, targetPosition, ref followVelocity, 0.01f, followSpeed);
    }

    public static IEnumerator CameraShake(float shakeDuration, float shakeIntensity)
    {
        if(cmBrain != null) cmBrain.enabled = false;
        canMove = false;
        Vector3 originalPosition = Camera.main.transform.position;
        float timeElapsed = 0;
        while (timeElapsed < shakeDuration)
        {
            float x = Random.Range(-1, 1) * shakeIntensity + originalPosition.x;
            float y = Random.Range(-1, 1) * shakeIntensity + originalPosition.y;
            float z = originalPosition.z;

            Vector3 newPosition = new Vector3(x, y, z);
            Camera.main.transform.localPosition = newPosition;

            timeElapsed += Time.deltaTime;
            yield return null;
        }
        Camera.main.transform.position = originalPosition;
        canMove = true;
        if (cmBrain != null) cmBrain.enabled = true;
        yield return null;
    }
}
