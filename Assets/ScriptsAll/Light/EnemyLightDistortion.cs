using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class EnemyLightDistortion : MonoBehaviour
{
    [Header("Состояние погони")]
    [Tooltip("Включается при старте погони")]
    public bool chaseActive = false;

    [Header("Настройки рядом с врагом")]
    public Transform enemy;                  // Человек с зонтом (root)
    public float effectRadius = 6f;          // Радиус вокруг зонта, где лампы “умирают”
    public float burstChance = 0.3f;         // Шанс вспышки в секунду
    public float burstDuration = 0.1f;       // Длительность вспышки
    public float offIntensityMultiplier = 0.1f; // Насколько лампы тусклые рядом с зонтиком

    [Header("Глобальное пульсирование при погоне")]
    [Tooltip("Минимальный множитель яркости при пульсации (0.6 = 60%)")]
    public float globalPulseMin = 0.6f;
    [Tooltip("Максимальный множитель яркости при пульсации (1 = 100%)")]
    public float globalPulseMax = 1.0f;
    [Tooltip("Скорость пульсации")]
    public float globalPulseSpeed = 2f;

    [Header("Какие лампы затрагиваем")]
    public List<Light> sceneLights = new List<Light>();

    private Dictionary<Light, float> originalIntensities = new Dictionary<Light, float>();

    // Вызвать из квеста/AI
    public void SetChaseActive(bool value)
    {
        chaseActive = value;
    }

    void Start()
    {
        // Запоминаем исходную яркость всех ламп
        foreach (var lamp in sceneLights)
        {
            if (lamp != null && !originalIntensities.ContainsKey(lamp))
                originalIntensities[lamp] = lamp.intensity;
        }

        StartCoroutine(ProcessLights());
    }

    IEnumerator ProcessLights()
    {
        while (true)
        {
            // Если погони нет или враг не активен – возвращаем свет в норму
            if (!chaseActive || enemy == null || !enemy.gameObject.activeInHierarchy)
            {
                foreach (var lamp in sceneLights)
                {
                    if (lamp == null) continue;
                    if (originalIntensities.TryGetValue(lamp, out float orig))
                        lamp.intensity = orig;
                }

                yield return null;
                continue;
            }

            // --- Погоня активна ---

            // Общий коэффициент пульсации (для всего света)
            float t = Mathf.PingPong(Time.time * globalPulseSpeed, 1f); // 0..1
            float pulse = Mathf.Lerp(globalPulseMin, globalPulseMax, t); // min..max

            foreach (var lamp in sceneLights)
            {
                if (lamp == null) continue;
                if (!originalIntensities.TryGetValue(lamp, out float orig))
                    continue;

                float dist = Vector3.Distance(enemy.position, lamp.transform.position);

                // Базовая яркость с учетом глобальной пульсации
                float targetIntensity = orig * pulse;

                if (dist <= effectRadius)
                {
                    // Рядом с зонтиком – лампа почти "умирает"
                    targetIntensity = orig * offIntensityMultiplier;

                    // И иногда вспыхивает
                    if (Random.value < burstChance * Time.deltaTime)
                        StartCoroutine(Burst(lamp));
                }

                lamp.intensity = targetIntensity;
            }

            yield return null;
        }
    }

    IEnumerator Burst(Light lamp)
    {
        if (lamp == null) yield break;
        if (!originalIntensities.TryGetValue(lamp, out float orig))
            yield break;

        float offValue = orig * offIntensityMultiplier;

        // Короткая вспышка до полной яркости
        lamp.intensity = orig;
        yield return new WaitForSeconds(burstDuration);
        lamp.intensity = offValue;
    }
}
