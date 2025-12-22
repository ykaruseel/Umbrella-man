using UnityEngine;

public class PlayerInteraction : MonoBehaviour
{
    public float interactionDistance = 3f;
    public LayerMask interactionLayerMask;
    public Camera playerCamera;

    private DialogueManager dialogueManager;
    private PlayerController playerController;

    void Start()
    {
        dialogueManager = FindObjectOfType<DialogueManager>();
        playerController = GetComponent<PlayerController>();
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.E) && !Pause.isPaused)
        {
            HandleInteraction();
        }
    }

    void HandleInteraction()
    {
        // Если диалог активен, не даём взаимодействовать
        if (dialogueManager != null && IsDialogueActive(dialogueManager))
            return;

        Ray ray = playerCamera.ScreenPointToRay(new Vector3(Screen.width / 2, Screen.height / 2));
        if (Physics.Raycast(ray, out RaycastHit hit, interactionDistance, interactionLayerMask))
        {
            // Проверяем, есть ли на объекте NPC_Dialogue
            NPC_Dialogue npcDialogue = hit.collider.GetComponent<NPC_Dialogue>();
            if (npcDialogue != null)
            {
                npcDialogue.TriggerDialogue();
                return;
            }

            // Проверяем, есть ли InteractableObject (например, щиток)
            InteractableObject interactable = hit.collider.GetComponent<InteractableObject>();
            if (interactable != null)
            {
                interactable.Interact();
                return;
            }

            // Проверяем, можно ли поднять предмет
            if (hit.collider.CompareTag("Pickable"))
            {
                ObjectInteraction objectInteraction = GetComponent<ObjectInteraction>();
                if (objectInteraction != null)
                {
                    objectInteraction.PickupObject(hit.collider.gameObject);
                }
            }
        }
    }

    private bool IsDialogueActive(DialogueManager manager)
    {
        // Проверяем, активен ли диалог
        var field = typeof(DialogueManager).GetField("isDialogueActive", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        if (field != null)
        {
            return (bool)field.GetValue(manager);
        }
        return false;
    }
}
