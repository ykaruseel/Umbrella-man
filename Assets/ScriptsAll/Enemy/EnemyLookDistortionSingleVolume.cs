using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class EnemyLookDistortionSingleVolume : MonoBehaviour
{
    [Header("References")]
    public Transform enemy;      // Человек с зонтом (root его объекта)
    public Volume mainVolume;    // твой основной Volume (тот, что всегда включен)

    [Header("Detection")]
    public float maxDistance = 20f;   // дальше эффекта нет вообще
    [Range(0f, 90f)]
    public float maxAngle = 70f;      // угол от центра, в пределах которого эффект ещё есть
    public float lerpSpeed = 3f;      // скорость плавного перехода

    [Header("Occlusion")]
    public LayerMask occlusionMask = ~0; // чем считать перекрытия (Default + стены и т.п.)

    private Camera cam;

    // Ссылки на эффекты
    Vignette vignette;
    ChromaticAberration chromatic;
    LensDistortion lens;
    FilmGrain grain;
    ColorAdjustments colorAdj;

    // БАЗОВЫЕ значения (какими игра выглядит обычно)
    float baseVignette;
    float baseVignetteSmoothness;
    float baseExposure;
    float baseContrast;
    float baseChromatic;
    float baseLensIntensity;
    float baseGrainIntensity;

    float target = 0f;
    float current = 0f;

    void Awake()
    {
        cam = GetComponent<Camera>();
        if (cam == null) cam = Camera.main;

        if (mainVolume == null)
        {
            Debug.LogError("[EnemyLookDistortion] mainVolume не назначен!");
            enabled = false;
            return;
        }

        // Достаём эффекты из ПРОФИЛЯ
        var profile = mainVolume.profile;

        profile.TryGet(out vignette);
        profile.TryGet(out chromatic);
        profile.TryGet(out lens);
        profile.TryGet(out grain);
        profile.TryGet(out colorAdj);

        // Сохраняем базовые значения (то, как ты хочешь, чтобы игра выглядела обычно)
        if (vignette != null)
        {
            baseVignette = vignette.intensity.value;
            baseVignetteSmoothness = vignette.smoothness.value;
        }

        if (colorAdj != null)
        {
            baseExposure = colorAdj.postExposure.value;
            baseContrast = colorAdj.contrast.value;
        }

        if (chromatic != null)
            baseChromatic = chromatic.intensity.value;

        if (lens != null)
            baseLensIntensity = lens.intensity.value;

        if (grain != null)
            baseGrainIntensity = grain.intensity.value;
    }

    // ... остальной код тот же до Update()

    void Update()
    {
        if (cam == null || enemy == null) return;

        Vector3 toEnemy = enemy.position - cam.transform.position;
        float distance = toEnemy.magnitude;

        float visibilityFactor = 0f;

        if (distance <= maxDistance)
        {
            Vector3 dirToEnemy = toEnemy.normalized;
            float angle = Vector3.Angle(cam.transform.forward, dirToEnemy);

            if (angle <= maxAngle)
            {
                bool blocked = false;
                if (Physics.Raycast(cam.transform.position, dirToEnemy, out RaycastHit hit, maxDistance, occlusionMask))
                {
                    if (hit.transform != enemy && !hit.transform.IsChildOf(enemy))
                        blocked = true;
                }

                if (!blocked)
                {
                    float tAngle = Mathf.Clamp01(angle / maxAngle);
                    float angleFactor = 1f - tAngle;

                    float tDist = Mathf.Clamp01(distance / maxDistance);
                    float distanceFactor = 1f - tDist * 0.5f;

                    visibilityFactor = angleFactor * distanceFactor;
                }
            }
        }

        target = visibilityFactor;
        current = Mathf.Lerp(current, target, Time.deltaTime * lerpSpeed);

        // ===== более мягкие максимумы =====
        if (vignette != null)
        {
            // было ~0.7, сделаем максимум 0.5
            vignette.intensity.value = Mathf.Lerp(baseVignette, 0.5f, current);
            vignette.smoothness.value = Mathf.Lerp(baseVignetteSmoothness, 0.75f, current);
        }

        if (colorAdj != null)
        {
            // экспозиция падала до -1.5, сделаем -0.7
            colorAdj.postExposure.value = Mathf.Lerp(baseExposure, -0.7f, current);
            // контраст до 25, пусть будет 12
            colorAdj.contrast.value = Mathf.Lerp(baseContrast, 12f, current);
        }

        if (chromatic != null)
        {
            // хроматика до 1, сделаем 0.6
            chromatic.intensity.value = Mathf.Lerp(baseChromatic, 0.6f, current);
        }

        if (lens != null)
        {
            // дисторшн до -0.7, сделаем -0.35
            lens.intensity.value = Mathf.Lerp(baseLensIntensity, -0.35f, current);
        }

        if (grain != null)
        {
            // шум до 1, сделаем 0.6
            grain.intensity.value = Mathf.Lerp(baseGrainIntensity, 0.6f, current);
        }

#if UNITY_EDITOR
    Debug.DrawRay(cam.transform.position, cam.transform.forward * 3f, Color.cyan);
#endif
    }
}
