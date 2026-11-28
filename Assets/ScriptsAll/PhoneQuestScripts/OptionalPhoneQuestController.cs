using System.Collections;
using FMOD.Studio;
using FMODUnity;
using TMPro;
using UnityEngine;
using UnityEngine.Events;

// Legacy class name retained to preserve Unity scene references.
// The phone is now a REQUIRED main quest, Q_PHONE, before Q2 (Lester).
public class OptionalPhoneQuestController : MonoBehaviour
{
    private enum PhoneState { Dormant, Ringing, InConversation, Completed }

    [Header("References")]
    [SerializeField] private PlayerController player;
    [SerializeField] private DialogueManager dialogueManager;
    [SerializeField] private PhoneAnimationController animationController;
    [SerializeField] private OutlineInteractable outline;

    [Header("Phone audio (FMOD)")]
    [SerializeField] private EventReference ringEvent;
    [SerializeField] private EventReference pickupEvent;
    [SerializeField] private EventReference hangupEvent;

    [Header("Legacy optional UI - no longer shown for the main phone quest")]
    [SerializeField] private TMP_Text optionalHint;
    [SerializeField] private TMP_Text skipHint;
    [SerializeField, Min(0f)] private float optionalHintDuration = 4f;

    [Header("Main quest IDs")]
    [SerializeField] private string requiredQuestID = "Q_PHONE";
    [SerializeField] private string completionTargetID = "PHONE_CALL_DONE";

    [Header("After-call cinematic")]
    [Tooltip("Empty GameObject at the former gramophone location; keep active.")]
    [SerializeField] private Transform gramophoneLookTarget;
    [SerializeField, Min(0f)] private float pauseAfterHangup = 1.15f;
    [SerializeField, Min(0f)] private float lookAtDuration = 1.7f;
    [SerializeField, Min(0f)] private float holdAfterLooking = 0.3f;

    [Header("Phone conversation")]
    [SerializeField] private DialogueLine[] conversation =
    {
        new DialogueLine { speakerName = "Daniel", sentence = "Hello?" },
        new DialogueLine { speakerName = "Emily", sentence = "Hi, Daniel. It's Emily." },
        new DialogueLine { speakerName = "Daniel", sentence = "Emily! God, how have you been? I haven't heard from you since... the funeral." },
        new DialogueLine { speakerName = "Emily", sentence = "I've been trying to figure out how to keep living... without him." },
        new DialogueLine { speakerName = "Daniel", sentence = "I miss him too. Work just isn't the same without him. Did something happen? Is there anything I can do?" },
        new DialogueLine { speakerName = "Emily", sentence = "No, nothing like that. I just wanted to return the book you lent Tommy." },
        new DialogueLine { speakerName = "Daniel", sentence = "I'd completely forgotten about it. You can keep it if you want." },
        new DialogueLine { speakerName = "Emily", sentence = "I'd rather not. There are already so many of his things here, and I don't know what to do with them. Besides, it's not really my kind of book. You can come by and pick it up today. I'll be at work, so I'll just leave the door unlocked." },
        new DialogueLine { speakerName = "Daniel", sentence = "Alright, Emily. I'll come get it. And... I'm sorry for your loss." },
        new DialogueLine { speakerName = "Emily", sentence = "Thank you, Daniel." }
    };

    [Header("Daniel monologue after looking at the missing gramophone")]
    [SerializeField] private DialogueLine[] afterCallMonologue =
    {
        new DialogueLine { speakerName = "Daniel", sentence = "Damn. The gramophone's gone." },
        new DialogueLine { speakerName = "Daniel", sentence = "So it really did happen yesterday." },
        new DialogueLine { speakerName = "Daniel", sentence = "I should talk to Lester about this. He lives in apartment 307." }
    };

    [Header("Tommy's book remains an OPTIONAL side quest")]
    [SerializeField] private UnityEvent onBookQuestUnlocked;

    private PhoneState state = PhoneState.Dormant;
    private EventInstance ringingInstance;
    private bool bookQuestUnlocked;

    public bool BookQuestUnlocked => bookQuestUnlocked;

