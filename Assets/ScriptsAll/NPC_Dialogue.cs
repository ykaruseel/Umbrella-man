using UnityEngine;
using System.Collections;
using FMODUnity;

[DisallowMultipleComponent]
public class NPC_Dialogue : MonoBehaviour
{
    public DialogueLine[] dialogueLines;
    public GameObject interactionPrompt;

    [Header("СВЯЗЬ С МОНСТРОМ")]
    // 👇 ПЕРЕТАЩИ СЮДА TRIGGER_REVEAL В ИНСПЕКТОРЕ 👇
    public CinematicReveal monsterTriggerScript; 

    [Header("FMOD")]
    [SerializeField] private EventReference knockSound;

    private DialogueManager dialogueManager;
    private QuestManager questManager;
    private PlayerController playerController;
    private bool dialogueTriggered = false;

    void Start()
    {
        dialogueManager = FindFirstObjectByType<DialogueManager>();
        questManager = QuestManager.instance;
        playerController = FindFirstObjectByType<PlayerController>();
    }

    public void PlayKnock()
    {
        if (!knockSound.IsNull)
            RuntimeManager.PlayOneShotAttached(knockSound, gameObject);
    }

    public void TriggerDialogue()
    {
        // Проверки квестов (если нужны)
        if (questManager != null && questManager.currentQuest != null)
        {
            QuestObjective objective = questManager.currentQuest.GetCurrentObjective();
            if (objective != null)
            {
                if (objective.targetID != "door" || objective.objectiveType != ObjectiveType.Interact)
                    return;
            }
        }

        if (dialogueTriggered) return;

        dialogueTriggered = true;

        questManager.SendMessage("StopMusicForDialogue", SendMessageOptions.DontRequireReceiver);

        if (playerController)
        {
            playerController.SetCanMove(false);
            playerController.SetDialogueZoom(true);
            playerController.ZoomIn();
        }

        if (dialogueManager != null)
        {
            dialogueManager.StartDialogue(dialogueLines);
            StartCoroutine(HandleDialogueSequence());
        }
    }

    private IEnumerator HandleDialogueSequence()
    {
        // Ждем пока игрок дочитает диалог
        while (dialogueManager != null && dialogueManager.IsDialogueActive())
            yield return null;

        // Возвращаем управление
        if (playerController)
        {
            playerController.ZoomOut();
            playerController.SetCanMove(true);
            playerController.SetDialogueZoom(false);
        }

        // Обновляем квест
        if (questManager != null)
        {
            questManager.UpdateQuestProgress("door", ObjectiveType.Interact);
            questManager.ForceUpdateQuestText("Follow the light");
        }

        // 🔥 САМОЕ ВАЖНОЕ: ВКЛЮЧАЕМ МОНСТРА 🔥
        if (monsterTriggerScript != null)
        {
            monsterTriggerScript.canActivate = true;
            Debug.Log("NPC_Dialogue: Диалог закончен -> МОНСТР ТЕПЕРЬ АКТИВЕН!");
        }
        else
        {
            Debug.LogWarning("NPC_Dialogue: ЗАБЫЛ ПЕРЕТАЩИТЬ Trigger_Reveal В ПОЛЕ СКРИПТА!");
        }
    }
}



