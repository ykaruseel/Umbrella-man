using System.Collections; // Добавили для корутин
using System.Collections.Generic;
using UnityEngine;
using TMPro;

// --- ВЕРСИЯ ТОЛЬКО С ЭФФЕКТОМ ПЕЧАТИ ---
public class DialogueManager : MonoBehaviour
{
    // --- Переменные для UI диалога ---
    public TextMeshProUGUI speakerNameText;
    public TextMeshProUGUI dialogueText;
    public GameObject dialogueBox;

    // --- Переменные для эффекта печати ---
    [Header("Typewriter Effect")]
    public float typingSpeed = 0.05f; // Скорость печати
    public Coroutine typingCoroutine; // Ссылка на процесс печати
    private string currentSentence;   // Текущая печатаемая фраза

    // --- Системные переменные ---
    public static bool IsDialogueActive = false; // Активен ли диалог сейчас?
    private Queue<DialogueLine> sentences; // Очередь реплик

    void Start()
    {
        sentences = new Queue<DialogueLine>();
    }

    // --- Функция начала диалога ---
    public void StartDialogue(DialogueLine[] lines)
    {
        // Не начинаем новый диалог, если старый ещё активен
        if (IsDialogueActive) return;

        IsDialogueActive = true;
        dialogueBox.SetActive(true); // Показываем окно диалога
        sentences.Clear(); // Чистим очередь от старых реплик

        // Добавляем новые реплики в очередь
        foreach (DialogueLine line in lines)
        {
            sentences.Enqueue(line);
        }

        // Показываем первую реплику (с эффектом печати)
        DisplayNextSentence();
    }

    // --- Функция показа следующей реплики ---
    public void DisplayNextSentence()
    {
        // Если реплик больше нет, завершаем диалог
        if (sentences.Count == 0)
        {
            EndDialogue();
            return;
        }

        // Берём следующую реплику из очереди
        DialogueLine currentLine = sentences.Dequeue();
        // Показываем имя говорящего
        speakerNameText.text = currentLine.speakerName;
        // Запоминаем текст реплики (для эффекта печати и пропуска)
        currentSentence = currentLine.sentence;

        // Останавливаем предыдущую печать, если она была
        if (typingCoroutine != null)
        {
            StopCoroutine(typingCoroutine);
        }
        // Запускаем корутину для эффекта печати
        typingCoroutine = StartCoroutine(TypeSentence(currentSentence));
    }

    // --- Функция завершения диалога ---
    public void EndDialogue()
    {
        // Не завершаем, если диалог и так неактивен
        if (!IsDialogueActive) return;

        // Останавливаем печать, если она шла
        if (typingCoroutine != null)
        {
            StopCoroutine(typingCoroutine);
            typingCoroutine = null;
        }

        IsDialogueActive = false;
        dialogueBox.SetActive(false); // Прячем окно диалога
    }

    // --- Корутина для эффекта печати ---
    IEnumerator TypeSentence(string sentence)
    {
        dialogueText.text = ""; // Очищаем текст
        // Печатаем по одной букве с задержкой
        foreach (char letter in sentence.ToCharArray())
        {
            dialogueText.text += letter;
            yield return new WaitForSeconds(typingSpeed);
        }
        typingCoroutine = null; // Печать завершена
    }

    // --- Функция для пропуска печати ---
    public void SkipTyping()
    {
        // Если печать активна
        if (typingCoroutine != null)
        {
            // Останавливаем её
            StopCoroutine(typingCoroutine);
            typingCoroutine = null;
            // Показываем весь текст сразу
            dialogueText.text = currentSentence;
        }
    }
}
