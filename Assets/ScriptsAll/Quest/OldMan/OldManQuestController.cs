using System.Collections;
using FMODUnity;
using TMPro;
using Unity.Cinemachine;
using UnityEngine;

public sealed class OldManQuestController : MonoBehaviour
{
    [Header("Quest (must match QuestData)")]
    [SerializeField] private string requiredQuestID = "Q_OLD_MAN";
    [SerializeField] private string completionTargetID = "OLD_MAN_COMPLETE";

    [Header("Required scene references")]
    [SerializeField] private PlayerController player;
    [SerializeField] private DialogueManager dialogueManager;
    [SerializeField] private TMP_Text shortMessage;
    [SerializeField] private Transform voiceOrigin;
    [SerializeField] private OutlineInteractable outline;

    [Header("Optional locked shot (Cinemachine 3)")]
    [SerializeField] private CinemachineCamera closedDoorCamera;
    [SerializeField] private CinemachineCamera fireSpreadCamera;
    [SerializeField] private int cinematicCameraPriority = 200;

    [Header("Knocking and comments")]
    [SerializeField, Min(0f)] private float noAnswerDelay = 0.7f;
    [SerializeField, Min(0.1f)] private float silenceMessageTime = 1.0f;
    [SerializeField, Min(0.1f)] private float danielCommentTime = 2.5f;
    [SerializeField, Min(0f)] private float delayAfterThirdKnock = 10f;

    [Header("Fire stages - root stays off until dialogue ends")]
    [SerializeField] private GameObject fireRoot;
    [SerializeField] private Light behindDoorLight;
    [SerializeField] private GameObject behindDoorFire;
    [SerializeField] private GameObject doorFire;
    [SerializeField] private GameObject[] corridorFire = new GameObject[0];
    [Tooltip("e.g. UmbrellaManFire: effects belonging to the NEXT quest, not this ignition.")]
    [SerializeField] private GameObject[] deferredEffects = new GameObject[0];
    [SerializeField, Min(0f)] private float waitToDoorFire = 1.5f;
    [SerializeField, Min(0f)] private float waitToCorridorFire = 1.0f;
    [SerializeField, Min(0f)] private float corridorSpreadInterval = 0.8f;
    [SerializeField, Min(0f)] private float finalFireHold = 1.3f;

    [Header("FMOD (assign actual events in the FMOD event browser)")]
    [SerializeField] private EventReference knockEvent;
    [SerializeField] private EventReference keroseneLineEvent;
    [SerializeField] private EventReference firstKnockCommentEvent;
    [SerializeField] private EventReference secondKnockCommentEvent;
    [SerializeField] private EventReference crashEvent;
    [Tooltip("Optional scene FMOD StudioEventEmitter. Put it on an active object; it can continue into Escape.")]
    [SerializeField] private StudioEventEmitter fireLoopEmitter;

