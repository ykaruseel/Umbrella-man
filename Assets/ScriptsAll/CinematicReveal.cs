using UnityEngine;
using System.Collections;
using FMODUnity;

public class CinematicReveal : MonoBehaviour
{
    [Header("Activation Conditions")]
    // Впиши сюда Quest_FollowLight
    public string requiredQuestID; 

    [Header("Main Settings")]
    public PlayerController player;
    public Transform lookTarget;
    public GameObject umbrellaMan;
    
    // 👇 ВОТ ЭТА ПЕРЕМЕННАЯ, КОТОРОЙ У ТЕБЯ НЕ БЫЛО
    [Tooltip("Перетащи сюда объект _FollowLightController")]
    public FollowLightController oldController; 

    [Header("Lighting & Effects")]
    public Light thirdLamp;
    public GameObject lampModel;
    public ParticleSystem smokeParticles;
    public ParticleSystem sparkParticles;

    [Header("Audio")]
    public EventReference appearSound;
    public EventReference explosionSound;

    [Header("Timing")]
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
        if (hasTriggered || !other.CompareTag("Player")) return;

        // ПРОВЕРКА КВЕСТА С ОТЛАДКОЙ
        if (QuestManager.instance != null && QuestManager.instance.currentQuest != null)
        {
            string currentID = QuestManager.instance.currentQuest.questID;
            
            // 👇 ЭТА СТРОЧКА ПОКАЖЕТ ПРАВДУ В КОНСОЛИ
            Debug.Log($"[CinematicReveal] Текущий квест: '{currentID}' | Требуется: '{requiredQuestID}'");

            if (!string.IsNullOrEmpty(requiredQuestID) && currentID != requiredQuestID)
            {
                Debug.Log($"[CinematicReveal] ОТМЕНА: ID не совпадают!");
                return;
            }
        }
        else
        {
            Debug.Log("[CinematicReveal] QuestManager не найден или квеста нет!");
        }

        Debug.Log("[CinematicReveal] УСЛОВИЯ СОВПАЛИ! ЗАПУСК!");
        hasTriggered = true;
        StartCoroutine(PlayCinematicSequence());
    }

    IEnumerator PlayCinematicSequence()
    {
        // 0. ОТКЛЮЧАЕМ СТАРЫЙ КОНТРОЛЛЕР СВЕТА
        if (oldController != null)
        {
            oldController.StopAllCoroutines();
            oldController.enabled = false; // Выключаем его, чтобы не мешал
            Debug.Log("Cinematic: Старый контроллер света отключен.");
        }

        // 1. БЛОКИРОВКА ИГРОКА
        if (player != null)
        {
            player.SetCanMove(false);
            if (lookTarget != null) player.StartCinematicPan(lookTarget, 1.5f);
            StartCoroutine(DoZoom(zoomFOV, 2.0f));
        }

        // 2. МИГАНИЕ ЛАМПЫ И ДЫМ
        if (thirdLamp != null) flickerCoroutine = StartCoroutine(FlickerLightRoutine());
        if (smokeParticles != null) smokeParticles.Play();

        yield return new WaitForSeconds(smokeDuration);

        // 3. ПОЯВЛЕНИЕ ЧЕЛОВЕКА
        if (umbrellaMan != null)
        {
            umbrellaMan.SetActive(true);
            Vector3 lookPos = player.transform.position;
            lookPos.y = umbrellaMan.transform.position.y;
            umbrellaMan.transform.LookAt(lookPos);
            if (!appearSound.IsNull) RuntimeManager.PlayOneShot(appearSound, umbrellaMan.transform.position);
        }

        // 4. ОСТАНОВКА МИГАНИЯ -> ЯРКИЙ СВЕТ (ЧТОБЫ ВИДЕТЬ ВРАГА)
        if (flickerCoroutine != null) StopCoroutine(flickerCoroutine);
        if (thirdLamp != null)
        {
            thirdLamp.enabled = true;
            thirdLamp.intensity = 2.5f; 
        }

        yield return new WaitForSeconds(stareDuration);

        // 5. ВЗРЫВ ЛАМПЫ
        if (!explosionSound.IsNull) RuntimeManager.PlayOneShot(explosionSound, thirdLamp.transform.position);
        if (sparkParticles != null) sparkParticles.Play();

        if (thirdLamp != null)
        {
            thirdLamp.enabled = false; // Свет гаснет
            thirdLamp.intensity = 0;
        }

        // Гасим материал лампы
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

        // 6. СТАРТ ПОГОНИ
        if (player != null)
        {
            StartCoroutine(DoZoom(60f, 0.5f)); // Возвращаем камеру
            player.isCinematic = false; 
            player.SetCanMove(true);    
        }

        // Запускаем скрипт бега на враге
        if (umbrellaMan != null)
        {
            var chase = umbrellaMan.GetComponent<UmbrellaManChase>();
            if (chase != null) chase.StartChase();
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