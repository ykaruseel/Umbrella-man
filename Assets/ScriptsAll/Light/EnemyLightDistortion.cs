using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using FMODUnity;
using FMOD.Studio;

public class EnemyLightDistortion : MonoBehaviour
{
    public bool chaseActive = false;

    public Transform enemy;
    public float effectRadius = 6f;
    public float burstChance = 0.3f;
    public float burstDuration = 0.1f;
    public float offIntensityMultiplier = 0.1f;

    public float globalPulseMin = 0.6f;
    public float globalPulseMax = 1.0f;
    public float globalPulseSpeed = 2f;

    public List<Light> sceneLights = new List<Light>();

    [SerializeField] private EventReference lightPulseEvent;

    private Dictionary<Light, float> originalIntensities = new Dictionary<Light, float>();
    private Dictionary<Light, EventInstance> pulseInstances = new Dictionary<Light, EventInstance>();

    private bool audioActive;

    public void SetChaseActive(bool value)
    {
        chaseActive = value;

        if (chaseActive && !audioActive)
        {
            foreach (var inst in pulseInstances.Values)
                inst.start();

            audioActive = true;
        }
        else if (!chaseActive && audioActive)
        {
            foreach (var inst in pulseInstances.Values)
                inst.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);

            audioActive = false;
        }
    }

    void Start()
    {
        foreach (var lamp in sceneLights)
        {
            if (lamp == null) continue;

            if (!originalIntensities.ContainsKey(lamp))
                originalIntensities[lamp] = lamp.intensity;

            if (!lightPulseEvent.IsNull)
            {
                var inst = RuntimeManager.CreateInstance(lightPulseEvent);
                RuntimeManager.AttachInstanceToGameObject(inst, lamp.transform, lamp.GetComponent<Rigidbody>());
                pulseInstances[lamp] = inst;
            }
        }

        StartCoroutine(ProcessLights());
    }

    IEnumerator ProcessLights()
    {
        while (true)
        {
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

            float t = Mathf.PingPong(Time.time * globalPulseSpeed, 1f);
            float pulse = Mathf.Lerp(globalPulseMin, globalPulseMax, t);

            foreach (var lamp in sceneLights)
            {
                if (lamp == null) continue;
                if (!originalIntensities.TryGetValue(lamp, out float orig))
                    continue;

                float dist = Vector3.Distance(enemy.position, lamp.transform.position);
                float targetIntensity = orig * pulse;

                if (dist <= effectRadius)
                {
                    targetIntensity = orig * offIntensityMultiplier;

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

        lamp.intensity = orig;
        yield return new WaitForSeconds(burstDuration);
        lamp.intensity = offValue;
    }

    private void OnDestroy()
    {
        foreach (var inst in pulseInstances.Values)
        {
            if (inst.isValid())
            {
                inst.stop(FMOD.Studio.STOP_MODE.IMMEDIATE);
                inst.release();
            }
        }

        pulseInstances.Clear();
    }
}

