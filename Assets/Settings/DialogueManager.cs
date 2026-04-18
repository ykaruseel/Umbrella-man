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
    public GameObject dialoguePanel;   
    public TMP_Text nameText;
    public TMP_Text dialogueText;       

    [Header("Settings")]
    public float typingSpeed = 0.03f;  
    public float fadeDuration = 0.2f;   

    [Header("FMOD Voices")]
    [SerializeField] private EventReference danielVoiceEvent;
    [SerializeField] private EventReference lesterVoiceEvent;
    [SerializeField] private Transform danielTransform;
    [SerializeField] private Transform lesterTransform;
    [SerializeField] private EventReference knifeManVoiceEvent;
    [SerializeField] private Transform knifeManTransform;

    [Header("Cinematic Cameras")]
    public DialogueCameraSystem currentCameraSystem;

    private Queue<DialogueLine> linesQueue = new Queue<DialogueLine>();
    private bool isDialogueActive = false;
    
    private bool isTyping = false;       
    private string currentSentence = ""; 
  
    private Coroutine typingCoroutine;
    private Coroutine fadeCoroutine; 

    private FMOD.Studio.EventInstance currentVoiceInstance;
    private bool hasActiveVoice = false;
    public CanvasGroup dialogueCanvasGroup; 

    
    private float lastClickTime = 0f;

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

    public void DisplayNextSentence()
    {
        
        if (Time.unscaledTime - lastClickTime < 0.1f) return;
        lastClickTime = Time.unscaledTime;

        if (isTyping)
        {
            if (typingCoroutine != null) StopCoroutine(typingCoroutine);
            if (dialogueText != null) dialogueText.text = currentSentence; 
            isTyping = false;
            return; 
        }

        StopCurrentVoice(); 

        if (linesQueue.Count == 0)
        {
            EndDialogue();
            return;
        }

        // Переключаем камеру
        if (currentCameraSystem != null)
        {
            currentCameraSystem.NextLine();
        }

        DialogueLine line = linesQueue.Dequeue();
        currentSentence = line.sentence;
        if (nameText != null) nameText.text = line.speakerName;
        typingCoroutine = StartCoroutine(TypeSentence(line));
    }

    private IEnumerator TypeSentence(DialogueLine line)
    {
        isTyping = true; 
        if (dialogueText != null) dialogueText.text = "";
        StartVoiceForSpeaker(line.speakerName);

        foreach (char letter in line.sentence.ToCharArray())
        {
            while (Pause.isPaused)
            {
                SetPaused(); 
                yield return null;
            }
            SetPaused(); 

            if (dialogueText != null) dialogueText.text += letter;
            yield return new WaitForSeconds(typingSpeed);
        }

        isTyping = false; 
        StopCurrentVoice();
        typingCoroutine = null;
    }

    private void EndDialogue()
    {
        isDialogueActive = false;
        StopCurrentVoice();

        if (currentCameraSystem != null)
        {
            currentCameraSystem.EndDialogue();
        }

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

    private void StartVoiceForSpeaker(string speakerName)
    {
        StopCurrentVoice();
        EventReference voiceEvent = new EventReference();
        Transform speakerTransform = null;

        if (speakerName == "Daniel")
        {
            voiceEvent = danielVoiceEvent;
            speakerTransform = danielTransform;
        }
        else if (speakerName == "Lester")
        {
            voiceEvent = lesterVoiceEvent;
            speakerTransform = lesterTransform;
        }
        else if (speakerName == "Suspicious man with a knife")
        {
            voiceEvent = knifeManVoiceEvent;
            speakerTransform = knifeManTransform;
        }
        else return;

        if (voiceEvent.IsNull || speakerTransform == null) return;
        currentVoiceInstance = RuntimeManager.CreateInstance(voiceEvent);
        RuntimeManager.AttachInstanceToGameObject(currentVoiceInstance, speakerTransform, speakerTransform.GetComponent<Rigidbody>());
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
