using System.Collections;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UI;
using FMODUnity;
using System.Collections.Generic;


public class DeathHandler : MonoBehaviour
{

    [Header("Ссылки")]
    public PlayerController playerController;
    public CanvasGroup gameOverUI;
    public KnifeManAI knifeManAI;
    public List<DoorController> doorControllers;

    void Start()
    {
        if (gameOverUI != null)
        {
            gameOverUI.alpha = 0;
            gameOverUI.interactable = false;
            gameOverUI.blocksRaycasts = false;
            gameOverUI.gameObject.SetActive(false);
        }
    }

    public void TriggerDeath()
    {
        gameOverUI.gameObject.SetActive(true);
        gameOverUI.alpha = 1;
        gameOverUI.interactable = true;
        gameOverUI.blocksRaycasts = true;



        foreach (DoorController door in doorControllers)
        {
            door.ResetDoor();
        }

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;


        if (playerController != null)
        {
            playerController.SetCanMove(false);
            playerController.isCinematic = true;
            playerController.transform.GetComponent<Flashlight>().SetBlocked(true);
        }
    }

    public void RestartGame()
    {
        StartCoroutine(RestartGameCoroutine());
    }

    public IEnumerator RestartGameCoroutine()
    {
        playerController.enabled = true;
        CharacterController cc = playerController.transform.GetComponent<CharacterController>();
        if (cc) cc.enabled = false;

        playerController.transform.position = new Vector3(-13.97f, -12.7062f, -20f);

        playerController.transform.rotation = Quaternion.Euler(0f, -0.853f, 0f);
        playerController.SetRotation(-0.853f, 0f);


        knifeManAI.ResetChasing();
        cc.enabled = true;
        gameOverUI.gameObject.SetActive(false);
        gameOverUI.alpha = 0;
        gameOverUI.interactable = false;
        gameOverUI.blocksRaycasts = false;

        playerController.transform.GetComponent<Flashlight>().SetBlocked(false);

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        playerController.isCinematic = false;
        playerController.SetCanMove(true);

        yield return new WaitForSeconds(2f);

        knifeManAI.StartChasing();
    }

    //    [Header("Ссылки")]
    //    public PlayerController playerController;
    //    public Volume horrorVolume;
    //    public CanvasGroup gameOverUI;
    //    public Transform playerCamera;


    //    [Header("Настройки Скримера")]
    //    [Tooltip("Как быстро происходит поворот и появление эффектов (в секундах). Для скримера ставь мало: 0.3 - 0.5")]
    //    public float scareDuration = 0.4f; 

    //    [Tooltip("Скорость поворота камеры. Для резкости ставь 15-20.")]
    //    public float turnSpeed = 20f;      

    //    [Header("Настройки Паузы")]
    //    [Tooltip("Сколько времени смотреть на врага ПОСЛЕ поворота, прежде чем появится надпись.")]
    //    public float stareDuration = 1.5f;  

    //    [Header("Настройка Взгляда")]
    //    [Tooltip("Высота глаз врага. Регулируй, чтобы смотреть в лицо.")]
    //    public float enemyEyeHeight = 1.5f;
    //    [Header("FMOD – Death Screamer")]
    //    [SerializeField] private EventReference deathScreamerEvent;
    //    private FMOD.Studio.EventInstance deathScreamerInstance;



    //    private bool isDead = false;

    //    void Start()
    //    {
    //        if (horrorVolume != null) horrorVolume.weight = 0;
    //        if (gameOverUI != null)
    //        {
    //            gameOverUI.alpha = 0; 
    //            gameOverUI.interactable = false;
    //            gameOverUI.blocksRaycasts = false;
    //            gameOverUI.gameObject.SetActive(false);
    //        }
    //    }

    //    public void TriggerDeath(Transform enemyFace)
    //    {
    //        if (PlayerController.isGameEnded) return;

    //        PlayerController.isGameEnded = true;
    //        isDead = true;

    //        if (!deathScreamerEvent.IsNull)
    //        {
    //            deathScreamerInstance = RuntimeManager.CreateInstance(deathScreamerEvent);

    //            RuntimeManager.AttachInstanceToGameObject(
    //    deathScreamerInstance,
    //    playerCamera.gameObject,
    //    (Rigidbody)null
    //);

    //            deathScreamerInstance.start();
    //        }

    //        if (playerController != null)
    //        {
    //            playerController.SetCanMove(false);
    //            playerController.isCinematic = true;

    //        }

    //        if (MusicManager.Instance != null)
    //        {
    //            MusicManager.Instance.FadeToVolume(0f, 0.4f);
    //        }

    //        StartCoroutine(DeathSequence(enemyFace));
    //    }

    //    private IEnumerator DeathSequence(Transform target)
    //    {
    //        if (MusicManager.Instance != null)
    //        {
    //            yield return new WaitForSeconds(2f);
    //            MusicManager.Instance.FadeToVolume(0f, 0.4f);
    //        }

    //        float timer = 0f;
    //        Quaternion startRotation = playerCamera.rotation;


    //        while (timer < scareDuration)
    //        {
    //            timer += Time.deltaTime;
    //            float progress = timer / scareDuration;

    //            if (target != null)
    //            {

    //                Vector3 lookTarget = target.position + Vector3.up * enemyEyeHeight;
    //                Vector3 direction = (lookTarget - playerCamera.position).normalized;
    //                Quaternion lookRotation = Quaternion.LookRotation(direction);


    //                playerCamera.rotation = Quaternion.Slerp(startRotation, lookRotation, progress * turnSpeed);
    //            }

    //            if (horrorVolume != null)
    //            {

    //                horrorVolume.weight = Mathf.Lerp(0f, 1f, progress);
    //            }

    //            yield return null;
    //        }


    //        if (horrorVolume != null) horrorVolume.weight = 1f;


    //        yield return new WaitForSeconds(stareDuration);


    //        if (gameOverUI != null)
    //        {
    //            gameOverUI.gameObject.SetActive(true);
    //            float fadeTimer = 0f;
    //            while (fadeTimer < 1f)
    //            {
    //                fadeTimer += Time.deltaTime;
    //                gameOverUI.alpha = fadeTimer; 
    //                yield return null;
    //            }
    //        }

    //        Cursor.lockState = CursorLockMode.None;
    //        Cursor.visible = true;

    //        gameOverUI.interactable = true;
    //        gameOverUI.blocksRaycasts = true;
    //        horrorVolume.weight = 0f;

    //        if (deathScreamerInstance.isValid())
    //        {
    //            deathScreamerInstance.release();
    //            deathScreamerInstance.clearHandle();
    //        }

    //    }
    //    public void RestartGame()
    //    {

    //        CinematicReveal cinematic = FindObjectOfType<CinematicReveal>();
    //        if (cinematic != null)
    //        {
    //            cinematic.ResetCinematicState();
    //        }


    //        PlayerController.isGameEnded = false;
    //        isDead = false;


    //        if (gameOverUI != null) gameOverUI.gameObject.SetActive(false);
    //        Cursor.lockState = CursorLockMode.Locked;
    //        Cursor.visible = false;
    //        if (horrorVolume != null) horrorVolume.weight = 0f;


    //        if (playerController != null)
    //        {
    //            playerController.isCinematic = false;
    //            playerController.SetCanMove(true);


    //        }


    //        if (MusicManager.Instance != null)
    //        {
    //            MusicManager.Instance.FadeToVolume(1f, 1f);
    //        }
    //    }
}