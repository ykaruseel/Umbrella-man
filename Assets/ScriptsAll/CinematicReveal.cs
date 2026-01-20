using UnityEngine;
using System.Collections;
using FMODUnity;

public class CinematicReveal : MonoBehaviour
{
    [Header("ЛОГИКА АКТИВАЦИИ")]
    [Tooltip("Эта галочка включится САМА, когда ты поговоришь с дверью.")]
    public bool canActivate = false; 

    [Header("Настройки")]
    public PlayerController player;
    public Transform lookTarget;
    public GameObject umbrellaMan;
    public FollowLightController oldController; 

    [Header("Свет и Эффекты")]
    public Light thirdLamp;
    public GameObject lampModel;
    public ParticleSystem smokeParticles;
    public ParticleSystem sparkParticles; 
    public GameObject sparksBaseLight;    

    [Header("Звук")]
    public EventReference appearSound;
    public EventReference explosionSound;

    [Header("Тайминги")]
    public float smokeDuration = 5.0f;
    public float stareDuration = 2.0f;
    public float zoomFOV = 40f;
    
    private bool hasTriggered = false;
    private Coroutine flickerCoroutine;
    private Renderer[] manRenderers;

    void Start()
    {
        if (umbrellaMan != null)
        {
            manRenderers = umbrellaMan.GetComponentsInChildren<Renderer>();
            foreach (var r in manRenderers) r.enabled = false; 
        }
        if (smokeParticles != null) smokeParticles.Stop();
        if (sparkParticles != null) sparkParticles.Stop();
        if (sparksBaseLight != null) sparksBaseLight.SetActive(false);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (hasTriggered || !other.CompareTag("Player")) return;

        if (canActivate == false)
        {
            return; 
        }

        Debug.Log("Cinematic: Дверь дала добро! ЗАПУСК!");
        hasTriggered = true;
        StartCoroutine(PlayCinematicSequence());
    }

    // --- КАТСЦЕНА ---
    IEnumerator PlayCinematicSequence()
    {
        if (oldController != null) { oldController.StopAllCoroutines(); oldController.enabled = false; }
        if (player != null) { player.SetCanMove(false); StartCoroutine(DoZoom(zoomFOV, 2.0f)); }
        
        // 🔥 ВОТ ИСПРАВЛЕНИЕ 🔥
        // Если лампа была выключена (темно), мы её ПРИНУДИТЕЛЬНО включаем перед миганием.
        if (thirdLamp != null)
        {
            thirdLamp.enabled = true; // Включаем объект
            thirdLamp.intensity = 2.0f; // Даем нормальную яркость
        }

        if (thirdLamp != null) flickerCoroutine = StartCoroutine(FlickerLightRoutine());
        if (smokeParticles != null) smokeParticles.Play();
        
        yield return new WaitForSeconds(smokeDuration);

        if (flickerCoroutine != null) StopCoroutine(flickerCoroutine);
        if (thirdLamp != null) thirdLamp.intensity = 0f; 

        if (umbrellaMan != null)
        {
            Vector3 lookPos = player.transform.position;
            lookPos.y = umbrellaMan.transform.position.y;
            umbrellaMan.transform.LookAt(lookPos);
            if (manRenderers != null) foreach (var r in manRenderers) r.enabled = true;
            if (!appearSound.IsNull) RuntimeManager.PlayOneShot(appearSound, umbrellaMan.transform.position);
        }

        float fadeTime = 0f;
        while (fadeTime < 1.5f) { fadeTime += Time.deltaTime; if (thirdLamp != null) thirdLamp.intensity = Mathf.Lerp(0f, 2.5f, fadeTime / 1.5f); yield return null; }
        if (thirdLamp != null) thirdLamp.intensity = 2.5f;

        yield return new WaitForSeconds(stareDuration);

        if (!explosionSound.IsNull) RuntimeManager.PlayOneShot(explosionSound, thirdLamp.transform.position);
        if (sparkParticles != null) sparkParticles.Play();
        if (sparksBaseLight != null) sparksBaseLight.SetActive(true);
        if (thirdLamp != null) { thirdLamp.enabled = false; thirdLamp.intensity = 0; }
        if (lampModel != null) { var r = lampModel.GetComponent<Renderer>(); if(r) r.material.DisableKeyword("_EMISSION"); }

        yield return new WaitForSeconds(0.5f);
        if (player != null) { StartCoroutine(DoZoom(60f, 0.5f)); player.isCinematic = false; player.SetCanMove(true); }
        if (umbrellaMan != null) { var chase = umbrellaMan.GetComponent<UmbrellaManChase>(); if (chase != null) chase.StartChase(); }
        
        Destroy(gameObject, 2f);
    }
    
    IEnumerator FlickerLightRoutine() { while (true) { if (!thirdLamp) yield break; thirdLamp.intensity = Random.Range(0.2f, 3f); yield return new WaitForSeconds(Random.Range(0.05f, 0.15f)); } }
    IEnumerator DoZoom(float t, float d) { if (!player) yield break; float s = player.playerCamera.fieldOfView; float x = 0; while (x < d) { x += Time.deltaTime; player.playerCamera.fieldOfView = Mathf.Lerp(s, t, x / d); yield return null; } player.playerCamera.fieldOfView = t; }
}