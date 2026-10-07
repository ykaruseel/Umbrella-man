using FMODUnity;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class PlayerComments : MonoBehaviour
{
    public GameObject commentsGO;
    public TMP_Text dialogueText;

    public DialogueLine[] dialogueLines;

    [Header("Settings")]
    public float typingSpeed = 0.03f;
    public float fadeDuration = 0.2f;

    [SerializeField]
    private bool autoCloseAfterLastLine = false;

    [SerializeField]
    private float autoCloseDelay = 0.5f;

    [Header("FMOD Voices")]
    [SerializeField] private EventReference danielVoiceEvent;
    [SerializeField] private Transform playerTransform;

    private Queue<DialogueLine> linesQueue = new Queue<DialogueLine>();
    private bool isDialogueActive = false;


    private bool isTyping = false; 
    private string currentSentence = "";

    private Coroutine typingCoroutine;
    private Coroutine fadeCoroutine;

    private FMOD.Studio.EventInstance currentVoiceInstance;
    private bool hasActiveVoice = false;
    public CanvasGroup dialogueCanvasGroup;
    private Coroutine autoCloseCoroutine;

    void Start()
    {
        if (commentsGO != null)
        {
            dialogueCanvasGroup =
                commentsGO.GetComponent<CanvasGroup>();

            if (dialogueCanvasGroup == null)
            {
                dialogueCanvasGroup =
                    commentsGO.AddComponent<CanvasGroup>();
            }

            dialogueCanvasGroup.alpha = 0f;
            commentsGO.SetActive(false);
        }
    }
    public void StartDialogue()
    {
        if (dialogueLines == null || dialogueLines.Length == 0) return;

        PlayerController.playerComments = this;

        linesQueue.Clear();
        foreach (var line in dialogueLines) linesQueue.Enqueue(line);

        isDialogueActive = true;

        if (commentsGO != null)
        {
            commentsGO.SetActive(true);
            if (fadeCoroutine != null) StopCoroutine(fadeCoroutine);
            fadeCoroutine = StartCoroutine(FadeCanvasGroup(dialogueCanvasGroup, 0f, 1f));
        }

        DisplayNextSentence();
    }


    public void DisplayNextSentence()
    {
        if (isTyping)
        {
            if (typingCoroutine != null)
                StopCoroutine(typingCoroutine);

            if (dialogueText != null)
            {
                dialogueText.text = currentSentence;
                dialogueText.maxVisibleCharacters = int.MaxValue;
            }

            isTyping = false;
            StopCurrentVoice();
            typingCoroutine = null;

            if (autoCloseAfterLastLine &&
                linesQueue.Count == 0)
            {
                if (autoCloseCoroutine != null)
                    StopCoroutine(autoCloseCoroutine);

                autoCloseCoroutine =
                    StartCoroutine(AutoCloseLastLine());
            }

            return;
        }

        StopCurrentVoice();

        if (linesQueue.Count == 0)
        {
            EndDialogue();
            return;
        }

        DialogueLine line = linesQueue.Dequeue();

        currentSentence = line.sentence;

        typingCoroutine = StartCoroutine(TypeSentence(line));
    }

    private IEnumerator TypeSentence(DialogueLine line)
    {
        isTyping = true;

        if (dialogueText != null)
        {
            dialogueText.richText = true;

            dialogueText.text = line.sentence;
            dialogueText.maxVisibleCharacters = 0;

            dialogueText.ForceMeshUpdate();
        }

        StartVoiceForSpeaker(line.speakerName);

        int visibleCharacterCount =
            dialogueText != null
                ? dialogueText.textInfo.characterCount
                : line.sentence.Length;

        for (int i = 1; i <= visibleCharacterCount; i++)
        {
            while (Pause.isPaused)
            {
                SetPaused();
                yield return null;
            }

            SetPaused();

            if (dialogueText != null)
            {
                dialogueText.maxVisibleCharacters = i;
            }

            yield return new WaitForSeconds(typingSpeed);
        }

        if (dialogueText != null)
        {
            dialogueText.maxVisibleCharacters = int.MaxValue;
        }

        isTyping = false;
        StopCurrentVoice();
        typingCoroutine = null;

        if (autoCloseAfterLastLine &&
            linesQueue.Count == 0)
        {
            if (autoCloseCoroutine != null)
                StopCoroutine(autoCloseCoroutine);

            autoCloseCoroutine =
                StartCoroutine(AutoCloseLastLine());
        }
    }

    private void EndDialogue()
    {
        isDialogueActive = false;
        StopCurrentVoice();

        if (commentsGO != null && dialogueCanvasGroup != null)
        {
            if (fadeCoroutine != null) StopCoroutine(fadeCoroutine);
            fadeCoroutine = StartCoroutine(FadeCanvasGroup(dialogueCanvasGroup, 1f, 0f, true));
        }
        PlayerController.playerComments = null;
    }

    private IEnumerator FadeCanvasGroup(CanvasGroup cg, float start, float end, bool disableAfter = false)
    {
        float timer = 0f;
        cg.alpha = start;

        while (timer < fadeDuration)
        {
            timer += Time.deltaTime;
            cg.alpha = Mathf.Lerp(start, end, timer / fadeDuration);
            yield return null;
        }

        cg.alpha = end;

        if (disableAfter && commentsGO != null)
        {
            commentsGO.SetActive(false);
        }
    }

    public bool IsDialogueActive()
    {
        return isDialogueActive;
    }

    private void StartVoiceForSpeaker(string speakerName)
    {
        StopCurrentVoice();

        EventReference voiceEvent = danielVoiceEvent;

        if (voiceEvent.IsNull || playerTransform == null) return;

        currentVoiceInstance = RuntimeManager.CreateInstance(voiceEvent);

        RuntimeManager.AttachInstanceToGameObject(
            currentVoiceInstance,
            playerTransform,
            playerTransform.GetComponent<Rigidbody>()
        );

        currentVoiceInstance.start();
        hasActiveVoice = true;
    }

    private void StopCurrentVoice()
    {
        if (!hasActiveVoice) return;
        if (currentVoiceInstance.isValid())
        {
            currentVoiceInstance.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
            currentVoiceInstance.release();
        }
        hasActiveVoice = false;
    }

    public void SetPaused()
    {
        if (hasActiveVoice && currentVoiceInstance.isValid())
            currentVoiceInstance.setPaused(Pause.isPaused);
    }

    void OnDestroy()
    {
        if (hasActiveVoice && currentVoiceInstance.isValid())
        {
            currentVoiceInstance.stop(FMOD.Studio.STOP_MODE.IMMEDIATE);
            currentVoiceInstance.release();
        }
    }

    private IEnumerator AutoCloseLastLine()
    {
        yield return new WaitForSeconds(autoCloseDelay);

        if (isDialogueActive &&
            !isTyping &&
            linesQueue.Count == 0)
        {
            EndDialogue();
        }

        autoCloseCoroutine = null;
    }
}
