// 📁 Assets/ScriptsAll/LightFlickerController.cs
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class LightFlickerController : MonoBehaviour
{
    [Header("Световые источники")]
    [Tooltip("Перетащи сюда все лампы, которые должны мигать")]
    public List<Light> lightsToFlicker = new List<Light>();

    [Header("Настройки времени и частоты")]
    [Tooltip("Общее время мигания (сек)")]
    public float totalFlickerTime = 25f;
    [Tooltip("Максимальная частота мигания (раз в сек)")]
    public float maxFlickerFrequency = 6f;
    [Tooltip("Минимальная доля яркости при затухании")]
    public float minIntensityFactor = 0.2f;
    [Tooltip("Кривая напряжения (скорость нарастания мигания)")]
    public AnimationCurve tensionCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);

    [Header("Интенсивность пика света")]
    [Tooltip("Во сколько раз ярче становится свет в пике мигания перед концом")]
    public float maxLightMultiplier = 3f;

    [Header("Постпроцесс")]
    [Tooltip("Volume с постпроцессом (Chromatic, Vignette, Grain, LensDistortion)")]
    public Volume globalVolume;
    [Tooltip("Включить пульсацию эффектов во время мигания")]
    public bool enableEffectPulse = true;
    [Tooltip("Скорость пульсации эффектов")]
    public float effectPulseSpeed = 1.5f;
    [Tooltip("Максимум ХА")]
    public float chromaticMax = 0.8f;
    [Tooltip("Максимум виньетки")]
    public float vignetteMax = 0.5f;
    [Tooltip("Максимум шума")]
    public float grainMax = 1f;
    [Tooltip("Максимум дисторсии")]
    public float lensDistortionMax = 0.4f;

    private List<float> initialIntensities = new List<float>();
    private ChromaticAberration chromatic;
    private Vignette vignette;
    private FilmGrain grain;
    private LensDistortion lensDistortion;

    private bool isSequenceRunning = false;

    void Start()
    {
        foreach (Light l in lightsToFlicker)
            initialIntensities.Add(l ? l.intensity : 0);

        if (globalVolume != null)
        {
            globalVolume.profile.TryGet(out chromatic);
            globalVolume.profile.TryGet(out vignette);
            globalVolume.profile.TryGet(out grain);
            globalVolume.profile.TryGet(out lensDistortion);
        }
    }

    /// <summary>
    /// Запуск последовательности мигания света.
    /// </summary>
    public IEnumerator FlickerSequence(System.Action onCompleteCallback)
    {
        if (isSequenceRunning) yield break;
        isSequenceRunning = true;

        Debug.Log("🎬 Начинается сцена мигания света...");

        float timer = 0f;

        while (timer < totalFlickerTime)
        {
            float t = timer / totalFlickerTime;
            float tension = tensionCurve.Evaluate(t);
            float freq = Mathf.Lerp(1f, maxFlickerFrequency, tension);
            float interval = 1f / freq;

            // Меняем свет
            for (int i = 0; i < lightsToFlicker.Count; i++)
            {
                Light l = lightsToFlicker[i];
                if (l == null) continue;

                float baseIntensity = initialIntensities[i];
                l.intensity = baseIntensity * Random.Range(minIntensityFactor, 1f);
                l.enabled = Random.value > 0.2f;
            }

            // Пульсация постпроцессинга
            if (enableEffectPulse && globalVolume != null)
                PulsePostEffects(tension);

            yield return new WaitForSeconds(interval);
            timer += interval;
        }

        Debug.Log("🎬 Мигание завершено.");

        // Проверяем состояние квеста (если панель не починена — смерть)
        QuestManager qm = QuestManager.instance;
        if (qm != null && qm.currentQuest != null)
        {
            QuestObjective obj = qm.currentQuest.GetCurrentObjective();
            if (obj != null && obj.targetID.ToLower() == "panel" && !obj.isComplete)
            {
                Debug.Log("💀 Игрок не успел — скример и Game Over.");
                qm.StartCoroutine(qm.TriggerUmbrellaManDeath());
                yield break;
            }
        }

        TurnOnAllLights();
        ResetPostEffects();

        isSequenceRunning = false;
        onCompleteCallback?.Invoke();
    }

    /// <summary>
    /// Пульсирует интенсивность эффектов в зависимости от напряжения.
    /// </summary>
    private void PulsePostEffects(float tension)
    {
        float pulse = Mathf.Abs(Mathf.Sin(Time.time * effectPulseSpeed * Mathf.Lerp(1f, 3f, tension)));

        if (chromatic != null)
            chromatic.intensity.value = Mathf.Lerp(0f, chromaticMax, pulse);
        if (vignette != null)
            vignette.intensity.value = Mathf.Lerp(0f, vignetteMax, pulse);
        if (grain != null)
            grain.intensity.value = Mathf.Lerp(0f, grainMax, pulse);
        if (lensDistortion != null)
            lensDistortion.intensity.value = Mathf.Lerp(0f, lensDistortionMax, pulse);
    }

    /// <summary>
    /// Возвращает эффекты к нормальному состоянию.
    /// </summary>
    public void ResetPostEffects()
    {
        if (chromatic != null) chromatic.intensity.value = 0f;
        if (vignette != null) vignette.intensity.value = 0f;
        if (grain != null) grain.intensity.value = 0f;
        if (lensDistortion != null) lensDistortion.intensity.value = 0f;
    }

    /// <summary>
    /// Включает все лампы с нормальной яркостью.
    /// </summary>
    public void TurnOnAllLights()
    {
        for (int i = 0; i < lightsToFlicker.Count; i++)
        {
            if (lightsToFlicker[i])
            {
                lightsToFlicker[i].enabled = true;
                lightsToFlicker[i].intensity = initialIntensities[i];
            }
        }
        ResetPostEffects();
        Debug.Log("💡 Свет восстановлен.");
    }

    /// <summary>
    /// Полное отключение света.
    /// </summary>
    public void TurnOffAllLights()
    {
        foreach (Light l in lightsToFlicker)
        {
            if (l) l.enabled = false;
        }
        ResetPostEffects();
        Debug.Log("💡 Свет полностью выключен.");
    }

    /// <summary>
    /// Максимальная вспышка света (в пике скримера или провала QTE).
    /// </summary>
    public void MaxOutLights()
    {
        for (int i = 0; i < lightsToFlicker.Count; i++)
        {
            if (lightsToFlicker[i])
            {
                lightsToFlicker[i].enabled = true;
                lightsToFlicker[i].intensity = initialIntensities[i] * maxLightMultiplier;
            }
        }

        if (chromatic != null) chromatic.intensity.value = chromaticMax;
        if (vignette != null) vignette.intensity.value = vignetteMax;
        if (grain != null) grain.intensity.value = grainMax;
        if (lensDistortion != null) lensDistortion.intensity.value = lensDistortionMax;

        Debug.Log("⚡ Свет достиг максимума — пик ужаса.");
    }
}
