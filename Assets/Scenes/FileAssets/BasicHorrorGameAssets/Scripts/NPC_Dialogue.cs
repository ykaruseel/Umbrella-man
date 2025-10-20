using UnityEngine;

// --- САМАЯ ПРОСТАЯ РАБОЧАЯ ВЕРСИЯ ---
// (Без проверки взгляда)
public class NPC_Dialogue : MonoBehaviour
{
    public DialogueLine[] dialogueLines; // Реплики
    public GameObject interactionPrompt; // Подсказка (может быть пустой)
    private bool playerInRange = false; // Игрок рядом?

    void Update()
    {
        // Если игрок рядом и нажал E
        if (playerInRange && Input.GetKeyDown(KeyCode.E))
        {
            DialogueManager manager = FindObjectOfType<DialogueManager>();
            if (manager == null) return; // Если менеджер не найден, выходим

            // Если диалог ещё не идёт
            if (!DialogueManager.IsDialogueActive)
            {
                // Начинаем диалог
                manager.StartDialogue(dialogueLines);
            }
            // Если диалог уже идёт
            else
            {
                // Проверяем, идёт ли печать
                if (manager.typingCoroutine != null)
                {
                    // Пропускаем печать
                    manager.SkipTyping();
                }
                else
                {
                    // Показываем следующую фразу
                    manager.DisplayNextSentence();
                }
            }
        }
    }

    // Когда игрок входит в триггер
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = true;
            // Показываем подсказку, если она есть
            if (interactionPrompt != null) interactionPrompt.SetActive(true);
        }
    }

    // Когда игрок выходит из триггера
    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = false;
            // Прячем подсказку
            if (interactionPrompt != null) interactionPrompt.SetActive(false);
            // Завершаем диалог
            FindObjectOfType<DialogueManager>()?.EndDialogue(); // ?. для безопасности
        }
    }
}
