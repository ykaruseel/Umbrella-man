// 📁 Assets/ScriptsAll/NPC_Dialogue.cs
using UnityEngine;
using System.Collections; // ✅ обязательно для IEnumerator

[DisallowMultipleComponent]
public class NPC_Dialogue : MonoBehaviour
{
    [Header("Dialogue Data")]
    public DialogueLine[] dialogueLines; // Реплики NPC

    [Header("UI")]
    public GameObject interactionPrompt; // Подсказка [E]

    private DialogueManager dialogueManager;
    private QuestManager questManager;
    private PlayerController playerController;
    private bool dialogueTriggered = false;

    void Start()
    {
        dialogueManager = FindObjectOfType<DialogueManager>();
        questManager = QuestManager.instance;
        playerController = FindObjectOfType<PlayerController>();

        if (dialogueManager == null)
            Debug.LogError("NPC_Dialogue: DialogueManager не найден в сцене!");
        if (questManager == null)
            Debug.LogError("NPC_Dialogue: QuestManager не найден в сцене!");
    }

    public void TriggerDialogue()
    {
        // --- Проверка: диалог можно начать только после 1-го квеста ---
        if (questManager == null || questManager.currentQuest == null)
        {
            Debug.Log("❌ Нет активного квеста — диалог недоступен.");
            return;
        }

        QuestObjective objective = questManager.currentQuest.GetCurrentObjective();
        if (objective == null)
        {
            Debug.Log("❌ Нет текущей цели — диалог недоступен.");
            return;
        }

        // Разрешаем диалог только если это квест 2 и цель — дверь
        if (objective.targetID != "door" || objective.objectiveType != ObjectiveType.Interact)
        {
            Debug.Log("🚪 Диалог с дверью сейчас недоступен (ещё не время).");
            return;
        }

        if (dialogueTriggered)
        {
            Debug.Log("🔁 Диалог уже был запущен — пропуск.");
            return;
        }

        dialogueTriggered = true;

        // Останавливаем музыку
        questManager.SendMessage("StopMusicForDialogue", SendMessageOptions.DontRequireReceiver);

        // Блокируем движение
        if (playerController)
        {
            playerController.SetCanMove(false);
            playerController.SetDialogueZoom(true);
        }

        // Запускаем диалог
        if (dialogueManager != null)
        {
            Debug.Log("💬 Запускается диалог с соседом...");
            dialogueManager.StartDialogue(dialogueLines);

            // Когда диалог завершён — возвращаем управление
            StartCoroutine(HandleDialogueSequence());
        }
    }

    private IEnumerator HandleDialogueSequence()
    {
        // Ждём пока активен диалог
        while (dialogueManager != null && dialogueManager.IsDialogueActive())
            yield return null;

        // Разблокируем движение и убираем зум
        if (playerController)
        {
            playerController.SetCanMove(true);
            playerController.SetDialogueZoom(false);
        }

        // Сообщаем квест-системе, что дверь "использована"
        if (questManager != null)
        {
            questManager.UpdateQuestProgress("door", ObjectiveType.Interact);
        }

        Debug.Log("📜 Диалог завершён — запускается мигание света и квест 3.");
    }
}

