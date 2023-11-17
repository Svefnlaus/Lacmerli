using UnityEngine;
using UnityEngine.Rendering.Universal;

public class LightingBehavior : MonoBehaviour
{
    public static float targetSize;
    private float size;
    private float sizeVelocity;
    [SerializeField] private float maxSpeed;

    private void Awake() { targetSize = 0; }

    private void LateUpdate()
    {
        if (size == targetSize) return;
        size = Mathf.SmoothDamp(size, targetSize, ref sizeVelocity, 0.01f, maxSpeed * (size > targetSize ? 2 : 1));
        GetComponent<Light2D>().pointLightOuterRadius = size;
    }
}