    [Header("Story: each short line is advanced with E")]
    [SerializeField] private DialogueLine[] conversation =
    {
        L("Old Man", "Coming, coming! Who's there?"),
        L("Daniel", "Hello. My name's Daniel. I live in this building."),
        L("Daniel", "I... wanted to ask you something. Would you mind opening the door?"),
        L("Old Man", "I'm not opening this door. I look too damn awful."),
        L("Old Man", "Besides, you don't look much better yourself."),
        L("Daniel", "Alright. I'll ask from here, then."),
        L("Daniel", "Have you ever heard of the man with the umbrella?"),
        L("Old Man", "HA-HA-HA. The man with the umbrella. So... you've seen him?"),
        L("Daniel", "Twice."),
        L("Old Man", "I've been seeing him more and more lately."),
        L("Old Man", "You're afraid of him, aren't you? And of what comes after him."),
        L("Daniel", "What comes after him? What do you know about him?"),
        L("Old Man", "No one knows what comes after him."),
        L("Old Man", "Though I suppose I'll find out soon enough. HA-HA-HA."),
        L("Old Man", "Tell me, Daniel... Why are you afraid of death?"),
        L("Daniel", "What makes you think I'm afraid of death?"),
        L("Old Man", "Because otherwise, you wouldn't pay any mind to a man carrying an umbrella."),
        L("Old Man", "Do you understand? So tell me… Why?"),
        L("Old Man", "You've lost someone, haven't you? Someone close to you."),
        L("Daniel", "What's that got to do with any of this?"),
        L("Daniel", "Of course I'm afraid of death. Everyone is."),
        L("Daniel", "Everyone's afraid of what comes after. Can you just tell me who he is?"),
        L("Old Man", "No. You're not just afraid of what comes after."),
        L("Old Man", "You're afraid there's nothing after. And that's why he'll never leave you alone."),
        L("Daniel", "But how are those things connected?"),
        L("Old Man", "I've lost a lot of people and I've lived a long life. Too long."),
        L("Old Man", "It's a sad thing to realize I spent the last years of my life"),
        L("Old Man", "rotting away in this miserable basement."),
        L("Old Man", "Now I'm ready to meet him. I think I can see him right now."),
        L("Old Man", "Time to set it all ablaze, Daniel!")
    };

    private int knockCount;
    private bool knockBusy;
    private bool finalSequenceStarted;
    private bool finished;
    private bool smellWasShown;
    private int cueVersion;
    private bool pausePreviouslyAllowed;
    private int previousDoorCameraPriority;
    private int previousFireCameraPriority;
    private DialogueCameraSystem previousDialogueCameraSystem;

    // Constructor-style helper is executed only when a component is first initialized;
    // Unity then serializes the editable array normally.
    private static DialogueLine L(string speaker, string words)
    {
        return new DialogueLine { speakerName = speaker, sentence = words };
    }

    private void Awake()
    {
        if (player == null) player = FindFirstObjectByType<PlayerController>();
        if (dialogueManager == null) dialogueManager = FindFirstObjectByType<DialogueManager>();
        if (outline == null) outline = GetComponent<OutlineInteractable>();

        ResetFireVisuals();
        SetShortMessage(string.Empty);
    }

    private bool IsCurrentQuest()
    {
        return QuestManagerV2.Instance != null &&
               QuestManagerV2.Instance.IsQuestActive(requiredQuestID);
    }

    private void Update()
    {
        if (outline == null) return;
        bool available = IsCurrentQuest() && !finalSequenceStarted && !finished;
        outline.isBlocked = !available;
        if (!available) outline.Hide();
    }

    /// <summary>Call from PlayerController's existing E-raycast handler.</summary>
    public bool TryInteract()
    {
        if (!IsCurrentQuest()) return false;
        // Consume interaction while another knock is in progress; never open the door.
        if (finished || knockBusy || finalSequenceStarted) return true;
        if (player == null || dialogueManager == null || shortMessage == null)
        {
            Debug.LogError("[OldMan] Assign Player, DialogueManager and Short Message.", this);
            return true;
        }

        if (knockCount == 2 && !FireReferencesReady())
        {
            Debug.LogError("[OldMan] Fire references are incomplete. Connect all fire stages BEFORE the third knock.", this);
            return true;
        }

        knockCount++;
        PlayOneShot(knockEvent);
        Debug.Log($"[OldMan] Knock {knockCount}/3.", this);

        if (knockCount < 3)
        {
            StartCoroutine(NoAnswerRoutine(knockCount));
        }
        else
        {
            finalSequenceStarted = true;
            StartCoroutine(FinalSequence());
        }
        return true;
    }

    private IEnumerator NoAnswerRoutine(int number)
    {
        // Player movement and camera stay free until knock #3 (QDD).
        knockBusy = true;
        yield return new WaitForSeconds(noAnswerDelay);
        yield return StartCoroutine(ShowCueFor("...", silenceMessageTime));
        if (number == 1)
        {
            PlayDanielOneShot(firstKnockCommentEvent);
            yield return StartCoroutine(ShowCueFor("Daniel: Maybe try again?", danielCommentTime));
        }
        else
        {
            PlayDanielOneShot(secondKnockCommentEvent);
            yield return StartCoroutine(ShowCueFor("Daniel: I'm not leaving just like that.", danielCommentTime));
        }
        knockBusy = false;
    }

