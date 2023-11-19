using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class HealthBar : MonoBehaviour
{
    [SerializeField] private Canvas canvas;
    [SerializeField] private Gradient gradient;
    [SerializeField] private Image bar;

    public Slider percentage;
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

    public void UpdateCurrentHealth(float targetHealth)
    {
        if (this == null) return;
        percentage.value = targetHealth;

        // transform color depending on the health percentage
        bar.color = gradient.Evaluate(percentage.normalizedValue);
    }

    // needs fixing
    /* private float currentHealth = percentage.maxValue;
     * private IEnumerator SmoothHealthUpdate(float targetHealth)
    {
        currentHealth = Mathf.Clamp(currentHealth - targetHealth, 0, percentage.maxValue);

        // read current health percentage
        while (percentage.value != currentHealth)
        {
            // smoother damage display
            percentage.value = Mathf.MoveTowards(percentage.value, currentHealth, 60 * Time.deltaTime);

            // transform color depending on the health percentage
            bar.color = gradient.Evaluate(percentage.normalizedValue);

            yield return null;
            // yield return new WaitForSeconds(Time.deltaTime);
        }
    }*/
}
