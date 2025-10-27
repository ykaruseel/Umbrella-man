using UnityEngine;

public class PlayerInteraction : MonoBehaviour
{
    [Header("References")]
    public float interactDistance = 3f;
    public KeyCode interactKey = KeyCode.E;

    private NPC_Dialogue currentNPC;
    private DialogueManager dialogueManager;

    void Start()
    {
        dialogueManager = FindObjectOfType<DialogueManager>();
    }

    void Update()
    {
        HandleRaycast();
    }

    private void HandleRaycast()
    {
        if (Physics.Raycast(Camera.main.transform.position, Camera.main.transform.forward, out RaycastHit hit, interactDistance))
        {
            NPC_Dialogue npc = hit.collider.GetComponent<NPC_Dialogue>();
            if (npc != null)
            {
                if (currentNPC != npc)
                {
                    ClearCurrentPrompt();
                    currentNPC = npc;
                    if (currentNPC.interactionPrompt != null)
                        currentNPC.interactionPrompt.SetActive(true);
                }

                if (Input.GetKeyDown(interactKey))
                    InteractWithNPC(currentNPC);

                return;
            }
        }

        ClearCurrentPrompt();
    }

    private void InteractWithNPC(NPC_Dialogue npc)
    {
        if (dialogueManager == null || npc == null) return;

        PlayerController playerController = GetComponent<PlayerController>();

        if (!DialogueManager.IsDialogueActive)
        {
            dialogueManager.StartDialogue(npc.dialogueLines);

            if (playerController != null)
            {
                playerController.SetCanMove(false);
                playerController.SetDialogueZoom(true);
            }
        }
        else
        {
            if (dialogueManager.typingCoroutine != null)
                dialogueManager.SkipTyping();
            else
            {
                dialogueManager.DisplayNextSentence();

                if (!DialogueManager.IsDialogueActive && playerController != null)
                {
                    playerController.SetCanMove(true);
                    playerController.SetDialogueZoom(false);
                }
            }
        }
    }

    private void ClearCurrentPrompt()
    {
        if (currentNPC != null && currentNPC.interactionPrompt != null)
            currentNPC.interactionPrompt.SetActive(false);

        currentNPC = null;
    }
}
