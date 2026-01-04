using System.Collections;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UI;

public class DeathHandler : MonoBehaviour
{
    [Header("Ссылки")]
    public PlayerController playerController;
    public Volume horrorVolume;
    public CanvasGroup gameOverUI;
    public Transform playerCamera;

    [Header("Настройки Смерти")]
    public float scareDuration = 1.5f; // Длительность испуга
    public float turnSpeed = 5f;       // Скорость поворота
    
    [Header("Настройка Взгляда")]
    [Tooltip("Высота глаз врага от пола. Если смотрит слишком высоко — уменьши, если в грудь — увеличь.")]
    public float enemyEyeHeight = 1.6f; // <--- ВОТ ЭТИМ ТЕПЕРЬ МОЖНО РЕГУЛИРОВАТЬ

    private bool isDead = false;

    void Start()
    {
        if (horrorVolume != null) horrorVolume.weight = 0;
        if (gameOverUI != null)
        {
            gameOverUI.alpha = 0; 
            gameOverUI.gameObject.SetActive(false);
        }
    }

    public void TriggerDeath(Transform enemyFace)
    {
        if (isDead) return;
        isDead = true;

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
        
        // Используем переменную из инспектора для высоты
        Vector3 lookTarget = target.position + Vector3.up * enemyEyeHeight;

        while (timer < scareDuration)
        {
            timer += Time.deltaTime;
            float progress = timer / scareDuration;

            if (target != null)
            {
                // Пересчитываем точку каждый кадр (вдруг враг чуть двинулся)
                lookTarget = target.position + Vector3.up * enemyEyeHeight;

                Vector3 direction = (lookTarget - playerCamera.position).normalized;
                Quaternion lookRotation = Quaternion.LookRotation(direction);
                playerCamera.rotation = Quaternion.Slerp(startRotation, lookRotation, progress * turnSpeed);
            }

            if (horrorVolume != null)
            {
                horrorVolume.weight = Mathf.Lerp(0f, 1f, progress);
            }

            yield return null;
        }

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
    }
}