using UnityEngine;


[RequireComponent(typeof(Light))]
public class HighlightFlicker : MonoBehaviour
{
    public float minIntensity = 0.5f; // Минимальная яркость
    public float maxIntensity = 1.5f; // Максимальная яркость
    public float flickerSpeed = 0.1f; // Как часто меняется яркость (чем меньше, тем быстрее)

    private Light lightComponent;
    private float timer;

    void Start()
    {
        lightComponent = GetComponent<Light>();
        timer = flickerSpeed;
    }

    void Update()
    {
        // Каждые 0.1 секунды (или сколько ты укажешь)
        timer -= Time.deltaTime;
        if (timer <= 0)
        {
            // Выбираем новую случайную яркость
            lightComponent.intensity = Random.Range(minIntensity, maxIntensity);
            // Сбрасываем таймер
            timer = flickerSpeed;
        }
    }
}
