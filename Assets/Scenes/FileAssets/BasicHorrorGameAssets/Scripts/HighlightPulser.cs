using UnityEngine;

// Требуем, чтобы на этом объекте был компонент Light
[RequireComponent(typeof(Light))]
public class HighlightPulser : MonoBehaviour
{
    public float minIntensity = 0.5f;
    public float maxIntensity = 1.5f;
    public float pulseSpeed = 1f;

    private Light lightComponent;

    void Start()
    {
        lightComponent = GetComponent<Light>();
    }

    void Update()
    {
        // Используем синус для создания плавной пульсации
        float targetIntensity = Mathf.Lerp(minIntensity, maxIntensity, (Mathf.Sin(Time.time * pulseSpeed) + 1f) / 2f);
        lightComponent.intensity = targetIntensity;
    }
}
