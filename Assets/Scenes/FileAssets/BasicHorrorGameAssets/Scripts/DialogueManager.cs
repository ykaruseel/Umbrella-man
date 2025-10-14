using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class DialogueManager : MonoBehaviour
{
    // --- ИЗМЕНЕНИЕ: Добавили поле для имени говорящего ---
    public TextMeshProUGUI speakerNameText; 
    public TextMeshProUGUI dialogueText;
    public GameObject dialogueBox;
    
    public static bool IsDialogueActive = false;

    // --- ИЗМЕНЕНИЕ: Очередь теперь хранит не string, а DialogueLine ---
    private Queue<DialogueLine> sentences;

    void Start()
    {
        sentences = new Queue<DialogueLine>();
    }

    // --- ИЗМЕНЕНИЕ: Метод теперь принимает массив DialogueLine ---
    public void StartDialogue(DialogueLine[] lines)
    {
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

        // --- ИЗМЕНЕНИЕ: Получаем целый объект DialogueLine из очереди ---
        DialogueLine currentLine = sentences.Dequeue();

        // Обновляем имя и текст реплики
        speakerNameText.text = currentLine.speakerName;
        dialogueText.text = currentLine.sentence;
    }

    public void EndDialogue()
    {
        IsDialogueActive = false;
        dialogueBox.SetActive(false);
    }
}
