using UnityEngine;
using System.Collections;
using FMODUnity;

public class LightBulbEvent : MonoBehaviour
{
    [Header("Objects")] public Light bulbLight;
    public Transform player;
    public ParticleSystem breakParticles;
    public GameObject bulbMesh;

    [Header("Settings")] public float breakDistance = 2.5f;
    public float minFlickerIntensity = 0.1f;
    public float maxFlickerIntensity = 2.5f;

    [Header("Final Death Sequence")] [Tooltip("How long the bulb blinks rapidly before dying")]
    public float finalStrobeDuration = 0.5f;

    [Tooltip("How long it takes to fade out after blinking")]
    public float finalFadeDuration = 1.5f;

    [Header("FMOD Audio")] public EventReference breakSound;
    public EventReference flickerSound;

    private bool isFlickering = false;
    private bool isBroken = false;
    private FMOD.Studio.EventInstance flickerInstance;

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") && !isFlickering && !isBroken)
        {

            Collider myCollider = GetComponent<Collider>();
            if (myCollider != null)
            {
                myCollider.enabled = false;
            }

            isFlickering = true;
            StartCoroutine(FlickerRoutine());

            if (!flickerSound.IsNull)
            {
                flickerInstance = RuntimeManager.CreateInstance(flickerSound);
                RuntimeManager.AttachInstanceToGameObject(flickerInstance, transform);
                flickerInstance.start();
            }
        }
    }

    void Update()
    {
        if (isFlickering && !isBroken && player != null)
        {
            float dist = Vector3.Distance(transform.position, player.position);

            if (dist <= breakDistance)
            {

                StartCoroutine(FinalBreakSequence());
            }
        }
    }

    IEnumerator FlickerRoutine()
    {
        while (isFlickering && !isBroken)
        {
            if (bulbLight != null)
                bulbLight.intensity = Random.Range(minFlickerIntensity, maxFlickerIntensity);

            yield return new WaitForSeconds(Random.Range(0.05f, 0.25f));
        }
    }


    IEnumerator FinalBreakSequence()
    {
        isBroken = true;
        isFlickering = false;

        float strobeTimer = 0;
        while (strobeTimer < finalStrobeDuration)
        {
            strobeTimer += Time.deltaTime;

            bulbLight.enabled = !bulbLight.enabled;
            bulbLight.intensity = maxFlickerIntensity * 1.5f;
            yield return new WaitForSeconds(0.02f);
        }

        bulbLight.enabled = true;

        if (flickerInstance.isValid())
        {
            flickerInstance.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
            flickerInstance.release();
        }

        if (breakParticles != null) breakParticles.Play();
        if (!breakSound.IsNull) RuntimeManager.PlayOneShot(breakSound, transform.position);

        float fadeTimer = 0;
        float startIntensity = bulbLight.intensity;

        while (fadeTimer < finalFadeDuration)
        {
            fadeTimer += Time.deltaTime;
            bulbLight.intensity = Mathf.Lerp(startIntensity, 0, fadeTimer / finalFadeDuration);
            yield return null;
        }

        bulbLight.enabled = false;
        if (bulbMesh != null) bulbMesh.SetActive(false);


        this.enabled = false;
    }
}