// FollowLightController.cs — fixed: all lamps steady ON at sequence start; only active lamp flickers.
// Includes per-lamp delays, sound debounce and final sequence (no strobe).
using UnityEngine;
using System.Collections;
using FMODUnity;

public class FollowLightController : MonoBehaviour
{
    public Light[] lightsSequence;

    public float flickerMinDelay = 0.2f;
    public float flickerMaxDelay = 0.8f;

    public float[] perLampMinDelay;
    public float[] perLampMaxDelay;

    [Header("Final lamp timing (no strobe)")]
    public float finalMinDelay = 0.08f;
    public float finalMaxDelay = 0.25f;

    private int currentLightIndex = 0;
    private Coroutine[] flickerCoroutines;
    private QuestManager questManager;

    [Header("Optional FMOD one-shot sounds (3D)")]
    [SerializeField] public EventReference lightOnEvent;
    [SerializeField] public EventReference lightOffEvent;

    [Header("Sound debounce (avoid annoying double clicks)")]
    public float minSoundInterval = 0.18f;
    public float[] perLampMinSoundInterval;
    private float[] lastSoundTime;

    void Awake()
    {
        if (lightsSequence != null)
        {
            flickerCoroutines = new Coroutine[lightsSequence.Length];
            lastSoundTime = new float[lightsSequence.Length];
            for (int i = 0; i < lastSoundTime.Length; i++) lastSoundTime[i] = -999f;
        }
    }

    // START: все лампы включены ровно; только первая начнёт мигать
    public void StartSequence(QuestManager qm)
    {
        questManager = qm;
        currentLightIndex = 0;

        if (lightsSequence == null) return;

        // 1) Включаем ВСЕ лампы (steady ON)
        for (int i = 0; i < lightsSequence.Length; i++)
        {
            var l = lightsSequence[i];
            if (l != null)
            {
                l.enabled = true;
            }

            // 2) останавливаем любые корутины и сбрасываем таймеры звуков
            if (flickerCoroutines != null && i < flickerCoroutines.Length && flickerCoroutines[i] != null)
            {
                StopCoroutine(flickerCoroutines[i]);
                flickerCoroutines[i] = null;
            }

            if (lastSoundTime != null && i < lastSoundTime.Length)
                lastSoundTime[i] = -999f;

            // 3) деактивируем триггеры, активируем только для pulsing lamp позже
            var trig = l != null ? l.GetComponent<FollowLightTrigger>() : null;
            if (trig != null)
                trig.DeactivateTrigger();
        }

        // Запускаем пульс только для первой лампы
        ActivateLight(currentLightIndex);
    }

    // Activate flicker for a single lamp. Do NOT change other lamps' enabled state.
    void ActivateLight(int index)
    {
        if (lightsSequence == null) return;
        if (index < 0 || index >= lightsSequence.Length) return;

        // stop other flicker coroutines but DO NOT modify their light.enabled values
        for (int i = 0; i < lightsSequence.Length; i++)
        {
            if (i == index) continue;
            if (flickerCoroutines != null && i < flickerCoroutines.Length && flickerCoroutines[i] != null)
            {
                StopCoroutine(flickerCoroutines[i]);
                flickerCoroutines[i] = null;
            }
        }

        Light light = lightsSequence[index];
        if (light == null) return;

        // activate trigger for this lamp
        var trig = light.GetComponent<FollowLightTrigger>();
        if (trig != null)
            trig.ActivateTrigger();

        // compute delays
        float min = flickerMinDelay;
        float max = flickerMaxDelay;
        if (perLampMinDelay != null && perLampMaxDelay != null &&
            perLampMinDelay.Length == lightsSequence.Length && perLampMaxDelay.Length == lightsSequence.Length)
        {
            min = perLampMinDelay[index];
            max = perLampMaxDelay[index];
        }

        // Ensure flickerCoroutines array exists
        if (flickerCoroutines == null)
            flickerCoroutines = new Coroutine[lightsSequence.Length];

        // Stop existing coroutine if any
        if (flickerCoroutines[index] != null)
        {
            StopCoroutine(flickerCoroutines[index]);
            flickerCoroutines[index] = null;
        }

        // Start flicker coroutine. IMPORTANT: coroutine will toggle lamp but we ensure its first action
        // will be to set lamp.enabled = true for visual continuity (so others don't go dark).
        flickerCoroutines[index] = StartCoroutine(FlickerLightCoroutine(index, light, min, max));
    }