    public void NotifyEnteredBasement()
    {
        if (!IsCurrentQuest() || smellWasShown || finalSequenceStarted) return;
        smellWasShown = true;
        PlayDanielOneShot(keroseneLineEvent);
        StartCoroutine(ShowCueFor("Daniel: What's that smell? Kerosene?", 3.2f));
    }

    private IEnumerator FinalSequence()
    {
        knockBusy = true;
        ClearCue();
        pausePreviouslyAllowed = Pause.canPause;
        Pause.canPause = false;
        player.SetCanMove(false);
        player.isCinematic = true;
        if (outline != null) { outline.isBlocked = true; outline.Hide(); }

        if (closedDoorCamera != null)
        {
            previousDoorCameraPriority = closedDoorCamera.Priority;
            closedDoorCamera.Priority = cinematicCameraPriority;
        }
        if (fireSpreadCamera != null)
            previousFireCameraPriority = fireSpreadCamera.Priority;

        // No voice from behind the door until ten seconds after knock #3.
        Debug.Log($"[OldMan] Third knock. Waiting {delayAfterThirdKnock}s before the voice.", this);
        yield return new WaitForSeconds(delayAfterThirdKnock);

        if (voiceOrigin == null)
            Debug.LogWarning("[OldMan] Assign OldManVoiceOrigin to DialogueManager for spatial FMOD dialogue.", this);

        previousDialogueCameraSystem = dialogueManager.currentCameraSystem;
        dialogueManager.currentCameraSystem = null; // Our locked-door camera stays fixed.
        dialogueManager.StartDialogue(conversation);
        while (dialogueManager.IsDialogueActive()) yield return null;
        // This also handles DialogueManager.SkipDialogue() correctly.
        dialogueManager.currentCameraSystem = previousDialogueCameraSystem;

        yield return new WaitForSeconds(0.35f);
        Debug.Log("[OldMan] Dialogue finished. Fire sequence begins.", this);
        PlayOneShot(crashEvent);
        yield return StartCoroutine(PlayFireSequence());

        // Restore controls ONLY after the complete fire sequence.
        if (closedDoorCamera != null)
            closedDoorCamera.Priority = previousDoorCameraPriority;
        if (fireSpreadCamera != null)
            fireSpreadCamera.Priority = previousFireCameraPriority;
        player.isCinematic = false;
        player.SetCanMove(true);
        Pause.canPause = pausePreviouslyAllowed;
        knockBusy = false;
        finished = true;

        if (QuestManagerV2.Instance != null &&
            QuestManagerV2.Instance.IsGoalRequired(completionTargetID, GoalType.ReachPoint))
        {
            QuestManagerV2.Instance.ProcessAction(completionTargetID, GoalType.ReachPoint);
            Debug.Log("[OldMan] OLD_MAN_COMPLETE -> next quest Escape!", this);
        }
        else
        {
            Debug.LogError("[OldMan] Quest target OLD_MAN_COMPLETE was not required. Check Q_OLD_MAN QuestData.", this);
        }
    }

    private bool FireReferencesReady()
    {
        if (fireRoot == null || behindDoorFire == null || doorFire == null ||
            corridorFire == null || corridorFire.Length == 0)
            return false;

        bool hasCorridorEffect = false;
        foreach (GameObject go in corridorFire)
            if (go != null) hasCorridorEffect = true;
        return hasCorridorEffect;
    }

    private void ResetFireVisuals()
    {
        if (behindDoorLight != null) behindDoorLight.enabled = false;
        SetFx(behindDoorFire, false);
        SetFx(doorFire, false);
        if (corridorFire != null)
            foreach (GameObject fx in corridorFire) SetFx(fx, false);
        if (deferredEffects != null)
            foreach (GameObject fx in deferredEffects) SetFx(fx, false);
        if (fireRoot != null) fireRoot.SetActive(false);
    }

