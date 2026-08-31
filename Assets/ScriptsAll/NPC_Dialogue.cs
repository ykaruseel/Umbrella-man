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
        while (dialogueManager != null && dialogueManager.IsDialogueActive())
        {
            
            if (Input.GetMouseButtonDown(0) || Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.E))
            {
                dialogueManager.DisplayNextSentence();
            }
            yield return null;
        }

        

        if (playerController)
        {
            playerController.SetCanMove(true);
        }

        if (QuestManagerV2.Instance.IsGoalRequired(npcID, GoalType.TalkToNPC))
        {
            QuestManagerV2.Instance.ProcessAction(npcID, GoalType.TalkToNPC);
        }

        if (questManager != null)
        {
            questManager.UpdateQuestProgress("door", ObjectiveType.Interact);
            questManager.ForceUpdateQuestText("Follow the light");
        }
    }
}
