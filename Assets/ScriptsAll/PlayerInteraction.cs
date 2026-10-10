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
        if (PlayerController.playerComments != null)
        {
            ClearOutline();
            return;
        }

        if (JanitorNoteUI.CurrentOpenNote != null)
        {
            ClearOutline();
            return;
        }

        if (!Pause.isPaused)
            UpdateOutline();

        if (Input.GetKeyDown(KeyCode.E) && !Pause.isPaused)
            HandleInteraction();
    }

    void UpdateOutline()
    {
        Ray ray = playerCamera.ScreenPointToRay(new Vector3(Screen.width / 2, Screen.height / 2));

        if (Physics.Raycast(
            ray,
            out RaycastHit hit,
            interactionDistance,
            interactionLayerMask,
            QueryTriggerInteraction.Collide))
        {
            OutlineInteractable outline = hit.collider.GetComponentInParent<OutlineInteractable>();
            CollectionPickup collectionPickup = hit.collider.GetComponentInParent<CollectionPickup>();
            CollectibleItem collectible = hit.collider.GetComponentInParent<CollectibleItem>();

            PlaceableItem item = hit.collider.GetComponentInParent<PlaceableItem>();
            DoorOutline doorOutline = hit.collider.GetComponentInParent<DoorOutline>();

            JanitorNoteInteractable janitorNote =
                hit.collider.GetComponentInParent<JanitorNoteInteractable>();

            MainEntranceExit mainEntranceExit =
                hit.collider.GetComponentInParent<MainEntranceExit>();

            TrashStorageDoor trashStorageDoor =
                hit.collider.GetComponentInParent<TrashStorageDoor>();

            BackyardReturnHomeExit returnHomeExit =
                hit.collider.GetComponentInParent<
                    BackyardReturnHomeExit
                >();

            TrashStorageKey trashStorageKey =
                hit.collider.GetComponentInParent<
                    TrashStorageKey
                >();

            TrashDumpsterInteractable dumpster =
                hit.collider.GetComponentInParent<
                    TrashDumpsterInteractable
                >();

            if (collectionPickup != null &&
                collectionPickup.CanInteract &&
                outline != null)
            {
                if (currentOutline != outline)
                {
                    ClearOutline();

                    currentOutline = outline;
                    currentOutline.Show();
                }

                return;
            }


            if (collectible != null &&
                collectible.CanInteract &&
                outline != null)
            {
                if (currentOutline != outline)
                {
                    ClearOutline();

                    currentOutline = outline;
                    currentOutline.Show();
                }

                return;
            }

            if (trashStorageKey != null &&
                trashStorageKey.CanInteract &&
                outline != null)
            {
                if (currentOutline != outline)
                {
                    ClearOutline();

                    currentOutline = outline;
                    currentOutline.Show();
                }

                return;
            }

            if (returnHomeExit != null &&
                returnHomeExit.CanInteract &&
                outline != null)
            {
                if (currentOutline != outline)
                {
                    ClearOutline();

                    currentOutline = outline;
                    currentOutline.Show();
                }

                return;
            }


            if (dumpster != null &&
                dumpster.CanInteract &&
                outline != null)
            {
                if (currentOutline != outline)
                {
                    ClearOutline();

                    currentOutline = outline;
                    currentOutline.Show();
                }

                return;
            }

            if (janitorNote != null &&
                janitorNote.CanInteract &&
                outline != null)
            {
                if (currentOutline != outline)
                {
                    ClearOutline();

                    currentOutline = outline;
                    currentOutline.Show();
                }

                return;
            }


            if (mainEntranceExit != null &&
                mainEntranceExit.CanInteract &&
                outline != null)
            {
                if (currentOutline != outline)
                {
                    ClearOutline();

                    currentOutline = outline;
                    currentOutline.Show();
                }

                return;
            }

            if (trashStorageDoor != null &&
                trashStorageDoor.CanInteract &&
                outline != null)
            {
                if (currentOutline != outline)
                {
                    ClearOutline();

                    currentOutline = outline;
                    currentOutline.Show();
                }

                return;
            }

            if (doorOutline != null && doorOutline.enabled)
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
        if (Physics.Raycast(
            ray,
            out RaycastHit hit,
            interactionDistance,
            interactionLayerMask,
            QueryTriggerInteraction.Collide))
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

            if(hit.collider.CompareTag("NeighborDoor"))
            {
                NeighborDoor neighborDoor = hit.collider.GetComponentInParent<NeighborDoor>();
                if (neighborDoor != null)
                {
                    neighborDoor.Interact(playerController);
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
