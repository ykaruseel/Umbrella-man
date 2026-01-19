using UnityEngine;
using System.Collections;
using FMODUnity; // Используем FMOD

public class CinematicReveal : MonoBehaviour
{
    [Header("Activation Conditions")]
    // 👇 НОВОЕ: Сюда пиши ID квеста (например, Quest_Explore)
    public string requiredQuestID; 

    [Header("Main Settings")]
    public PlayerController player;          // Ссылка на игрока
    public Transform lookTarget;             // Пустой объект в углу
    public GameObject umbrellaMan;           // Объект врага
    
    [Header("Lighting & Effects")]
    public Light thirdLamp;                  // Лампа
    public GameObject lampModel;             // Модель лампы
    public ParticleSystem smokeParticles;    // Дым
    public ParticleSystem sparkParticles;    // Искры

    [Header("Audio (FMOD)")]
    public EventReference lightFlickerSound; 
    public EventReference appearSound;       
    public EventReference explosionSound;    

    [Header("Timing Settings")]
    public float smokeDuration = 5.0f;       
    public float stareDuration = 2.0f;       
    public float zoomFOV = 40f;              
    
    private bool hasTriggered = false;
    private Coroutine flickerCoroutine;

    void Start()
    {
        if (umbrellaMan != null) umbrellaMan.SetActive(false);
        if (smokeParticles != null) smokeParticles.Stop();
        if (sparkParticles != null) sparkParticles.Stop();
    }

    private void OnTriggerEnter(Collider other)
    {
        // Сначала проверяем, что это Игрок и триггер еще не сработал
        if (hasTriggered || !other.CompareTag("Player")) return;

        // 👇 НОВАЯ ПРОВЕРКА КВЕСТА
        // Если поле requiredQuestID не пустое, проверяем текущий квест
        if (!string.IsNullOrEmpty(requiredQuestID))
        {
            if (QuestManager.instance != null && QuestManager.instance.currentQuest != null)
            {
                // Если ID текущего квеста НЕ совпадает с нужным -> выходим
                if (QuestManager.instance.currentQuest.questID != requiredQuestID)
                {
                    return; 
                }
            }
        }

        // Если квест совпал (или ID не был задан), запускаем сцену
        hasTriggered = true;
        StartCoroutine(PlayCinematicSequence());
    }

    IEnumerator PlayCinematicSequence()
    {
        // -----------------------------------------------------------
        // 1. БЛОКИРОВКА И ПОВОРОТ КАМЕРЫ
        // -----------------------------------------------------------
        if (player != null)
        {
            player.SetCanMove(false); 
            if (lookTarget != null) 
                player.StartCinematicPan(lookTarget, 1.5f);
            
            StartCoroutine(DoZoom(zoomFOV, 2.0f));
        }

        // -----------------------------------------------------------
        // 2. МИГАНИЕ СВЕТА И ДЫМ
        // -----------------------------------------------------------
        if (thirdLamp != null)
        {
            flickerCoroutine = StartCoroutine(FlickerLightRoutine());
        }

        if (smokeParticles != null) smokeParticles.Play();

        yield return new WaitForSeconds(smokeDuration);

        // -----------------------------------------------------------
        // 3. ПОЯВЛЕНИЕ ЧЕЛОВЕКА
        // -----------------------------------------------------------
        if (umbrellaMan != null)
        {
            umbrellaMan.SetActive(true);
            
            Vector3 lookPos = player.transform.position;
            lookPos.y = umbrellaMan.transform.position.y; 
            umbrellaMan.transform.LookAt(lookPos);

            if (!appearSound.IsNull) RuntimeManager.PlayOneShot(appearSound, umbrellaMan.transform.position);
        }

        yield return new WaitForSeconds(stareDuration);

        // -----------------------------------------------------------
        // 4. ВЗРЫВ ЛАМПЫ
        // -----------------------------------------------------------
        if (flickerCoroutine != null) StopCoroutine(flickerCoroutine);

        if (!explosionSound.IsNull) RuntimeManager.PlayOneShot(explosionSound, thirdLamp.transform.position);
        if (sparkParticles != null) sparkParticles.Play();

        if (thirdLamp != null)
        {
            thirdLamp.enabled = false;
            thirdLamp.intensity = 0;
        }

        if (lampModel != null)
        {
            var renderer = lampModel.GetComponent<Renderer>();
            if (renderer != null)
            {
                renderer.material.DisableKeyword("_EMISSION");
                renderer.material.globalIlluminationFlags = MaterialGlobalIlluminationFlags.EmissiveIsBlack;
                renderer.material.SetColor("_EmissionColor", Color.black);
            }
        }

        yield return new WaitForSeconds(0.5f);

        // -----------------------------------------------------------
        // 5. СТАРТ ПОГОНИ
        // -----------------------------------------------------------
        
        if (player != null)
        {
            StartCoroutine(DoZoom(60f, 0.5f)); 
            player.isCinematic = false; 
            player.SetCanMove(true);    
        }

        if (umbrellaMan != null)
        {
            var chaseScript = umbrellaMan.GetComponent<UmbrellaManChase>();
            if (chaseScript != null)
            {
                chaseScript.StartChase();
            }
            else
            {
                Debug.LogWarning("На объекте UmbrellaMan нет скрипта UmbrellaManChase!");
            }
        }
        
        if (QuestManager.instance != null)
        {
            // QuestManager.instance.TriggerChaseScene(); 
        }

        Destroy(gameObject, 2f);
    }

    IEnumerator FlickerLightRoutine()
    {
        while (true)
        {
            if (thirdLamp == null) yield break;
            
            thirdLamp.intensity = Random.Range(0.2f, 3.0f);
            yield return new WaitForSeconds(Random.Range(0.05f, 0.15f));
            
            if (Random.value > 0.7f)
            {
                thirdLamp.enabled = false;
                yield return new WaitForSeconds(Random.Range(0.05f, 0.2f));
                thirdLamp.enabled = true;
            }
        }
    }

    IEnumerator DoZoom(float targetFOV, float duration)
    {
        if (player == null || player.playerCamera == null) yield break;

        float startFOV = player.playerCamera.fieldOfView;
        float time = 0f;

        while (time < duration)
        {
            time += Time.deltaTime;
            player.playerCamera.fieldOfView = Mathf.Lerp(startFOV, targetFOV, time / duration);
            yield return null;
        }
        player.playerCamera.fieldOfView = targetFOV;
    }
}