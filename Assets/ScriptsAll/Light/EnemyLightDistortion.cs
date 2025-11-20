using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class EnemyLightDistortion : MonoBehaviour
{
    [Header("Settings")]
    public Transform enemy;                  // Человек с зонтом (root)
    public float effectRadius = 6f;          // Радиус вокруг зонтика, где лампы “умирают”
    public float burstChance = 0.3f;         // Шанс вспышки в секунду
    public float burstDuration = 0.1f;       // Длительность вспышки
    public float offIntensityMultiplier = 0.1f; // Насколько лампы тусклые рядом с зонтиком

    [Header("Lights to affect")]
    public List<Light> sceneLights = new List<Light>();

    private Dictionary<Light, float> originalIntensities = new Dictionary<Light, float>();

    void Start()
    {
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
            // Если враг отсутствует или ОТКЛЮЧЕН, просто возвращаем свет в норму
            if (enemy == null || !enemy.gameObject.activeInHierarchy)
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

            // Если враг активен – применяем эффект
            foreach (var lamp in sceneLights)
            {
                if (lamp == null) continue;

                float dist = Vector3.Distance(enemy.position, lamp.transform.position);

                if (dist <= effectRadius)
                {
                    // Основной режим: лампа почти не горит
                    if (originalIntensities.TryGetValue(lamp, out float orig))
                        lamp.intensity = orig * offIntensityMultiplier;

                    // Вспышки
                    if (Random.value < burstChance * Time.deltaTime)
                    {
                        StartCoroutine(Burst(lamp));
                    }
                }
                else
                {
                    // Вдали – нормальный свет
                    if (originalIntensities.TryGetValue(lamp, out float orig))
                        lamp.intensity = orig;
                }
            }

            yield return null;
        }
    }

    IEnumerator Burst(Light lamp)
    {
        if (lamp == null) yield break;

        if (!originalIntensities.TryGetValue(lamp, out float orig))
            yield break;

        lamp.intensity = orig;
        yield return new WaitForSeconds(burstDuration);
        lamp.intensity = orig * offIntensityMultiplier;
    }
}
