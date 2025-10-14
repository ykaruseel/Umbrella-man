using UnityEngine;

public class NPC_Dialogue : MonoBehaviour
{
    // --- ГЛАВНОЕ ИЗМЕНЕНИЕ: Используем наш новый класс DialogueLine ---
    public DialogueLine[] dialogueLines;

    public GameObject interactionPrompt;
    private bool playerInRange = false;

    void Update()
    {
        if (playerInRange && Input.GetKeyDown(KeyCode.E))
        {
            if (!DialogueManager.IsDialogueActive)
            {
                FindObjectOfType<DialogueManager>().StartDialogue(dialogueLines);
            }
            else
            {
                FindObjectOfType<DialogueManager>().DisplayNextSentence();
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = true;
            if (interactionPrompt != null) interactionPrompt.SetActive(true);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = false;
            if (interactionPrompt != null) interactionPrompt.SetActive(false);
            FindObjectOfType<DialogueManager>().EndDialogue();
        }
    }
}
