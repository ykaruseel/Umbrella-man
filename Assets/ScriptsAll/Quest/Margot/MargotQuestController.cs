using System.Collections;
using FMODUnity;
using UnityEngine;

// Main-scene controller for Q_MARGOT, one-time interaction on apartment 206.
// Uses existing DialogueManager and DialogueCameraSystem.
// The actual quest completes only after Daniel's final monologue.
public sealed class MargotQuestController : MonoBehaviour
{
    [Header("Quest IDs (match QuestData)")]
    [SerializeField] private string requiredQuestID = "Q_MARGOT";
    [SerializeField] private string completionTargetID = "MARGOT_COMPLETE";

    [Header("Scene references")]
    [SerializeField] private PlayerController player;
    [SerializeField] private DialogueManager dialogueManager;
    [SerializeField] private DialogueCameraSystem cameraSystem;
    [SerializeField] private Animator margotAnimator;
    [SerializeField] private OutlineInteractable outline;

    [Header("Door references")]
    [Tooltip("Empty hinge pivot rotating ONLY the door leaf, cross and handle; never the frame.")]
    [SerializeField] private Transform doorPivot;
    [SerializeField] private Vector3 doorOpenEuler = new Vector3(0f, -90f, 0f);
    [SerializeField, Min(0.1f)] private float doorDuration = 1.2f;
    [SerializeField, Min(0f)] private float waitAfterKnock = 10f;
    [SerializeField, Min(0f)] private float delayBeforeMonologue = 0.25f;

    [Header("FMOD — assign existing studio events")]
    [SerializeField] private EventReference knockEvent;
    [SerializeField] private EventReference openEvent;
    [SerializeField] private EventReference closeEvent;

    [Header("Daniel Cinematic Visual")]
[SerializeField] private GameObject danielVisual;
[SerializeField] private Transform danielStandPoint;

    [Header("Scenario — line by line; assign voiceEvent individually when available")]
    [SerializeField] private DialogueLine[] conversation =
    {
        new DialogueLine { speakerName = "Old Margo", sentence = "Hello, dear. What can I do for you?" },
        new DialogueLine { speakerName = "Daniel", sentence = "Hello, Margo. It's me, Daniel." },
        new DialogueLine { speakerName = "Old Margo", sentence = "Daniel! I haven't seen you in ages. And I haven't seen you at church in even longer." },
        new DialogueLine { speakerName = "Daniel", sentence = "I'm sorry. I just don't think I'll find what I'm looking for there." },
        new DialogueLine { speakerName = "Old Margo", sentence = "Oh, Daniel, I think that's exactly where you'll find it. But enough about that. What brings you here so early?" },
        new DialogueLine { speakerName = "Daniel", sentence = "My neighbor, Lester, sent me to you. You've lived here for a long time, haven't you?" },
        new DialogueLine { speakerName = "Old Margo", sentence = "Ever since the church was built. Now, how many years has it been? Twenty, I think. Or maybe thirty... Either way, it's been a long time." },
        new DialogueLine { speakerName = "Daniel", sentence = "Then you probably know a lot of the people who have lived here. Have you ever heard of the man with the umbrella?" },
        new DialogueLine { speakerName = "Old Margo", sentence = "The man with the umbrella?" },
        new DialogueLine { speakerName = "Daniel", sentence = "Yeah. Tall, dressed in black, always carrying an umbrella. I think he's watching me. Or maybe even following me." },
        new DialogueLine { speakerName = "Old Margo", sentence = "Hmm... No, I've never seen him. And I've never seen him at church either. I don't think anyone there has. But I don't think you should be afraid of him. If he's following you, then perhaps you'll be meeting him soon." },
        new DialogueLine { speakerName = "Daniel", sentence = "What do you mean? How could you possibly know that?" },
        new DialogueLine { speakerName = "Old Margo", sentence = "Perhaps he doesn't mean any harm. Perhaps, once you meet him, he'll show you kindness and mercy. And if his intentions are truly bad... then perhaps he has his reasons. Tell me, Daniel... have you sinned?" },
        new DialogueLine { speakerName = "Daniel", sentence = "Have I sinned? What are you talking about? Just tell me who he is." },
        new DialogueLine { speakerName = "Old Margo", sentence = "... I have a feeling you've already asked that question. You should speak to the old man in the basement instead. Just be careful. He's had a hard life. His family died in a fire many years ago, and now he's gravely ill. He's lived here much longer than I have. Maybe he knows something. God be with you, Daniel." }
    };

    [Header("Only after door closes")]
    [SerializeField] private DialogueLine[] finalMonologue =
    {
        new DialogueLine { speakerName = "Daniel", sentence = "What did she mean by that?" },
        new DialogueLine { speakerName = "Daniel", sentence = "What do my sins have to do with any of this?" },
        new DialogueLine { speakerName = "Daniel", sentence = "None of this makes any sense. Hopefully the old man in the basement knows more." }
    };

    private bool hasStarted;
    private bool finished;
    private Quaternion closedRotation;

    private void Awake()
    {
        if (player == null) player = FindFirstObjectByType<PlayerController>();
        if (dialogueManager == null) dialogueManager = FindFirstObjectByType<DialogueManager>();
        if (outline == null) outline = GetComponent<OutlineInteractable>();
        if (doorPivot != null) closedRotation = doorPivot.localRotation;
        if (margotAnimator != null) margotAnimator.SetBool("Talk", false);
    }

