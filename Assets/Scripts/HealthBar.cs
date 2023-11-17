using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class HealthBar : MonoBehaviour
{
    [SerializeField] private Canvas canvas;
    [SerializeField] private Gradient gradient;
    [SerializeField] private Image bar;
    [SerializeField] private float updateSpeed;

    private Slider percentage;
    private float updateVelocity;

    private void Awake()
    {
        percentage = GetComponent<Slider>();
        canvas.worldCamera = Camera.main;
    }

    public void SetMaxHealth(float health)
    {
        // set the slider to max health
        percentage.maxValue = health;

        // give the obejct a full hp
        percentage.value = health;

        // adjust the gradient
        bar.color = gradient.Evaluate(1);
    }

    public void UpdateCurrentHealth(float target)
    {
        if (this == null) return;
        StartCoroutine(SmoothHealthUpdate(target));
    }

    private IEnumerator SmoothHealthUpdate(float target)
    {
        // read current health percentage
        while (percentage.value != target)
        {
            percentage.value = Mathf.SmoothDamp(percentage.value, target, ref updateVelocity, 0.01f, updateSpeed);
            yield return null;
        }

        // transform color depending on the health percentage
        bar.color = gradient.Evaluate(percentage.normalizedValue);
    }
}
