using System.Collections;
using UnityEngine;
using FMOD.Studio;
using FMODUnity;

[RequireComponent(typeof(Light))]
public class FlickeringLamp : MonoBehaviour
{
    private Light lamp;

    [Header("Base Light")]
    public float baseIntensity = 1.2f;
    public float intensityVariation = 0.4f;

    [Header("Pulse")]
    public float pulseSpeed = 1.5f;
    public float pulseStrength = 0.25f;

    [Header("Noise")]
    public float noiseSpeed = 3.5f;
    public float noiseStrength = 0.35f;

    [Header("Blackout")]
    [Range(0f, 1f)] public float blackoutChance = 0.015f;
    public Vector2 blackoutDuration = new Vector2(0.1f, 0.6f);

    [Header("FMOD")]
    public EventReference pulseEvent;
    public EventReference flickerEvent;

    private EventInstance pulseInstance;

    private float noiseSeed;
    private bool isBlackout;

    void Awake()
    {
        lamp = GetComponent<Light>();
        noiseSeed = Random.Range(0f, 1000f);

        // Pulse loop
        if (!pulseEvent.IsNull)
        {
            pulseInstance = RuntimeManager.CreateInstance(pulseEvent);
            pulseInstance.start();
        }
    }

    void Update()
    {
        if (isBlackout) return;

        float pulse = Mathf.Sin(Time.time * pulseSpeed) * pulseStrength;
        float normalizedPulse = Mathf.InverseLerp(-pulseStrength, pulseStrength, pulse);

        float noise = Mathf.PerlinNoise(Time.time * noiseSpeed, noiseSeed);
        noise = (noise - 0.5f) * noiseStrength;

        float finalIntensity = baseIntensity + pulse + noise;
        finalIntensity = Mathf.Clamp(finalIntensity, 0f, baseIntensity + intensityVariation);
        lamp.intensity = finalIntensity;

        // Передаём пульсацию в FMOD
        if (pulseInstance.isValid())
        {
            pulseInstance.setParameterByName("PulseIntensity", normalizedPulse);
        }

        if (Random.value < blackoutChance * Time.deltaTime)
        {
            StartCoroutine(Blackout());
        }
    }

    IEnumerator Blackout()
    {
        isBlackout = true;

        // Flicker OFF
        if (!flickerEvent.IsNull)
            RuntimeManager.PlayOneShot(flickerEvent, transform.position);

        lamp.intensity = 0f;

        yield return new WaitForSeconds(Random.Range(blackoutDuration.x, blackoutDuration.y));

        // Flicker ON
        if (!flickerEvent.IsNull)
            RuntimeManager.PlayOneShot(flickerEvent, transform.position);

        isBlackout = false;
    }

    void OnDestroy()
    {
        if (pulseInstance.isValid())
        {
            pulseInstance.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
            pulseInstance.release();
        }
    }
}