    private void Update()
    {
        // Avoid highlighting prior to Q_MARGOT or after first interaction.
        if (outline != null)
        {
            bool available = !hasStarted && !finished &&
                QuestManagerV2.Instance != null &&
                QuestManagerV2.Instance.IsQuestActive(requiredQuestID);
            if (!available) outline.Hide();
            outline.isBlocked = !available;
        }
    }

    // Called by the existing raycast handling in PlayerController.
    public bool TryInteract()
    {
        if (hasStarted || finished) return false;
        if (QuestManagerV2.Instance == null ||
            !QuestManagerV2.Instance.IsQuestActive(requiredQuestID)) return false;
        if (player == null || dialogueManager == null || cameraSystem == null || doorPivot == null)
        {
            Debug.LogError("[Margot] Missing Player, DialogueManager, CameraSystem or DoorPivot reference.", this);
            return false;
        }
        hasStarted = true;
        StartCoroutine(QuestRoutine());
        return true;
    }

    private IEnumerator QuestRoutine()
    {
        if (outline != null) { outline.isBlocked = true; outline.Hide(); }
        PlayOneShot(knockEvent);
        Debug.Log("[Margot] Knock! Waiting 10 seconds...");

        // QDD: player may still move while awaiting the answer.
        yield return new WaitForSeconds(waitAfterKnock);

        player.SetCanMove(false);
        player.isCinematic = true;
        bool oldCanPause = Pause.canPause;
        Pause.canPause = false;

        PlayOneShot(openEvent);
        Quaternion openRotation = closedRotation * Quaternion.Euler(doorOpenEuler);
        yield return AnimateDoor(openRotation);



GameObject cinematicDaniel = danielVisual;

if (cinematicDaniel == null && QuestEvents.Instance != null)
{
    cinematicDaniel = QuestEvents.Instance.DanielsModel;
}

bool danielWasActive = false;
Vector3 oldDanielPosition = Vector3.zero;
Quaternion oldDanielRotation = Quaternion.identity;

if (cinematicDaniel != null)
{
    // Save original state for other cutscenes.
    danielWasActive = cinematicDaniel.activeSelf;
    oldDanielPosition = cinematicDaniel.transform.position;
    oldDanielRotation = cinematicDaniel.transform.rotation;

    if (danielStandPoint != null)
    {
        cinematicDaniel.transform.SetPositionAndRotation(
            danielStandPoint.position,
            danielStandPoint.rotation
        );
    }

    cinematicDaniel.SetActive(true);

    Debug.Log("[Margot] Daniel cinematic model enabled.");
}
else
{
    Debug.LogWarning(
        "[Margot] Daniel visual is not assigned."
    );
}


        // Cinemachine priorities & dialogue text are controlled by existing systems.
        DialogueCameraSystem previousCameraSystem = dialogueManager.currentCameraSystem;
        dialogueManager.currentCameraSystem = cameraSystem;
        cameraSystem.StartDialogue();
        if (margotAnimator != null) margotAnimator.SetBool("Talk", false);
        dialogueManager.StartDialogue(conversation);
        while (dialogueManager.IsDialogueActive()) yield return null;

        if (margotAnimator != null) margotAnimator.SetBool("Talk", false);
        if (cinematicDaniel != null)
        {
            cinematicDaniel.SetActive(false);

            cinematicDaniel.transform.SetPositionAndRotation(
                oldDanielPosition,
                oldDanielRotation
            );

            cinematicDaniel.SetActive(danielWasActive);
        }
        PlayOneShot(closeEvent);
        yield return AnimateDoor(closedRotation);
        yield return new WaitForSeconds(delayBeforeMonologue);

        // No new cinematic camera cuts during Daniel's closing monologue.
        dialogueManager.currentCameraSystem = null;
        dialogueManager.StartDialogue(finalMonologue);
        while (dialogueManager.IsDialogueActive()) yield return null;
        dialogueManager.currentCameraSystem = previousCameraSystem;

        player.isCinematic = false;
        player.SetCanMove(true);
        Pause.canPause = oldCanPause;

        finished = true;
        Debug.Log("[Margot] All dialogue + closing monologue finished; completing Q_MARGOT.");
        if (QuestManagerV2.Instance != null &&
            QuestManagerV2.Instance.IsGoalRequired(completionTargetID, GoalType.ReachPoint))
        {
            QuestManagerV2.Instance.ProcessAction(completionTargetID, GoalType.ReachPoint);
        }
    }

    private IEnumerator AnimateDoor(Quaternion goal)
    {
        Quaternion start = doorPivot.localRotation;
        float elapsed = 0f;
        while (elapsed < doorDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.SmoothStep(0f, 1f, Mathf.Clamp01(elapsed / doorDuration));
            doorPivot.localRotation = Quaternion.Slerp(start, goal, t);
            yield return null;
        }
        doorPivot.localRotation = goal;
    }

    private void PlayOneShot(EventReference reference)
    {
        if (!reference.IsNull) RuntimeManager.PlayOneShotAttached(reference, gameObject);
    }
}
