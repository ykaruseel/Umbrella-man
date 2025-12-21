using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using FMODUnity;
using TMPro;

public class DialogueManager : MonoBehaviour
{
    public static DialogueManager instance;

    [Header("UI")]
    public GameObject dialoguePanel;    // панель с диалогом
    public TMP_Text nameText;
    public TMP_Text dialogueText;       

    [Header("Settings")]
    public float typingSpeed = 0.03f;   // скорость печати
    public float fadeDuration = 0.5f;   // скорость появления окна

    [Header("FMOD Voices")]
    [SerializeField] private EventReference danielVoiceEvent;
    [SerializeField] private EventReference lesterVoiceEvent;

    private Queue<DialogueLine> linesQueue = new Queue<DialogueLine>();
    private bool isDialogueActive = false;
    
    private Coroutine typingCoroutine;
    private Coroutine fadeCoroutine; // Корутина для плавного появления/исчезновения

    private FMOD.Studio.EventInstance currentVoiceInstance;
    private bool hasActiveVoice = false;
    public CanvasGroup dialogueCanvasGroup; // Ссылка на CanvasGroup

    void Awake()
    {
        if (instance == null)
            instance = this;
        else
            Destroy(gameObject);
    }

    void Start()
    {
        // Пытаемся найти CanvasGroup, если его нет — добавим сами
        if (dialoguePanel != null)
        {
            dialogueCanvasGroup = dialoguePanel.GetComponent<CanvasGroup>();
            if (dialogueCanvasGroup == null)
            {
                dialogueCanvasGroup = dialoguePanel.AddComponent<CanvasGroup>();
            }
            
            dialoguePanel.SetActive(false);
            dialogueCanvasGroup.alpha = 0f; // Скрываем сразу
        }
    }

    // Вызывается из NPC_Dialogue
    public void StartDialogue(DialogueLine[] lines)
    {
        if (lines == null || lines.Length == 0) return;

        linesQueue.Clear();
        foreach (var line in lines) linesQueue.Enqueue(line);

        isDialogueActive = true;

        // Запускаем плавное появление окна
        if (dialoguePanel != null)
        {
            dialoguePanel.SetActive(true);
            
            // Если была корутина исчезновения — останавливаем
            if (fadeCoroutine != null) StopCoroutine(fadeCoroutine);
            fadeCoroutine = StartCoroutine(FadeCanvasGroup(dialogueCanvasGroup, 0f, 1f));
        }

        DisplayNextSentence();
    }

    public void DisplayNextSentence()
    {
        if (typingCoroutine != null)
        {
            StopCoroutine(typingCoroutine);
            typingCoroutine = null;
        }

        StopCurrentVoice(); 

        if (linesQueue.Count == 0)
        {
            EndDialogue();
            return;
        }

        DialogueLine line = linesQueue.Dequeue();

        if (nameText != null) nameText.text = line.speakerName;

        typingCoroutine = StartCoroutine(TypeSentence(line));
    }

    private IEnumerator TypeSentence(DialogueLine line)
    {
        if (dialogueText != null) dialogueText.text = "";

        StartVoiceForSpeaker(line.speakerName);

        foreach (char letter in line.sentence.ToCharArray())
        {
            if (dialogueText != null) dialogueText.text += letter;
            yield return new WaitForSeconds(typingSpeed);
        }

        StopCurrentVoice();
        typingCoroutine = null;
    }

    private void EndDialogue()
    {
        isDialogueActive = false;
        StopCurrentVoice();

        // Запускаем плавное исчезновение
        if (dialoguePanel != null && dialogueCanvasGroup != null)
        {
            if (fadeCoroutine != null) StopCoroutine(fadeCoroutine);
            fadeCoroutine = StartCoroutine(FadeCanvasGroup(dialogueCanvasGroup, 1f, 0f, true));
        }
    }

    // Универсальная корутина для фейда
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

        if (disableAfter && dialoguePanel != null)
        {
            dialoguePanel.SetActive(false);
        }
    }

    public bool IsDialogueActive()
    {
        return isDialogueActive;
    }

    // ---------- FMOD ГОЛОСА ----------
    private void StartVoiceForSpeaker(string speakerName)
    {
        StopCurrentVoice();
        EventReference voiceEvent = new EventReference();

        if (speakerName == "Daniel") voiceEvent = danielVoiceEvent;
        else if (speakerName == "Lester") voiceEvent = lesterVoiceEvent;
        else return;

        if (voiceEvent.IsNull) return;

        currentVoiceInstance = RuntimeManager.CreateInstance(voiceEvent);
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

    void OnDestroy()
    {
        if (hasActiveVoice && currentVoiceInstance.isValid())
        {
            currentVoiceInstance.stop(FMOD.Studio.STOP_MODE.IMMEDIATE);
            currentVoiceInstance.release();
        }
    }
}
