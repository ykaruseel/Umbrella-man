using UnityEngine;
using System.Collections;
using FMODUnity;

[DisallowMultipleComponent]
public class NPC_Dialogue : MonoBehaviour
{
    public DialogueLine[] dialogueLines;
    public GameObject interactionPrompt;

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
        if (questManager == null || questManager.currentQuest == null)
            return;

        QuestObjective objective = questManager.currentQuest.GetCurrentObjective();
        if (objective == null)
            return;

        if (objective.targetID != "door" || objective.objectiveType != ObjectiveType.Interact)
            return;

        if (dialogueTriggered)
            return;

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
        while (dialogueManager != null && dialogueManager.IsDialogueActive())
            yield return null;

        if (playerController)
        {
            playerController.ZoomOut();
            playerController.SetCanMove(true);
            playerController.SetDialogueZoom(false);
        }

        if (questManager != null)
        {
            questManager.UpdateQuestProgress("door", ObjectiveType.Interact);
            questManager.ForceUpdateQuestText("Follow the light");
        }
    }
}



