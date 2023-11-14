using UnityEngine;
using UnityEngine.UI;

public class HealthBar : MonoBehaviour
{
    [SerializeField] private Canvas canvas;
    [SerializeField] private Gradient gradient;
    [SerializeField] private Image bar;

    private Slider percentage;

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
        // read current health percentage
        percentage.value = target;

        // transform color depending on the health percentage
        bar.color = gradient.Evaluate(percentage.normalizedValue);
    }
}
