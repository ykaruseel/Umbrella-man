using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class EnemyLookDistortionSingleVolume : MonoBehaviour
{
    [Header("References")]
    public Transform enemy;      // Человек с зонтом (root)
    public Volume mainVolume;    // Твой основной Volume (всегда активен)

    [Header("Distance thresholds")]
    public float maxDistance = 20f;   // дальше эффекта нет
    public float midDistance = 12f;   // от mid до max – дальняя зона
    public float nearDistance = 6f;   // ближняя зона

    [Header("Angle")]
    [Range(0f, 90f)]
    public float maxAngle = 70f;      // максимум, где эффект ещё есть
    [Range(0f, 45f)]
    public float nearAngle = 30f;     // “почти прямо смотришь”

    [Header("Behaviour")]
    public float lerpSpeed = 3f;      // скорость плавного перехода
    public float pulseSpeed = 4f;     // скорость пульсации на максимуме

    [Header("Occlusion")]
    public LayerMask occlusionMask = ~0; // слои стен/мебели и т.п.

    private Camera cam;

    // Эффекты
    Vignette vignette;
    ChromaticAberration chromatic;
    LensDistortion lens;
    FilmGrain grain;
    ColorAdjustments colorAdj;

    // Базовые значения (как выглядит игра обычно)
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
            Debug.LogError("[EnemyLookDistortion] mainVolume nie jest przypisany!");
            enabled = false;
            return;
        }

        var profile = mainVolume.profile;

        profile.TryGet(out vignette);
        profile.TryGet(out chromatic);
        profile.TryGet(out lens);
        profile.TryGet(out grain);
        profile.TryGet(out colorAdj);

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

    void Update()
    {
        if (cam == null || enemy == null) return;

        Vector3 toEnemy = enemy.position - cam.transform.position;
        float distance = toEnemy.magnitude;

        float visibilityFactor = 0f;
        bool isNearZone = false;

        if (distance <= maxDistance)
        {
            Vector3 dirToEnemy = toEnemy.normalized;
            float angle = Vector3.Angle(cam.transform.forward, dirToEnemy);

            // --- 1. Фактор по дистанции (работает даже спиной) ---
            float distanceZoneMultiplier;
            if (distance > midDistance)
            {
                // дальняя зона – совсем слабый “фон”
                distanceZoneMultiplier = 0.2f;
            }
            else if (distance > nearDistance)
            {
                // средняя зона – заметный эффект
                distanceZoneMultiplier = 0.4f;
            }
            else
            {
                // близко – максимум
                distanceZoneMultiplier = 1f;
            }

            // --- 2. Фактор по углу (усиление, если смотришь в его сторону) ---
            float angleFactor = 0f;

            // проверка перекрытия (чтоб через стену не работало)
            bool blocked = false;
            if (Physics.Raycast(cam.transform.position, dirToEnemy, out RaycastHit hit, maxDistance, occlusionMask))
            {
                if (hit.transform != enemy && !hit.transform.IsChildOf(enemy))
                    blocked = true;
            }

            if (!blocked)
            {
                if (angle <= maxAngle)
                {
                    // внутри конуса зрения
                    float tAngle = Mathf.Clamp01(angle / maxAngle);
                    angleFactor = 1f - tAngle; // 0..1 – чем ближе к центру, тем сильнее

                    // ближняя зона + почти прямо смотришь → для пульсации
                    if (distance <= nearDistance && angle <= nearAngle)
                        isNearZone = true;
                }
                else
                {
                    // ВНЕ конуса зрения (спиной), но ОЧЕНЬ близко → небольшой эффект присутствия
                    if (distance <= nearDistance)
                    {
                        angleFactor = 0.3f; // “он за спиной”, но без жести
                    }
                    else
                    {
                        angleFactor = 0f; // далеко и ещё и не видно – ничего
                    }
                }

                visibilityFactor = distanceZoneMultiplier * angleFactor;
            }
        }

        target = visibilityFactor;
        current = Mathf.Lerp(current, target, Time.deltaTime * lerpSpeed);

        // --- Пульсация на максимуме (ближняя зона + почти прямо смотришь) ---
        float finalFactor = current;
        if (isNearZone && current > 0.5f)
        {
            // мягкий пульс 0.85–1.05 вокруг current
            float pulse = 0.9f + 0.15f * Mathf.Sin(Time.time * pulseSpeed);
            finalFactor *= pulse;
        }

        finalFactor = Mathf.Clamp01(finalFactor);

        // --- Применяем эффекты с мягкими максимумами ---
        // --- Применяем эффекты с ещё более мягкими максимумами ---
        if (vignette != null)
        {
            // меньше затемнение по краям
            vignette.intensity.value = Mathf.Lerp(baseVignette, 0.35f, finalFactor);
            vignette.smoothness.value = Mathf.Lerp(baseVignetteSmoothness, 0.7f, finalFactor);
        }

        if (colorAdj != null)
        {
            // экспозиция: совсем немного темнее
            colorAdj.postExposure.value = Mathf.Lerp(baseExposure, -0.3f, finalFactor);
            // контраст: лёгкий, не убивает детали
            colorAdj.contrast.value = Mathf.Lerp(baseContrast, 6f, finalFactor);
        }

        if (chromatic != null)
        {
            // хроматика ощутима, но не превращает всё в кашу
            chromatic.intensity.value = Mathf.Lerp(baseChromatic, 0.25f, finalFactor);
        }

        if (lens != null)
        {
            // меньше "рыбьего глаза"
            lens.intensity.value = Mathf.Lerp(baseLensIntensity, -0.12f, finalFactor);
        }

        if (grain != null)
        {
            // шум помягче
            grain.intensity.value = Mathf.Lerp(baseGrainIntensity, 0.3f, finalFactor);
        }

#if UNITY_EDITOR
        Debug.DrawRay(cam.transform.position, cam.transform.forward * 3f, Color.cyan);
#endif
    }
}

