// 📁 Assets/ScriptsAll/DialogueManager.cs
using UnityEngine;
using TMPro;
using System.Collections;
using System.Collections.Generic;

public class DialogueManager : MonoBehaviour
{
    [Header("UI References")]
    public GameObject dialogueUI;               // Панель диалога
    public TextMeshProUGUI speakerNameText;     // Имя персонажа
    public TextMeshProUGUI dialogueText;        // Реплика
    public float typingSpeed = 0.03f;           // Скорость "печати" текста

    private Queue<DialogueLine> sentences = new Queue<DialogueLine>();
    private bool isTyping = false;
    private string currentSentence = "";
    private Coroutine typingCoroutine;

    private bool isDialogueActive = false;

    private PlayerController playerController;

    void Start()
    {
        if (dialogueUI) dialogueUI.SetActive(false);
        playerController = FindObjectOfType<PlayerController>();
    }

    // 🔹 Запуск диалога
    public void StartDialogue(DialogueLine[] dialogueLines)
    {
        if (dialogueLines == null || dialogueLines.Length == 0)
        {
            Debug.LogWarning("DialogueManager: диалог пуст!");
            return;
        }

        isDialogueActive = true;
        sentences.Clear();

        foreach (DialogueLine line in dialogueLines)
            sentences.Enqueue(line);

        if (dialogueUI != null)
            dialogueUI.SetActive(true);

        DisplayNextSentence();
    }

    // 🔹 Показ следующей реплики
    public void DisplayNextSentence()
    {
        if (isTyping)
        {
            // Если игрок нажал E во время печати — досвечиваем текст
            StopCoroutine(typingCoroutine);
            dialogueText.text = currentSentence;
            isTyping = false;
            return;
        }

        if (sentences.Count == 0)
        {
            EndDialogue();
            return;
        }

        DialogueLine line = sentences.Dequeue();
        currentSentence = line.sentence;
        speakerNameText.text = line.speakerName;

        if (typingCoroutine != null)
            StopCoroutine(typingCoroutine);

        typingCoroutine = StartCoroutine(TypeSentence(currentSentence));
    }

    // 🔹 Эффект печати текста
    private IEnumerator TypeSentence(string sentence)
    {
        isTyping = true;
        dialogueText.text = "";

        foreach (char letter in sentence.ToCharArray())
        {
            dialogueText.text += letter;
            yield return new WaitForSeconds(typingSpeed);
        }

        isTyping = false;
    }

    // 🔹 Завершение диалога
    public void EndDialogue()
    {
        isDialogueActive = false;

        if (dialogueUI != null)
            dialogueUI.SetActive(false);

        // Возвращаем управление игроку, если оно было заблокировано
        if (playerController != null)
        {
            playerController.SetCanMove(true);
            playerController.SetDialogueZoom(false);
        }

        Debug.Log("💬 Диалог завершён.");
    }

    // ✅ Возвращает состояние диалога (для NPC_Dialogue)
    public bool IsDialogueActive()
    {
        return isDialogueActive;
    }
}
