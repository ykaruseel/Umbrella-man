using UnityEngine;
using System.Collections;

public class PlaceholderDialogueTrigger : MonoBehaviour
{
    [Header("Ссылка на систему камер")]
    public DialogueCameraSystem cameraSystem;

    private bool hasTriggered = false;

    private void OnTriggerEnter(Collider other)
    {
        // Как только игрок касается кубика
        if (other.CompareTag("Player") && !hasTriggered)
        {
            hasTriggered = true;
            StartCoroutine(RunTestDialogue());
        }
    }

    private IEnumerator RunTestDialogue()
    {
        // 1. Старт диалога (Главная камера отключится, включится Камера 1)
        cameraSystem.StartDialogue();

        // Имитируем, что персонаж говорит первую фразу 3 секунды
        yield return new WaitForSeconds(3f);

        // 2. Смена реплики (Мгновенный Cut на Камеру 2)
        cameraSystem.NextLine();

        // Имитируем, что персонаж говорит вторую фразу 3 секунды
        yield return new WaitForSeconds(3f);

        // 3. Конец диалога (Камеры выключаются, возвращаемся к игроку)
        cameraSystem.EndDialogue();
    }
}
