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
    public float fadeDuration = 0.2f;   // чуть ускорил появление (было 0.5)

    [Header("FMOD Voices")]
    [SerializeField] private EventReference danielVoiceEvent;
    [SerializeField] private EventReference lesterVoiceEvent;

    private Queue<DialogueLine> linesQueue = new Queue<DialogueLine>();
    private bool isDialogueActive = false;
    
    // --- НОВЫЕ ПЕРЕМЕННЫЕ ДЛЯ ПРОПУСКА ---
    private bool isTyping = false;       // Печатается ли текст сейчас?
    private string currentSentence = ""; // Храним полную фразу здесь
    // -------------------------------------

    private Coroutine typingCoroutine;
    private Coroutine fadeCoroutine; 

    private FMOD.Studio.EventInstance currentVoiceInstance;
    private bool hasActiveVoice = false;
    public CanvasGroup dialogueCanvasGroup; 

    void Awake()
    {
        if (instance == null) instance = this;
        else Destroy(gameObject);
    }

    void Start()
    {
        if (dialoguePanel != null)
        {
            dialogueCanvasGroup = dialoguePanel.GetComponent<CanvasGroup>();
            if (dialogueCanvasGroup == null)
            {
                dialogueCanvasGroup = dialoguePanel.AddComponent<CanvasGroup>();
            }
            
            dialoguePanel.SetActive(false);
            dialogueCanvasGroup.alpha = 0f; 
        }
    }

    public void StartDialogue(DialogueLine[] lines)
    {
        if (lines == null || lines.Length == 0) return;

        linesQueue.Clear();
        foreach (var line in lines) linesQueue.Enqueue(line);

        isDialogueActive = true;

        if (dialoguePanel != null)
        {
            dialoguePanel.SetActive(true);
            if (fadeCoroutine != null) StopCoroutine(fadeCoroutine);
            fadeCoroutine = StartCoroutine(FadeCanvasGroup(dialogueCanvasGroup, 0f, 1f));
        }

        DisplayNextSentence();
    }

    // --- ОБНОВЛЕННАЯ ЛОГИКА ОТОБРАЖЕНИЯ ---
    public void DisplayNextSentence()
    {
        // 1. Если текст ЕЩЁ печатается — завершаем его мгновенно
        if (isTyping)
        {
            if (typingCoroutine != null) StopCoroutine(typingCoroutine);
            
            if (dialogueText != null) 
                dialogueText.text = currentSentence; // Показываем всю фразу сразу
            
            isTyping = false;
            return; // ВАЖНО: Выходим, не запуская следующую фразу
        }

        // 2. Если текст УЖЕ написан полностью — переходим к следующему
        StopCurrentVoice(); 

        if (linesQueue.Count == 0)
        {
            EndDialogue();
            return;
        }

        DialogueLine line = linesQueue.Dequeue();
        
        // Сохраняем полную фразу для пропуска
        currentSentence = line.sentence;

        if (nameText != null) nameText.text = line.speakerName;

        typingCoroutine = StartCoroutine(TypeSentence(line));
    }

    private IEnumerator TypeSentence(DialogueLine line)
    {
        isTyping = true; // Начали печать
        
        if (dialogueText != null) dialogueText.text = "";

        StartVoiceForSpeaker(line.speakerName);

        foreach (char letter in line.sentence.ToCharArray())
        {
            // Учитываем паузу (твоя старая логика)
            while (Pause.isPaused)
            {
                SetPaused(); // ставим звук на паузу
                yield return null;
            }
            SetPaused(); // снимаем звук с паузы

            if (dialogueText != null) dialogueText.text += letter;
            yield return new WaitForSeconds(typingSpeed);
        }

        isTyping = false; // Закончили печать
        StopCurrentVoice();
        typingCoroutine = null;
    }

    private void EndDialogue()
    {
        isDialogueActive = false;
        StopCurrentVoice();

        if (dialoguePanel != null && dialogueCanvasGroup != null)
        {
            if (fadeCoroutine != null) StopCoroutine(fadeCoroutine);
            fadeCoroutine = StartCoroutine(FadeCanvasGroup(dialogueCanvasGroup, 1f, 0f, true));
        }
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
}
