using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HealthBarManager : MonoBehaviour
{
    public Slider slider;
    public Gradient gradient;
    public Image fill;

    [Header("Settings")]
    [SerializeField] private float speed = 2f;
    private float currentSlider;
    private Coroutine sliderChange;
    private float currentHealth;

    public void SetMaxHealth(float health)
    {
        slider.maxValue = health;
        slider.value = health;

        fill.color = gradient.Evaluate(1f);
    }

    public void SetHealth(float health)
    {
        currentHealth = health;
        if(currentSlider == 0)
        {
            currentSlider = slider.maxValue;
        }
        if(sliderChange != null)
        {
            return;
        }

        sliderChange = StartCoroutine(SliderChange());
    }

    IEnumerator SliderChange()
    {
        while (true)
        {
            float sliderSpeed = (currentHealth - currentSlider) * speed;

            currentSlider += Time.deltaTime * sliderSpeed;

            if (currentSlider <= currentHealth)
            {
                currentSlider = currentHealth;
                yield break;
            }
            slider.value = currentSlider;
            fill.color = gradient.Evaluate(slider.normalizedValue);
            yield return null;
        }
    }
}
