using UnityEngine;

public class PlayerInteraction : MonoBehaviour
{
    public float interactionDistance = 3f;
    public LayerMask interactionLayerMask;
    public Camera playerCamera;

    DialogueManager dialogueManager;
    PlayerController playerController;

    OutlineInteractable currentOutline;

    DoorOutline currentDoorOutline;

    void Start()
    {
        dialogueManager = FindObjectOfType<DialogueManager>();
        playerController = GetComponent<PlayerController>();
    }

    void Update()
    {
        if (!Pause.isPaused)
            UpdateOutline();

        if (Input.GetKeyDown(KeyCode.E) && !Pause.isPaused)
            HandleInteraction();
    }

    void UpdateOutline()
    {
        Ray ray = playerCamera.ScreenPointToRay(new Vector3(Screen.width / 2, Screen.height / 2));

        if (Physics.Raycast(ray, out RaycastHit hit, interactionDistance, interactionLayerMask))
        {
            OutlineInteractable outline = hit.collider.GetComponentInParent<OutlineInteractable>();
            PlaceableItem item = hit.collider.GetComponentInParent<PlaceableItem>();
            DoorOutline doorOutline = hit.collider.GetComponentInParent<DoorOutline>();

            if (doorOutline != null)
            {
                if (currentOutline != doorOutline)
                {
                    ClearOutline();
                    currentDoorOutline = doorOutline;
                    currentDoorOutline.Show();
                }
                return;
            }

            if (outline != null && item != null && item.CurrentState == PlaceableItem.ItemState.OnGround)
            {
                if (currentOutline != outline)
                {
                    ClearOutline();
                    currentOutline = outline;
                    currentOutline.Show();
                }
                return;
            }
        }

        ClearOutline();
    }

    void ClearOutline()
    {
        if (currentOutline != null)
        {
            currentOutline.Hide();
            currentOutline = null;
        }

        if (currentDoorOutline != null)
        {
            currentDoorOutline.Hide();
            currentDoorOutline = null;
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

            if (hit.collider.CompareTag("Object"))
            {
                FocusTrigger focusTrigger = hit.collider.GetComponent<FocusTrigger>();
                if (focusTrigger != null)
                {
                    focusTrigger.TriggerFocus(playerController);
                    return;
                }
            }

            //if (hit.collider.CompareTag("Pickable"))
            //{
            //    ObjectInteraction oi = GetComponent<ObjectInteraction>();
            //    if (oi != null)
            //        oi.PickupObject(hit.collider.gameObject);
            //}
        }
    }

    bool IsDialogueActive(DialogueManager manager)
    {
        var field = typeof(DialogueManager).GetField("isDialogueActive", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        if (field != null)
            return (bool)field.GetValue(manager);

        return false;
    }
}
