using UnityEngine;
using FMODUnity;
using FMOD.Studio;

public class ImpactSound : MonoBehaviour
{
    public enum ItemWeightType
    {
        Light = 0,
        Medium = 1,
        Heavy = 2
    }

    [SerializeField] private EventReference impactEvent;
    [SerializeField] private float minVelocity = 1.5f;
    [SerializeField] private float maxVelocity = 10f;
    [SerializeField] private float cooldown = 0.1f;
    [SerializeField] private ItemWeightType itemWeight;

    private float lastPlayTime;

    private void OnCollisionEnter(Collision collision)
    {
        float velocity = collision.relativeVelocity.magnitude;

        if (velocity < minVelocity) return;
        if (Time.time - lastPlayTime < cooldown) return;

        lastPlayTime = Time.time;

        float normalized = Mathf.InverseLerp(minVelocity, maxVelocity, velocity);

        var instance = RuntimeManager.CreateInstance(impactEvent);
        instance.set3DAttributes(RuntimeUtils.To3DAttributes(transform.position));

        instance.setParameterByName("ItemWeight", (float)itemWeight);
        instance.setVolume(normalized);

        instance.start();
        instance.release();
    }
}