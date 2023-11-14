using UnityEngine;
using UnityEngine.Rendering.Universal;

public class LightingBehavior : MonoBehaviour
{
    public static float targetSize;
    private float size;
    private float sizeVelocity;
    [SerializeField] private float maxSpeed;

    private void Awake()
    {
        targetSize = 0;
    }

    private void LateUpdate()
    {
        size = Mathf.SmoothDamp(size, targetSize, ref sizeVelocity, 0.01f, maxSpeed);
        GetComponent<Light2D>().pointLightOuterRadius = size;
    }
}
