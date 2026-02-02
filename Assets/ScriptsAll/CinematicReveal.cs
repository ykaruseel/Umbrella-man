using UnityEngine;
using System.Collections;
using FMODUnity;

public class CinematicReveal : MonoBehaviour
{
    [Header("ЛОГИКА АКТИВАЦИИ")]
    public bool canActivate = false;

    [Header("СВЕТ ПАНЕЛИ (МАЯК)")]
    public PanelLightBeacon electricPanelLight;

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
    public EventReference smokeSound;

    [Header("Тайминги")]
    public float smokeDuration = 5.0f;
    public float stareDuration = 2.0f;
    public float zoomFOV = 40f;

    private bool hasTriggered = false;
    private Coroutine flickerCoroutine;
    private Renderer[] manRenderers;
    private Collider myCollider; 

    void Start()
    {
        myCollider = GetComponent<Collider>(); 

        if (umbrellaMan != null)
        {
            manRenderers = umbrellaMan.GetComponentsInChildren<Renderer>();
            foreach (var r in manRenderers) r.enabled = false;
        }
        
        // Гарантируем чистое состояние на старте
        ResetCinematicState();
    }

    // ЭТА ФУНКЦИЯ ЧИНИТ БАГ: ОНА СБРАСЫВАЕТ СЦЕНУ ПРИ РЕСПАУНЕ
    public void ResetCinematicState()
    {
        StopAllCoroutines(); 
        hasTriggered = false; 

        // 1. Включаем триггер обратно
        if (myCollider != null) myCollider.enabled = true;

        // 2. Выключаем все эффекты
        if (smokeParticles != null) smokeParticles.Stop();
        if (sparkParticles != null) sparkParticles.Stop();
        if (sparksBaseLight != null) sparksBaseLight.SetActive(false);
        if (thirdLamp != null) { thirdLamp.enabled = false; thirdLamp.intensity = 0; }
        
        if (flickerCoroutine != null) StopCoroutine(flickerCoroutine);

        // 3. Сбрасываем врага
        if (umbrellaMan != null)
        {
            var chase = umbrellaMan.GetComponent<UmbrellaManChase>();
            if (chase != null) chase.ResetChase(); 

            if (manRenderers != null) foreach (var r in manRenderers) r.enabled = false;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (hasTriggered || !other.CompareTag("Player")) return;
        if (canActivate == false) return;

        Debug.Log("Cinematic: Дверь дала добро! ЗАПУСК!");
        hasTriggered = true;
        
        // Временно выключаем триггер, пока идет сцена
        if (myCollider != null) myCollider.enabled = false;

        StartCoroutine(PlayCinematicSequence());
    }

    IEnumerator PlayCinematicSequence()
    {
        if (MusicManager.Instance != null)
        {
            MusicManager.Instance.EnsureMusicPlaying();
            MusicManager.Instance.SetSection("Value E");
            MusicManager.Instance.SetVolumeImmediate(1f);
        }

        if (oldController != null) { oldController.StopAllCoroutines(); oldController.enabled = false; }
        if (player != null) { player.SetCanMove(false); StartCoroutine(DoZoom(zoomFOV, 2.0f)); }


        if (thirdLamp != null)
        {
            thirdLamp.enabled = true;
            thirdLamp.intensity = 2.0f;
        }

        if (thirdLamp != null) flickerCoroutine = StartCoroutine(FlickerLightRoutine());

        if (smokeParticles != null) smokeParticles.Play();
        if (!smokeSound.IsNull)
            RuntimeManager.PlayOneShot(smokeSound, smokeParticles.transform.position);

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
        while (fadeTime < 1.5f)
        {
            fadeTime += Time.deltaTime;
            if (thirdLamp != null) thirdLamp.intensity = Mathf.Lerp(0f, 2.5f, fadeTime / 1.5f);
            yield return null;
        }
        if (thirdLamp != null) thirdLamp.intensity = 2.5f;

        yield return new WaitForSeconds(stareDuration);


        if (!explosionSound.IsNull) RuntimeManager.PlayOneShot(explosionSound, thirdLamp.transform.position);


        if (smokeParticles != null) smokeParticles.Stop();


        if (sparkParticles != null)
        {
            var main = sparkParticles.main;
            main.loop = false;
            main.startSpeed = 35f; 
            sparkParticles.Play();
        }


        if (sparksBaseLight != null) sparksBaseLight.SetActive(true);


        float dieTimer = 0f;
        float startIntensity = (thirdLamp != null) ? thirdLamp.intensity : 2.5f;
        bool sparksStopped = false;

        while (dieTimer < 1.5f)
        {
            dieTimer += Time.deltaTime;
            float t = dieTimer / 1.5f;

            if (thirdLamp != null) thirdLamp.intensity = Mathf.Lerp(startIntensity, 0f, t);

            if (sparksBaseLight != null)
            {
                var l = sparksBaseLight.GetComponent<Light>();
                if (l != null) l.intensity = Mathf.Lerp(5f, 0f, t);
            }

            if (dieTimer > 0.1f && !sparksStopped)
            {
                if (sparkParticles != null) sparkParticles.Stop();
                sparksStopped = true;
            }

            yield return null;
        }


        if (thirdLamp != null) { thirdLamp.enabled = false; thirdLamp.intensity = 0; }
        if (lampModel != null) { var r = lampModel.GetComponent<Renderer>(); if (r) r.material.DisableKeyword("_EMISSION"); }
        if (sparksBaseLight != null) sparksBaseLight.SetActive(false);

        if (sparkParticles != null) sparkParticles.Stop();
        if (smokeParticles != null) smokeParticles.Stop();

        yield return new WaitForSeconds(0.5f);

        if (electricPanelLight != null) electricPanelLight.ActivateBeacon();

        if (player != null) { StartCoroutine(DoZoom(60f, 0.5f)); player.isCinematic = false; player.SetCanMove(true); }
        if (umbrellaMan != null) { var chase = umbrellaMan.GetComponent<UmbrellaManChase>(); if (chase != null) chase.StartChase(); }
        
        // УБРАЛИ Destroy(gameObject), ТЕПЕРЬ ВСЁ РАБОТАЕТ ВЕЧНО
    }


    IEnumerator FlickerLightRoutine()
    {
        while (true)
        {
            if (!thirdLamp) yield break;
            thirdLamp.intensity = Random.Range(0.2f, 3f);
            yield return new WaitForSeconds(Random.Range(0.05f, 0.15f));
        }
    }

    IEnumerator DoZoom(float t, float d) { if (!player) yield break; float s = player.playerCamera.fieldOfView; float x = 0; while (x < d) { x += Time.deltaTime; player.playerCamera.fieldOfView = Mathf.Lerp(s, t, x / d); yield return null; } player.playerCamera.fieldOfView = t; }
}