    private IEnumerator PlayFireSequence()
    {
        if (!FireReferencesReady())
        {
            Debug.LogError("[OldMan] Cannot play fire: missing references.", this);
            yield break;
        }

        fireRoot.SetActive(true);
        if (behindDoorLight != null) behindDoorLight.enabled = true;
        SetFx(behindDoorFire, true);
        if (fireLoopEmitter != null) fireLoopEmitter.Play();
        Debug.Log("[OldMan] Fire stage 1: light and flames BEHIND the door.", this);
        yield return new WaitForSeconds(waitToDoorFire);

        SetFx(doorFire, true);
        Debug.Log("[OldMan] Fire stage 2: door starts burning.", this);
        yield return new WaitForSeconds(waitToCorridorFire);

        // Optional second shot: show the fire travelling down the corridor.
        if (finalSequenceStarted && fireSpreadCamera != null)
            fireSpreadCamera.Priority = cinematicCameraPriority + 1;

        foreach (GameObject fx in corridorFire)
        {
            if (fx == null) continue;
            SetFx(fx, true);
            yield return new WaitForSeconds(corridorSpreadInterval);
        }

        Debug.Log("[OldMan] Fire stage 3 finished: corridor burning.", this);
        yield return new WaitForSeconds(finalFireHold);
        // Keep fire ON for Escape!; do not turn off FX or stop fire FMOD here.
    }

    private static void SetFx(GameObject root, bool active)
    {
        if (root == null) return;
        root.SetActive(active);
        if (active)
        {
            ParticleSystem[] particles = root.GetComponentsInChildren<ParticleSystem>(true);
            foreach (ParticleSystem ps in particles)
            {
                if (!ps.gameObject.activeInHierarchy) continue;
                ps.Clear(true);
                ps.Play(true);
            }
        }
        else
        {
            foreach (ParticleSystem ps in root.GetComponentsInChildren<ParticleSystem>(true))
                ps.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
        }
    }

    private IEnumerator ShowCueFor(string text, float seconds)
    {
        int version = ++cueVersion;
        SetShortMessage(text);
        yield return new WaitForSeconds(seconds);
        if (version == cueVersion) SetShortMessage(string.Empty);
    }

    private void ClearCue()
    {
        cueVersion++;
        SetShortMessage(string.Empty);
    }

    private void SetShortMessage(string message)
    {
        if (shortMessage == null) return;
        shortMessage.text = message;
        shortMessage.gameObject.SetActive(!string.IsNullOrEmpty(message));
    }

    private void PlayOneShot(EventReference reference)
    {
        if (!reference.IsNull)
            RuntimeManager.PlayOneShotAttached(reference, gameObject);
    }

    private void PlayDanielOneShot(EventReference reference)
    {
        if (!reference.IsNull && player != null)
            RuntimeManager.PlayOneShotAttached(reference, player.gameObject);
    }

#if UNITY_EDITOR
    [ContextMenu("DEBUG: Activate Q_OLD_MAN (Play Mode only)")]
    private void DebugActivateQuest()
    {
        if (!Application.isPlaying) return;
        if (QuestManagerV2.Instance != null)
            QuestManagerV2.Instance.ActivateQuestByID(requiredQuestID, true);
    }

    [ContextMenu("DEBUG: Preview all fire stages (Play Mode only)")]
    private void DebugFire()
    {
        if (!Application.isPlaying) return;
        if (!FireReferencesReady())
        {
            Debug.LogError("[OldMan] Assign Fire Root + Behind Door + Door + Corridor FX before preview.", this);
            return;
        }
        StartCoroutine(PlayFireSequence());
    }

    [ContextMenu("DEBUG: Reset fire visuals (Play Mode only)")]
    private void DebugResetFire()
    {
        if (!Application.isPlaying) return;
        ResetFireVisuals();
    }
#endif
}