    private void Awake()
    {
        if (player == null) player = FindFirstObjectByType<PlayerController>();
        if (dialogueManager == null) dialogueManager = FindFirstObjectByType<DialogueManager>();
        if (animationController == null) animationController = GetComponent<PhoneAnimationController>();
        if (outline == null) outline = GetComponent<OutlineInteractable>();
        SetOutline(false);
        if (optionalHint != null) optionalHint.gameObject.SetActive(false);
        ShowSkipHint(false);
    }

    // Called by QuestManagerV2 when Q_PHONE becomes active.
    [ContextMenu("DEBUG: Start phone ringing")]
    public void BeginAfterWake()
    {
        if (state != PhoneState.Dormant) return;

        state = PhoneState.Ringing;
        if (animationController != null) animationController.StartRinging();
        SetOutline(true);
        if (optionalHint != null) optionalHint.gameObject.SetActive(false);
        StartRingAudio();
        Debug.Log("[PhoneQuest] Q_PHONE: telephone is ringing.");
    }

    [ContextMenu("DEBUG: Activate Q_PHONE + ring (Play Mode)")]
    private void DebugActivateQuestAndRing()
    {
        if (!Application.isPlaying) return;
        QuestManagerV2 manager = QuestManagerV2.Instance;
        if (manager != null && manager.ActivateQuestByID(requiredQuestID, true))
            BeginAfterWake();
    }

    public bool TryInteract()
    {
        if (state != PhoneState.Ringing) return false;

        QuestManagerV2 manager = QuestManagerV2.Instance;
        if (manager == null || !manager.IsQuestActive(requiredQuestID))
        {
            Debug.LogWarning("[PhoneQuest] Phone cannot be answered until Q_PHONE is active.");
            return false;
        }

        state = PhoneState.InConversation;
        StartCoroutine(CallRoutine());
        return true;
    }

    // Legacy method referenced by LesterDoor: no longer cancels a required quest.
    public void CancelIfUnanswered()
    {
        if (state == PhoneState.Ringing)
            Debug.LogWarning("[PhoneQuest] Q_PHONE is mandatory and cannot be cancelled by LesterDoor.");
    }

    private IEnumerator CallRoutine()
    {
        StopRingAudio();
        if (animationController != null) animationController.StopRinging();
        SetOutline(false);
        if (optionalHint != null) optionalHint.gameObject.SetActive(false);

        if (player != null)
        {
            player.SetCanMove(false);
            player.isCinematic = true;
        }
        Pause.canPause = false;

        PlayOneShot(pickupEvent);
        if (animationController != null)
            yield return StartCoroutine(animationController.PickUp());

        // Keep standard first-person view; avoid Lester's dialogue cameras.
        DialogueCameraSystem previousCameraSystem =
            dialogueManager != null ? dialogueManager.currentCameraSystem : null;
        if (dialogueManager != null)
            dialogueManager.currentCameraSystem = null;

        yield return StartCoroutine(RunDialogue(conversation));

        PlayOneShot(hangupEvent);
        if (animationController != null)
            yield return StartCoroutine(animationController.PutBack());

        // The phone call has finished. Pause, then look towards the missing gramophone.
        if (pauseAfterHangup > 0f)
            yield return new WaitForSeconds(pauseAfterHangup);

        if (player != null && gramophoneLookTarget != null)
            yield return StartCoroutine(LookAtGramophone());
        else if (gramophoneLookTarget == null)
            Debug.LogWarning("[PhoneQuest] Gramophone Look Target not assigned.");

        if (holdAfterLooking > 0f)
            yield return new WaitForSeconds(holdAfterLooking);

        // Always play the monologue even if the Emily dialogue was skipped.
        yield return StartCoroutine(RunDialogue(afterCallMonologue));

        if (dialogueManager != null)
            dialogueManager.currentCameraSystem = previousCameraSystem;

        if (!bookQuestUnlocked)
        {
            bookQuestUnlocked = true;
            onBookQuestUnlocked?.Invoke();
        }

        state = PhoneState.Completed;
        ShowSkipHint(false);

        if (player != null)
        {
            player.isCinematic = false;
            player.SetCanMove(true);
        }
        Pause.canPause = true;

        // This is the ONLY place that completes the main phone quest.
        // QuestManagerV2 then activates Q2 (Talk to Lester).
        QuestManagerV2 manager = QuestManagerV2.Instance;
        if (manager != null && manager.IsQuestActive(requiredQuestID))
        {
            manager.ProcessAction(completionTargetID, GoalType.ReachPoint);
            Debug.Log("[PhoneQuest] Phone + gramophone monologue completed. Advancing to Lester.");
        }
        else
        {
            Debug.LogWarning("[PhoneQuest] Q_PHONE not active; unable to advance to Lester.");
        }
    }

