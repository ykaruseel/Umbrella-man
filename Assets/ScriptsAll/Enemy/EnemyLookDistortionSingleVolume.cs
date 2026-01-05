// EnemyLookDistortionSingleVolume.cs — softened postprocess strength and pulse
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class EnemyLookDistortionSingleVolume : MonoBehaviour
{
    [Header("References")]
    public Transform enemy;
    public Volume mainVolume;

    [Header("Distance thresholds")]
    public float maxDistance = 20f;
    public float midDistance = 12f;
    public float nearDistance = 6f;

    [Header("Angle")]
    [Range(0f, 90f)]
    public float maxAngle = 70f;
    [Range(0f, 45f)]
    public float nearAngle = 30f;

    [Header("Behaviour")]
    public float lerpSpeed = 3f;
    public float pulseSpeed = 3.0f;

    [Header("Occlusion")]
    public LayerMask occlusionMask = ~0;

    [Header("Tuning (reduce to weaken effects)")]
    [Range(0f, 1f)] public float postProcessStrength = 0.6f; // overall multiplier - tweak this down to weaken effects
    [Range(0f, 1f)] public float pulseAmplitude = 0.08f; // was ~0.15 - lower for less strong pulse

    private Camera cam;

    Vignette vignette;
    ChromaticAberration chromatic;
    LensDistortion lens;
    FilmGrain grain;
    ColorAdjustments colorAdj;

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
            Debug.LogError("[EnemyLookDistortion] mainVolume not assigned!");
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

        if (chromatic != null) baseChromatic = chromatic.intensity.value;
        if (lens != null) baseLensIntensity = lens.intensity.value;
        if (grain != null) baseGrainIntensity = grain.intensity.value;
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

            // distance zone multiplier (keep smaller to soften effect)
            float distanceZoneMultiplier;
            if (distance > midDistance) distanceZoneMultiplier = 0.14f; // was 0.2
            else if (distance > nearDistance) distanceZoneMultiplier = 0.28f; // was 0.4
            else distanceZoneMultiplier = 0.7f; // was 1.0

            // occlusion check
            bool blocked = false;
            if (Physics.Raycast(cam.transform.position, dirToEnemy, out RaycastHit hit, maxDistance, occlusionMask))
            {
                if (hit.transform != enemy && !hit.transform.IsChildOf(enemy))
                    blocked = true;
            }

            if (!blocked)
            {
                float angleFactor = 0f;
                if (angle <= maxAngle)
                {
                    float tAngle = Mathf.Clamp01(angle / maxAngle);
                    angleFactor = 1f - tAngle;

                    if (distance <= nearDistance && angle <= nearAngle)
                        isNearZone = true;
                }
                else
                {
                    if (distance <= nearDistance) angleFactor = 0.2f;
                    else angleFactor = 0f;
                }

                visibilityFactor = distanceZoneMultiplier * angleFactor;
            }
        }

        target = visibilityFactor;
        current = Mathf.Lerp(current, target, Time.deltaTime * lerpSpeed);

        float finalFactor = current;

        if (isNearZone && current > 0.5f)
        {
            float pulse = 1f + pulseAmplitude * Mathf.Sin(Time.time * pulseSpeed); // milder pulse
            finalFactor *= pulse;
        }

        // overall strength multiplier (soften everything)
        finalFactor = Mathf.Clamp01(finalFactor * postProcessStrength);

        // Apply softened effects
        if (vignette != null)
        {
            vignette.intensity.value = Mathf.Lerp(baseVignette, baseVignette + 0.25f, finalFactor); // softer max increase
            vignette.smoothness.value = Mathf.Lerp(baseVignetteSmoothness, 0.5f, finalFactor);
        }

        if (colorAdj != null)
        {
            //colorAdj.postExposure.value = Mathf.Lerp(baseExposure, -0.5f, finalFactor); // less darkening (was -1)
            colorAdj.contrast.value = Mathf.Lerp(baseContrast, 3f, finalFactor); // softer contrast (was 7)
        }

        if (chromatic != null)
            chromatic.intensity.value = Mathf.Lerp(baseChromatic, 1.5f, finalFactor); // softer chroma

        if (lens != null)
            lens.intensity.value = Mathf.Lerp(baseLensIntensity, -0.5f, finalFactor);

        if (grain != null)
            grain.intensity.value = Mathf.Lerp(baseGrainIntensity, 1.5f, finalFactor);

        if (colorAdj != null)
            colorAdj.saturation.value = Mathf.Lerp(baseSaturation, -10f, finalFactor);
    }
}