    // Called by FollowLightTrigger when player approaches
    public void LightTriggered(int index)
    {
        if (lightsSequence == null) return;
        if (index != currentLightIndex) return;
        if (index < 0 || index >= lightsSequence.Length) return;

        Light light = lightsSequence[index];

        // stop flicker coroutine for this lamp
        if (flickerCoroutines != null && index < flickerCoroutines.Length && flickerCoroutines[index] != null)
        {
            StopCoroutine(flickerCoroutines[index]);
            flickerCoroutines[index] = null;
        }

        // set steady ON
        if (light != null)
            light.enabled = true;

        // play on-sound with debounce
        if (light != null && !lightOnEvent.IsNull)
        {
            if (CanPlaySoundForLamp(index))
                RuntimeManager.PlayOneShotAttached(lightOnEvent, light.gameObject);
        }

        // deactivate trigger
        var trig = light != null ? light.GetComponent<FollowLightTrigger>() : null;
        if (trig != null) trig.DeactivateTrigger();

        // advance or final
        if (index == lightsSequence.Length - 1)
        {
            StartCoroutine(FinalLightSequence(light));
        }
        else
        {
            currentLightIndex++;
            flickerMinDelay *= 0.9f;
            flickerMaxDelay *= 0.9f;

            if (perLampMinDelay != null && perLampMaxDelay != null &&
                perLampMinDelay.Length == lightsSequence.Length && perLampMaxDelay.Length == lightsSequence.Length)
            {
                perLampMinDelay[index] *= 0.9f;
                perLampMaxDelay[index] *= 0.9f;
            }

            // small delay before next lamp starts flickering to avoid immediate double-click feeling
            StartCoroutine(DelayedActivateNext(currentLightIndex, 0.25f));
        }
    }

    // Delayed activation so player has small breathing space
    private IEnumerator DelayedActivateNext(int index, float delay)
    {
        yield return new WaitForSeconds(delay);
        ActivateLight(index);
    }

    // Flicker coroutine — toggles the lamp. First iteration ensures it's ON before toggling off,
    // so other lamps never end up all off at start.
    IEnumerator FlickerLightCoroutine(int index, Light light, float min, float max)
    {
        if (light == null) yield break;

        // make sure lamp is visibly ON to start with, then wait one interval before toggling off,
        // so initial state for other lamps remains ON.
        light.enabled = true;
        float firstWait = Random.Range(min, max);
        yield return new WaitForSeconds(firstWait);

        while (true)
        {
            // toggle
            light.enabled = !light.enabled;

            // play sound with debounce
            if (light.enabled)
            {
                if (!lightOnEvent.IsNull && CanPlaySoundForLamp(index))
                    RuntimeManager.PlayOneShotAttached(lightOnEvent, light.gameObject);
            }
            else
            {
                if (!lightOffEvent.IsNull && CanPlaySoundForLamp(index))
                    RuntimeManager.PlayOneShotAttached(lightOffEvent, light.gameObject);
            }

            float wait = Random.Range(min, max);
            yield return new WaitForSeconds(wait);
        }
    }

    // Final sequence: no fast strobe, controlled toggles, then final off and chase trigger
    IEnumerator FinalLightSequence(Light light)
    {
        if (light == null)
        {
            if (questManager != null) questManager.TriggerChaseScene();
            yield break;
        }

        int lastIndex = lightsSequence != null ? lightsSequence.Length - 1 : -1;
        float min = finalMinDelay;
        float max = finalMaxDelay;
        if (lastIndex >= 0 && perLampMinDelay != null && perLampMaxDelay != null &&
            perLampMinDelay.Length == lightsSequence.Length && perLampMaxDelay.Length == lightsSequence.Length)
        {
            min = perLampMinDelay[lastIndex];
            max = perLampMaxDelay[lastIndex];
        }

        float wait1 = Mathf.Clamp(Random.Range(min, max) * 0.7f, 0.02f, 1f);
        float wait2 = Mathf.Clamp(Random.Range(min, max) * 0.6f, 0.02f, 1f);
        float wait3 = Mathf.Clamp(Random.Range(min, max) * 0.8f, 0.02f, 1f);

        // sequence: on -> off -> on -> final off (sounds debounced)
        light.enabled = true;
        if (!lightOnEvent.IsNull && CanPlaySoundForLamp(lastIndex)) RuntimeManager.PlayOneShotAttached(lightOnEvent, light.gameObject);
        yield return new WaitForSeconds(wait1);

        light.enabled = false;
        if (!lightOffEvent.IsNull && CanPlaySoundForLamp(lastIndex)) RuntimeManager.PlayOneShotAttached(lightOffEvent, light.gameObject);
        yield return new WaitForSeconds(wait2);

        light.enabled = true;
        if (!lightOnEvent.IsNull && CanPlaySoundForLamp(lastIndex)) RuntimeManager.PlayOneShotAttached(lightOnEvent, light.gameObject);
        yield return new WaitForSeconds(wait3);

        light.enabled = false;
        if (!lightOffEvent.IsNull && CanPlaySoundForLamp(lastIndex)) RuntimeManager.PlayOneShotAttached(lightOffEvent, light.gameObject);

        if (questManager != null) questManager.TriggerChaseScene();
    }

    private bool CanPlaySoundForLamp(int index)
    {
        if (index < 0 || lastSoundTime == null || index >= lastSoundTime.Length)
            return true;

        float minInterval = minSoundInterval;
        if (perLampMinSoundInterval != null && perLampMinSoundInterval.Length == lightsSequence.Length)
            minInterval = perLampMinSoundInterval[index];

        float since = Time.time - lastSoundTime[index];
        if (since >= minInterval)
        {
            lastSoundTime[index] = Time.time;
            return true;
        }
        else
        {
            return false;
        }
    }

    private void OnDestroy()
    {
        if (flickerCoroutines == null) return;
        for (int i = 0; i < flickerCoroutines.Length; i++)
        {
            if (flickerCoroutines[i] != null)
            {
                StopCoroutine(flickerCoroutines[i]);
                flickerCoroutines[i] = null;
            }
        }
    }
}