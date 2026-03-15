using System.Collections;
using UnityEngine;

public class ChaseLightController : MonoBehaviour
{
    public Light[] allLights;
    public UmbrellaManChase umbrellaMan;
    public float dimAmount = 0.5f;
    public float pulseSpeed = 2f;
    public float pulseIntensity = 0.3f;
    public float radiusAroundUmbrella = 3f;
    private float[] originalIntensity;

    void Awake()
    {
        originalIntensity = new float[allLights.Length];
        for (int i = 0; i < allLights.Length; i++)
        {
            if (allLights[i] != null)
                originalIntensity[i] = allLights[i].intensity;
        }
    }

    public void StartChaseLights()
    {
        StopAllCoroutines();
        StartCoroutine(LightChaseRoutine());
    }

    IEnumerator LightChaseRoutine()
    {
        while (umbrellaMan != null && umbrellaMan.gameObject.activeInHierarchy)
        {
            for (int i = 0; i < allLights.Length; i++)
            {
                if (allLights[i] == null) continue;

                float baseIntensity = originalIntensity[i] * dimAmount;

                float pulse = Mathf.Sin(Time.time * pulseSpeed + i) * pulseIntensity;

                if (umbrellaMan != null)
                {
                    float dist = Vector3.Distance(allLights[i].transform.position, umbrellaMan.transform.position);
                    if (dist < radiusAroundUmbrella)
                    {
                        float factor = 1f - (radiusAroundUmbrella - dist) / radiusAroundUmbrella;
                        baseIntensity *= factor;
                    }
                }

                allLights[i].intensity = Mathf.Max(0, baseIntensity + pulse);
            }

            yield return null;
        }

        for (int i = 0; i < allLights.Length; i++)
        {
            if (allLights[i] != null)
                allLights[i].intensity = originalIntensity[i];
        }
    }
}
