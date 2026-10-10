using UnityEngine;

public class TrashStorageKey : MonoBehaviour
{
    [Header("Quest")]
    [SerializeField]
    private string requiredQuestID =
        "Q_TAKE_OUT_TRASH_2";


    [Header("Door")]
    [SerializeField]
    private TrashStorageDoor storageDoor;


    [Header("Highlight")]
    [SerializeField]
    private OutlineInteractable outline;

    [SerializeField]
    private PulseHighlight pulseHighlight;


    private bool pickedUp = false;

    private bool lastAvailable = false;


    public bool CanInteract
    {
        get
        {
            if (pickedUp)
                return false;


            if (QuestManagerV2.Instance == null)
                return false;


            return QuestManagerV2.Instance.IsQuestActive(
                requiredQuestID
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


        if (pulseHighlight == null)
        {
            pulseHighlight =
                GetComponent<PulseHighlight>();
        }
    }


    private void Start()
    {
        RefreshAvailability();
    }


    private void Update()
    {
        bool available =
            CanInteract;


        if (available != lastAvailable)
        {
            RefreshAvailability();
        }
    }


    private void RefreshAvailability()
    {
        bool available =
            CanInteract;


        if (outline != null)
        {
            outline.isBlocked =
                !available;


            if (!available)
            {
                outline.Hide();
            }
        }


        if (pulseHighlight != null)
        {
            if (available)
            {
                pulseHighlight.Show();
            }
            else
            {
                pulseHighlight.Hide();
            }
        }


        lastAvailable =
            available;
    }


    public bool TryInteract()
    {
        if (!CanInteract)
            return false;


        pickedUp = true;


        if (outline != null)
        {
            outline.Hide();
            outline.isBlocked = true;
        }


        if (pulseHighlight != null)
        {
            pulseHighlight.Hide();
        }


        if (storageDoor != null)
        {
            storageDoor.Unlock();
        }
        else
        {
            Debug.LogWarning(
                "[TrashStorageKey] " +
                "Storage door is missing."
            );
        }


        Debug.Log(
            "[TrashStorageKey] Key collected."
        );


        gameObject.SetActive(false);


        return true;
    }
}