// Assets/Scripts/NPC_Dialogue.cs
// (ПОЛНАЯ ИСПРАВЛЕННАЯ ВЕРСИЯ)
using UnityEngine;

[DisallowMultipleComponent]
public class NPC_Dialogue : MonoBehaviour
{
    [Header("Dialogue Data")]
    public DialogueLine[] dialogueLines; // Сюда ты перетащишь реплики

    [Header("UI")]
    public GameObject interactionPrompt; // Это, видимо, для подсказки [E]
    
    // --- ССЫЛКА НА МОЗГ ДИАЛОГОВ ---
    private DialogueManager dialogueManager; 

    void Start()
    {
        // Ищем DialogueManager в сцене при старте
        dialogueManager = FindObjectOfType<DialogueManager>();
        if (dialogueManager == null)
            Debug.LogError("NPC_Dialogue (на " + gameObject.name + ") не может найти DialogueManager!");
    }

    // --- ВОТ МЕТОД, КОТОРЫЙ ИЩЕТ PlayerController ---
    // Этот метод будет вызывать PlayerController
    public void TriggerDialogue()
    {
        if (dialogueManager != null)
        {
            // Проверяем, активен ли Квест 2
            QuestManager qm = QuestManager.instance;
            QuestObjective objective = qm?.currentQuest?.GetCurrentObjective();

            // Проверяем, что это Дверь ("door") И квест еще не выполнен
            if (objective != null && objective.targetID == "door" && objective.objectiveType == ObjectiveType.Interact && !objective.isComplete)
            {
                Debug.Log("Запускаем диалог с дверью...");
                dialogueManager.StartDialogue(dialogueLines);
            }
            // Проверяем, может это другой NPC (не дверь)?
            else if (objective == null || objective.targetID != "door")
            {
                Debug.Log("Запускаем обычный диалог с NPC...");
                dialogueManager.StartDialogue(dialogueLines); // Запускаем диалог в любом случае
            }
            else
            {
                Debug.Log("Сейчас нельзя поговорить через эту дверь (квест уже выполнен?).");
            }
        }
    }
}
