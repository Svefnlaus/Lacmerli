using UnityEngine;
using UnityEngine.UI;

public class FillBar : MonoBehaviour
{
    [SerializeField] private Gradient gradient;
    [SerializeField] private Image fill;

    public Slider slider;
    private float updateVelocity;

    private void Awake()
    {
        slider = GetComponent<Slider>();
    }

    private void Start()
    {
        slider.value = 1;
        fill.color = gradient.Evaluate(1);
    }

    public void UpdateFillBar(float value)
    {
        slider.value = value;
        fill.color = gradient.Evaluate(value);

        /*if (value == 0) slider.value = 0;

        float update = slider.value;

        while (update != value)
        {
            update = Mathf.SmoothDamp(update, value, ref updateVelocity, .001f, 0.25f);
        }*/
    }
}
