using UnityEngine;

public class TrashStorageDoor : MonoBehaviour
{
    [Header("Quest")]
    [SerializeField]
    private string partOneQuestID =
        "Q_TAKE_OUT_TRASH_1";

    [SerializeField]
    private string partTwoQuestID =
        "Q_TAKE_OUT_TRASH_2";


    [Header("UI")]
    [SerializeField]
    private InteractionMessageUI messageUI;


    [Header("Highlight")]
    [SerializeField]
    private OutlineInteractable outline;


    private bool isUnlocked = false;


    public bool CanInteract
    {
        get
        {
            if (QuestManagerV2.Instance == null)
                return false;


            return
                QuestManagerV2.Instance.IsQuestActive(
                    partOneQuestID
                )
                ||
                QuestManagerV2.Instance.IsQuestActive(
                    partTwoQuestID
                );
        }
    }


    private void Awake()
    {
        if (outline == null)
        {
            outline =
                GetComponent<OutlineInteractable>();
        }
    }


    public bool TryInteract()
    {
        Debug.Log(
            $"[TrashStorageDoor] TryInteract. " +
            $"CanInteract = {CanInteract}, " +
            $"Unlocked = {isUnlocked}"
        );


        if (!CanInteract)
            return false;


        if (!isUnlocked)
        {
            if (messageUI != null)
            {
                messageUI.ShowMessage("Closed");
            }
            else
            {
                Debug.LogWarning(
                    "[TrashStorageDoor] Message UI is missing."
                );
            }

            return true;
        }


        // Take Out the Trash II:
        // здесь позже откроем дверь ключом.

        return true;
    }


    public void Unlock()
    {
        isUnlocked = true;
    }
}