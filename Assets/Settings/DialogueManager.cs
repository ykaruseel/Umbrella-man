// DialogueManager.cs
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
    public GameObject dialoguePanel;    // панель с диалогом (можно выключать/включать)
    public TMP_Text nameText;
    public TMP_Text dialogueText;           // текст реплики

    [Header("Typing")]
    public float typingSpeed = 0.03f;   // скорость "печати" символов

    [Header("FMOD Voices")]
    [SerializeField] private EventReference danielVoiceEvent;
    [SerializeField] private EventReference lesterVoiceEvent;

    private Queue<DialogueLine> linesQueue = new Queue<DialogueLine>();
    private bool isDialogueActive = false;
    private Coroutine typingCoroutine;

    private FMOD.Studio.EventInstance currentVoiceInstance;
    private bool hasActiveVoice = false;

    void Awake()
    {
        if (instance == null)
            instance = this;
        else
            Destroy(gameObject);
    }

    void Start()
    {
        if (dialoguePanel != null)
            dialoguePanel.SetActive(false);
    }

    // Вызывается из NPC_Dialogue
    public void StartDialogue(DialogueLine[] lines)
    {
        if (lines == null || lines.Length == 0)
        {
            Debug.LogWarning("DialogueManager: Пустой массив реплик.");
            return;
        }

        linesQueue.Clear();

        foreach (var line in lines)
            linesQueue.Enqueue(line);

        isDialogueActive = true;

        if (dialoguePanel != null)
            dialoguePanel.SetActive(true);

        DisplayNextSentence();
    }

    // Вызывается при нажатии E из PlayerController
    public void DisplayNextSentence()
    {
        // если сейчас что-то печатается — остановим корутину и плавно заглушим звук
        if (typingCoroutine != null)
        {
            StopCoroutine(typingCoroutine);
            typingCoroutine = null;
        }

        StopCurrentVoice(); // мягко глушим предыдущий голос

        if (linesQueue.Count == 0)
        {
            EndDialogue();
            return;
        }

        DialogueLine line = linesQueue.Dequeue();

        if (nameText != null)
            nameText.text = line.speakerName;

        // запускаем печать новой реплики с голосом
        typingCoroutine = StartCoroutine(TypeSentence(line));
    }

    private IEnumerator TypeSentence(DialogueLine line)
    {
        if (dialogueText != null)
            dialogueText.text = "";

        // запускаем голос под текущего спикера
        StartVoiceForSpeaker(line.speakerName);

        // по одному символу
        foreach (char letter in line.sentence.ToCharArray())
        {
            if (dialogueText != null)
                dialogueText.text += letter;

            yield return new WaitForSeconds(typingSpeed);
        }

        // когда печать закончилась — плавно заглушаем голос
        StopCurrentVoice();

        typingCoroutine = null;
    }

    private void EndDialogue()
    {
        isDialogueActive = false;

        StopCurrentVoice(); // на всякий случай

        if (dialoguePanel != null)
            dialoguePanel.SetActive(false);

        if (dialogueText != null)
            dialogueText.text = "";

        if (nameText != null)
            nameText.text = "";
    }

    public bool IsDialogueActive()
    {
        return isDialogueActive;
    }

    // ---------- FMOD ГОЛОСА ----------

    private void StartVoiceForSpeaker(string speakerName)
    {
        // сначала глушим предыдущий голос
        StopCurrentVoice();

        EventReference voiceEvent = new EventReference();

        // МЭППИНГ ИМЁН -> FMOD ИВЕНТ
        // Важно: в инспекторе speakerName у DialogueLine
        // должен быть строго "Daniel" или "Lester"
        if (speakerName == "Daniel")
        {
            voiceEvent = danielVoiceEvent;
        }
        else if (speakerName == "Lester")
        {
            voiceEvent = lesterVoiceEvent;
        }
        else
        {
            // если неизвестный спикер — не включаем голос
            return;
        }

        if (voiceEvent.IsNull)
        {
            Debug.LogWarning($"DialogueManager: для спикера {speakerName} не назначен FMOD Event.");
            return;
        }

        currentVoiceInstance = RuntimeManager.CreateInstance(voiceEvent);
        currentVoiceInstance.start();
        hasActiveVoice = true;
    }

    private void StopCurrentVoice()
    {
        if (!hasActiveVoice)
            return;

        if (currentVoiceInstance.isValid())
        {
            // мягкое затухание
            currentVoiceInstance.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
            currentVoiceInstance.release();
        }

        hasActiveVoice = false;
    }

    void OnDestroy()
    {
        // если объект уничтожается — не забываем освободить инстанс
        if (hasActiveVoice && currentVoiceInstance.isValid())
        {
            currentVoiceInstance.stop(FMOD.Studio.STOP_MODE.IMMEDIATE);
            currentVoiceInstance.release();
        }
    }
}