    private IEnumerator LookAtGramophone()
    {
        if (player == null || gramophoneLookTarget == null) yield break;
        Camera currentCamera = player.playerCam != null ? player.playerCam : player.playerCamera;
        if (currentCamera == null)
        {
            Debug.LogWarning("[PhoneQuest] Player camera not assigned; skipping look-at.");
            yield break;
        }

        Vector3 direction = gramophoneLookTarget.position - currentCamera.transform.position;
        if (direction.sqrMagnitude < 0.0001f) yield break;

        Quaternion lookRotation = Quaternion.LookRotation(direction.normalized, Vector3.up);
        float targetYaw = lookRotation.eulerAngles.y;
        float targetPitch = Mathf.DeltaAngle(0f, lookRotation.eulerAngles.x);
        targetPitch = Mathf.Clamp(targetPitch, -75f, 75f);

        player.GetRotation(out float initialYaw, out float initialPitch);

        if (lookAtDuration <= 0f)
        {
            player.SetRotation(targetYaw, targetPitch);
            yield break;
        }

        float elapsed = 0f;
        while (elapsed < lookAtDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.SmoothStep(0f, 1f, Mathf.Clamp01(elapsed / lookAtDuration));
            player.SetRotation(
                Mathf.LerpAngle(initialYaw, targetYaw, t),
                Mathf.LerpAngle(initialPitch, targetPitch, t));
            yield return null;
        }
        player.SetRotation(targetYaw, targetPitch);
    }

    private IEnumerator RunDialogue(DialogueLine[] lines)
    {
        if (dialogueManager == null || lines == null || lines.Length == 0)
        {
            Debug.LogWarning("[PhoneQuest] DialogueManager or dialogue lines missing.");
            yield break;
        }

        dialogueManager.StartDialogue(lines);
        ShowSkipHint(true);

        while (dialogueManager.IsDialogueActive())
        {
            // E advances through PlayerController; Space skips THIS dialogue segment.
            if (Input.GetKeyDown(KeyCode.Space))
                dialogueManager.SkipDialogue();
            yield return null;
        }
        ShowSkipHint(false);
        yield return new WaitForSeconds(0.25f);
    }

    private void SetOutline(bool enabled)
    {
        if (outline == null) return;
        outline.isBlocked = !enabled;
        if (!enabled) outline.Hide();
    }

    private void ShowSkipHint(bool show)
    {
        if (skipHint == null) return;
        skipHint.gameObject.SetActive(show);
        if (show) skipHint.text = "[E] Next    [Space] Skip dialogue";
    }

    private void StartRingAudio()
    {
        StopRingAudio();
        if (ringEvent.IsNull)
        {
            Debug.LogWarning("[PhoneQuest] Ring FMOD event is not assigned.");
            return;
        }
        ringingInstance = RuntimeManager.CreateInstance(ringEvent);
        RuntimeManager.AttachInstanceToGameObject(ringingInstance, transform);
        ringingInstance.start();
    }

    private void StopRingAudio()
    {
        if (!ringingInstance.isValid()) return;
        ringingInstance.stop(FMOD.Studio.STOP_MODE.IMMEDIATE);
        ringingInstance.release();
        ringingInstance = default;
    }

    private void PlayOneShot(EventReference sound)
    {
        if (!sound.IsNull)
            RuntimeManager.PlayOneShot(sound, transform.position);
    }

    private void OnDestroy()
    {
        StopRingAudio();
    }
}
