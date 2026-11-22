using System.Collections;
using FMOD.Studio;
using FMODUnity;
using TMPro;
using UnityEngine;
using UnityEngine.Events;

public class OptionalPhoneQuestController : MonoBehaviour
{
    private enum PhoneState { Dormant, Ringing, InConversation, Completed, Missed }

    [Header("References")]
    [SerializeField] private PlayerController player;
    [SerializeField] private DialogueManager dialogueManager;
    [SerializeField] private PhoneAnimationController animationController;
    [SerializeField] private OutlineInteractable outline;

    [Header("Phone audio (FMOD)")]
    [SerializeField] private EventReference ringEvent;
    [SerializeField] private EventReference pickupEvent;
    [SerializeField] private EventReference hangupEvent;

    [Header("Optional UI (not the main quest UI)")]
    [SerializeField] private TMP_Text optionalHint;
    [SerializeField] private TMP_Text skipHint;
    [SerializeField, Min(0f)] private float optionalHintDuration = 4f;
    private Coroutine hintHideRoutine;

    [Header("Story")]
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

    [SerializeField] private DialogueLine[] afterCallMonologue =
    {
        new DialogueLine { speakerName = "Daniel", sentence = "Damn. The gramophone's gone." },
        new DialogueLine { speakerName = "Daniel", sentence = "So it really did happen yesterday." },
        new DialogueLine { speakerName = "Daniel", sentence = "I should talk to Lester about this. He lives in apartment 307." }
    };

    [Header("Book side quest")]
    [SerializeField] private UnityEvent onBookQuestUnlocked;

    private PhoneState state = PhoneState.Dormant;
    private EventInstance ringingInstance;
    private bool bookQuestUnlocked;
    private bool skipWholeCall;

    public bool BookQuestUnlocked => bookQuestUnlocked;

    private void Awake()
    {
        if (player == null) player = FindFirstObjectByType<PlayerController>();
        if (dialogueManager == null) dialogueManager = FindFirstObjectByType<DialogueManager>();
        if (animationController == null) animationController = GetComponent<PhoneAnimationController>();
        if (outline == null) outline = GetComponent<OutlineInteractable>();
        SetOutline(false);
        ShowOptionalHint(false);
        ShowSkipHint(false);
    }

    [ContextMenu("DEBUG: Start phone ringing")]
    public void BeginAfterWake()
    {
        if (state != PhoneState.Dormant) return;
        state = PhoneState.Ringing;
        if (animationController != null) animationController.StartRinging();
        SetOutline(true);
        ShowOptionalHint(true);
        StartRingAudio();
        Debug.Log("[OptionalPhone] Phone available. Main quest continues independently.");
    }

    public bool TryInteract()
    {
        if (state != PhoneState.Ringing) return false;
        state = PhoneState.InConversation;
        skipWholeCall = false;
        StartCoroutine(CallRoutine());
        return true;
    }

    public void CancelIfUnanswered()
    {
        if (state != PhoneState.Ringing) return;
        state = PhoneState.Missed;
        StopRingAudio();
        if (animationController != null) animationController.StopRinging();
        SetOutline(false);
        ShowOptionalHint(false);
        Debug.Log("[OptionalPhone] Missed call. Lester remains available.");
    }

    private IEnumerator CallRoutine()
    {
        StopRingAudio();
        if (animationController != null) animationController.StopRinging();
        SetOutline(false);
        ShowOptionalHint(false);

        if (player != null)
        {
            player.SetCanMove(false);
            player.isCinematic = true;
        }
        Pause.canPause = false;

        PlayOneShot(pickupEvent);
        if (animationController != null)
            yield return StartCoroutine(animationController.PickUp());

        // The phone must not trigger Lester's cinematic camera system.
        DialogueCameraSystem previousCameraSystem =
            dialogueManager != null ? dialogueManager.currentCameraSystem : null;
        if (dialogueManager != null)
            dialogueManager.currentCameraSystem = null;

        yield return StartCoroutine(RunDialogue(conversation));

        PlayOneShot(hangupEvent);
        if (animationController != null)
            yield return StartCoroutine(animationController.PutBack());

        if (!skipWholeCall)
            yield return StartCoroutine(RunDialogue(afterCallMonologue));

        if (dialogueManager != null)
            dialogueManager.currentCameraSystem = previousCameraSystem;

        state = PhoneState.Completed;
        if (!bookQuestUnlocked)
        {
            bookQuestUnlocked = true;
            onBookQuestUnlocked?.Invoke();
            Debug.Log("[OptionalPhone] Side quest unlocked: Retrieve Tommy's book.");
        }

        ShowSkipHint(false);
        if (player != null)
        {
            player.isCinematic = false;
            player.SetCanMove(true);
        }
        Pause.canPause = true;
    }

    private IEnumerator RunDialogue(DialogueLine[] lines)
    {
        if (dialogueManager == null || lines == null || lines.Length == 0)
        {
            Debug.LogWarning("[OptionalPhone] DialogueManager or dialogue lines missing.");
            yield break;
        }

        dialogueManager.StartDialogue(lines);
        ShowSkipHint(true);

        while (dialogueManager.IsDialogueActive())
        {
            // E/Controller advances lines through the existing PlayerController.
            // Space skips the rest of the optional call via DialogueManager.SkipDialogue().
            if (Input.GetKeyDown(KeyCode.Space))
            {
                skipWholeCall = true;
                dialogueManager.SkipDialogue();
            }
            yield return null;
        }
        ShowSkipHint(false);
        // Allow the existing fade to begin before the next segment.
        yield return new WaitForSeconds(0.25f);
    }

    private void SetOutline(bool enabled)
    {
        if (outline == null) return;
        outline.isBlocked = !enabled;
        if (!enabled) outline.Hide();
    }

    private void ShowOptionalHint(bool show)
    {
        if (hintHideRoutine != null)
        {
            StopCoroutine(hintHideRoutine);
            hintHideRoutine = null;
        }
        if (optionalHint == null) return;
        optionalHint.gameObject.SetActive(show);
        if (show)
        {
            optionalHint.text = "Optional: Answer the phone";
            if (optionalHintDuration > 0f)
                hintHideRoutine = StartCoroutine(HideOptionalHintAfterDelay());
        }
    }

    private IEnumerator HideOptionalHintAfterDelay()
    {
        yield return new WaitForSeconds(optionalHintDuration);
        if (optionalHint != null) optionalHint.gameObject.SetActive(false);
        hintHideRoutine = null;
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
            Debug.LogWarning("[OptionalPhone] Ring FMOD event is not assigned.");
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

    private void PlayOneShot(EventReference eventReference)
    {
        if (!eventReference.IsNull)
            RuntimeManager.PlayOneShot(eventReference, transform.position);
    }

    private void OnDestroy()
    {
        StopRingAudio();
    }
}
