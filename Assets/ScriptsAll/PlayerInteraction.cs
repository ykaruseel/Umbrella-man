using UnityEngine;

public class PlayerInteraction : MonoBehaviour
{
    public float interactionDistance = 3f;
    public LayerMask interactionLayerMask;
    public Camera playerCamera;

    DialogueManager dialogueManager;
    PlayerController playerController;

    PulseHighlight currentPulse;

    void Start()
    {
        dialogueManager = FindObjectOfType<DialogueManager>();
        playerController = GetComponent<PlayerController>();
    }

    void Update()
    {
        if (!Pause.isPaused)
            UpdatePulse();

        if (Input.GetKeyDown(KeyCode.E) && !Pause.isPaused)
            HandleInteraction();
    }

    void UpdatePulse()
    {
        Ray ray = playerCamera.ScreenPointToRay(new Vector3(Screen.width / 2, Screen.height / 2));

        if (Physics.Raycast(ray, out RaycastHit hit, interactionDistance, interactionLayerMask))
        {
            UpdatePulseHighlight(hit);
            return;
        }

        ClearPulse();
    }

    void UpdatePulseHighlight(RaycastHit hit)
    {
        PulseHighlight pulse = hit.collider.GetComponentInParent<PulseHighlight>();
        PlaceableItem placeable = hit.collider.GetComponentInParent<PlaceableItem>();

        if (pulse == null || placeable == null)
        {
            ClearPulse();
            return;
        }

        if (placeable.isPlaced || IsHoldingObject())
        {
            ClearPulse();
            return;
        }

        if (currentPulse != pulse)
        {
            ClearPulse();
            currentPulse = pulse;
            currentPulse.Show();
        }
    }

    void ClearPulse()
    {
        if (currentPulse != null)
        {
            currentPulse.Hide();
            currentPulse = null;
        }
    }

    void HandleInteraction()
    {
        if (dialogueManager != null && IsDialogueActive(dialogueManager))
            return;

        Ray ray = playerCamera.ScreenPointToRay(new Vector3(Screen.width / 2, Screen.height / 2));
        if (Physics.Raycast(ray, out RaycastHit hit, interactionDistance, interactionLayerMask))
        {
            NPC_Dialogue npcDialogue = hit.collider.GetComponent<NPC_Dialogue>();
            if (npcDialogue != null)
            {
                npcDialogue.TriggerDialogue();
                return;
            }

            InteractableObject interactable = hit.collider.GetComponent<InteractableObject>();
            if (interactable != null)
            {
                interactable.Interact();
                return;
            }

            if (hit.collider.CompareTag("Pickable"))
            {
                ObjectInteraction objectInteraction = GetComponent<ObjectInteraction>();
                if (objectInteraction != null)
                    objectInteraction.PickupObject(hit.collider.gameObject);
            }
        }
    }

    bool IsHoldingObject()
    {
        ObjectInteraction oi = GetComponent<ObjectInteraction>();
        return oi != null && oi.IsHoldingObject();
    }

    bool IsDialogueActive(DialogueManager manager)
    {
        var field = typeof(DialogueManager).GetField("isDialogueActive", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        if (field != null)
            return (bool)field.GetValue(manager);

        return false;
    }
}
