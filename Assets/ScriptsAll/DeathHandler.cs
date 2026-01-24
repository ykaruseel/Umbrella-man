using System.Collections;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UI;
using FMODUnity;


public class DeathHandler : MonoBehaviour
{
    [Header("Ссылки")]
    public PlayerController playerController;
    public Volume horrorVolume;
    public CanvasGroup gameOverUI;
    public Transform playerCamera;


    [Header("Настройки Скримера")]
    [Tooltip("Как быстро происходит поворот и появление эффектов (в секундах). Для скримера ставь мало: 0.3 - 0.5")]
    public float scareDuration = 0.4f; 

    [Tooltip("Скорость поворота камеры. Для резкости ставь 15-20.")]
    public float turnSpeed = 20f;      

    [Header("Настройки Паузы")]
    [Tooltip("Сколько времени смотреть на врага ПОСЛЕ поворота, прежде чем появится надпись.")]
    public float stareDuration = 1.5f;  

    [Header("Настройка Взгляда")]
    [Tooltip("Высота глаз врага. Регулируй, чтобы смотреть в лицо.")]
    public float enemyEyeHeight = 1.5f;
    [Header("FMOD – Death Screamer")]
    [SerializeField] private EventReference deathScreamerEvent;
    private FMOD.Studio.EventInstance deathScreamerInstance;



    private bool isDead = false;

    void Start()
    {
        if (horrorVolume != null) horrorVolume.weight = 0;
        if (gameOverUI != null)
        {
            gameOverUI.alpha = 0; 
            gameOverUI.interactable = false;
            gameOverUI.blocksRaycasts = false;
            gameOverUI.gameObject.SetActive(false);
        }
    }

    public void TriggerDeath(Transform enemyFace)
    {
        if (PlayerController.isGameEnded) return;

        PlayerController.isGameEnded = true;
        isDead = true;

        if (!deathScreamerEvent.IsNull)
        {
            deathScreamerInstance = RuntimeManager.CreateInstance(deathScreamerEvent);

            RuntimeManager.AttachInstanceToGameObject(
    deathScreamerInstance,
    playerCamera.gameObject,
    (Rigidbody)null
);

            deathScreamerInstance.start();
        }

        if (playerController != null)
        {
            playerController.SetCanMove(false);
            playerController.isCinematic = true;
        }

        StartCoroutine(DeathSequence(enemyFace));
    }

    private IEnumerator DeathSequence(Transform target)
    {
        float timer = 0f;
        Quaternion startRotation = playerCamera.rotation;
        
        // 1. ФАЗА СКРИМЕРА (Резкий поворот + Эффекты)
        while (timer < scareDuration)
        {
            timer += Time.deltaTime;
            float progress = timer / scareDuration;

            if (target != null)
            {
                // Считаем точку взгляда
                Vector3 lookTarget = target.position + Vector3.up * enemyEyeHeight;
                Vector3 direction = (lookTarget - playerCamera.position).normalized;
                Quaternion lookRotation = Quaternion.LookRotation(direction);
                
                // Lerp теперь будет очень быстрым из-за высокого turnSpeed и короткого scareDuration
                playerCamera.rotation = Quaternion.Slerp(startRotation, lookRotation, progress * turnSpeed);
            }

            if (horrorVolume != null)
            {
                // Эффекты нарастают резко
                horrorVolume.weight = Mathf.Lerp(0f, 1f, progress);
            }

            yield return null;
        }

        // Гарантируем, что эффекты включены на 100% в конце фазы
        if (horrorVolume != null) horrorVolume.weight = 1f;

        // 2. ФАЗА ПАУЗЫ (Смотрим на врага)
        // Ждем указанное время, ничего не делая — просто страх
        yield return new WaitForSeconds(stareDuration);

        // 3. ФАЗА ТЕКСТА (Появление UI)
        if (gameOverUI != null)
        {
            gameOverUI.gameObject.SetActive(true);
            float fadeTimer = 0f;
            while (fadeTimer < 1f)
            {
                fadeTimer += Time.deltaTime;
                gameOverUI.alpha = fadeTimer; 
                yield return null;
            }
        }

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        gameOverUI.interactable = true;
        gameOverUI.blocksRaycasts = true;
        horrorVolume.weight = 0f;

        if (deathScreamerInstance.isValid())
        {
            deathScreamerInstance.release();
            deathScreamerInstance.clearHandle();
        }

    }
}