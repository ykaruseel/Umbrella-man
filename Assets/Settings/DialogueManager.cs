
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class DialogueManager : MonoBehaviour
{
    // --- Переменные для UI диалога ---
    public TextMeshProUGUI speakerNameText;
    public TextMeshProUGUI dialogueText;
    public GameObject dialogueBox;

    // --- Переменные для эффекта печати ---
    [Header("Typewriter Effect")]
    public float typingSpeed = 0.05f;
    public Coroutine typingCoroutine;
    private string currentSentence;

    // --- Системные переменные ---
    public static bool IsDialogueActive = false;
    private Queue<DialogueLine> sentences;

    // --- ДОБАВЛЕНО: Ссылка на Игрока ---
    private PlayerController playerController; 

    void Start()
    {
        sentences = new Queue<DialogueLine>();
        
        // --- ДОБАВЛЕНО: Находим Игрока ---
        playerController = FindObjectOfType<PlayerController>();
        if (playerController == null)
            Debug.LogError("DialogueManager не смог найти PlayerController!");
    }

    public void StartDialogue(DialogueLine[] lines)
    {
        if (IsDialogueActive) return;

        // --- ДОБАВЛЕНО: Блокируем игрока и зумим ---
        if (playerController != null)
        {
            playerController.SetCanMove(false); // Запретить двигаться
            playerController.SetDialogueZoom(true); // Включить зум
        }
        // --- КОНЕЦ ДОБАВЛЕННОГО ---

        IsDialogueActive = true;
        dialogueBox.SetActive(true);
        sentences.Clear();

        foreach (DialogueLine line in lines)
        {
            sentences.Enqueue(line);
        }
        DisplayNextSentence();
    }

    public void DisplayNextSentence()
    {
        if (sentences.Count == 0)
        {
            EndDialogue();
            return;
        }

        DialogueLine currentLine = sentences.Dequeue();
        speakerNameText.text = currentLine.speakerName;
        currentSentence = currentLine.sentence;

        if (typingCoroutine != null)
        {
            StopCoroutine(typingCoroutine);
        }
        typingCoroutine = StartCoroutine(TypeSentence(currentSentence));
    }

    public void EndDialogue()
    {
        if (!IsDialogueActive) return;

        if (typingCoroutine != null)
        {
            StopCoroutine(typingCoroutine);
            typingCoroutine = null;
        }

        // --- ДОБАВЛЕНО: Возвращаем управление и зум ---
        if (playerController != null)
        {
            playerController.SetCanMove(true); // Разрешить двигаться
            playerController.SetDialogueZoom(false); // Выключить зум
        }
        // --- КОНЕЦ ДОБАВЛЕННОГО ---

        // --- ЭТО КОД ДЛЯ КВЕСТА 2 (Он должен быть здесь) ---
        QuestManager qm = QuestManager.instance;
        QuestObjective objective = qm?.currentQuest?.GetCurrentObjective();
        if (objective != null && objective.targetID == "door" && objective.objectiveType == ObjectiveType.Interact && !objective.isComplete)
        {
            qm.UpdateQuestProgress("door", ObjectiveType.Interact);
        }
        // --- КОНЕЦ КОДА КВЕСТА ---

        IsDialogueActive = false;
        dialogueBox.SetActive(false); 
    }

    IEnumerator TypeSentence(string sentence)
    {
        dialogueText.text = "";
        foreach (char letter in sentence.ToCharArray())
        {
            dialogueText.text += letter;
            yield return new WaitForSeconds(typingSpeed);
        }
        typingCoroutine = null;
    }

    public void SkipTyping()
    {
        if (typingCoroutine != null)
        {
            StopCoroutine(typingCoroutine);
            typingCoroutine = null;
            dialogueText.text = currentSentence;
        }
    }
}
