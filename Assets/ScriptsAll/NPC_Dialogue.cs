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

    string npcID;

    void Start()
    {
        npcID = gameObject.name;
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
        if (!QuestManagerV2.Instance.IsGoalRequired(npcID, GoalType.TalkToNPC))
        {
            return;
        }

        if (dialogueTriggered)
            return;

        dialogueTriggered = true;

        if (questManager != null)
        {
            questManager.SendMessage("StopMusicForDialogue", SendMessageOptions.DontRequireReceiver);
        }

        if (playerController)
        {
            playerController.SetCanMove(false);
            
        }

        if (dialogueManager != null)
        {
            dialogueManager.StartDialogue(dialogueLines);
            StartCoroutine(HandleDialogueSequence());
        }
    }


private IEnumerator HandleDialogueSequence()
{
    while (dialogueManager != null &&
        dialogueManager.IsDialogueActive())
    {
        // Dialogue input is handled by PlayerController.
        // E advances the dialogue.
        // LMB remains available for the flashlight.
        yield return null;
    }

    QuestManagerV2 manager = QuestManagerV2.Instance;

    // Determine whether this was the final Lester dialogue.
    bool isFinalLesterDialogue =
        manager != null &&
        manager.IsQuestActive("Q2") &&
        manager.IsGoalRequired(
            npcID,
            GoalType.TalkToNPC
        );

    // Complete the quest
    if (manager != null &&
        manager.IsGoalRequired(
            npcID,
            GoalType.TalkToNPC
        ))
    {
        manager.ProcessAction(
            npcID,
            GoalType.TalkToNPC
        );
    }

    // Q2 is our final demo quest
    if (isFinalLesterDialogue)
    {
        Debug.Log(
            "[NPC Dialogue] Final Lester conversation finished"
        );

        if (QuestEvents.Instance != null)
        {
            yield return StartCoroutine(
                QuestEvents.Instance.FinishDemoAfterLester()
            );
        }
        else
        {
            Debug.LogError(
                "[NPC Dialogue] QuestEvents not found!"
            );
        }

        yield break;
    }

    // Normal dialogue behavior
    if (playerController != null)
    {
        CharacterController cc =
            playerController.GetComponent<CharacterController>();

        if (cc != null)
            cc.enabled = true;

        playerController.isCinematic = false;
        playerController.SetCanMove(true);
    }

    // Preserve old prototype behavior for other quests
    if (questManager != null)
    {
        questManager.UpdateQuestProgress(
            "door",
            ObjectiveType.Interact
        );

        questManager.ForceUpdateQuestText(
            "Follow the light"
        );
    }
}

}
