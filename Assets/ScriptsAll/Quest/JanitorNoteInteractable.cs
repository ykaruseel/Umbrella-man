using UnityEngine;

public class JanitorNoteInteractable : MonoBehaviour
{
    [SerializeField] private JanitorNoteUI noteUI;
    [SerializeField] private OutlineInteractable outline;
    [SerializeField] private PulseHighlight pulseHighlight;

    [SerializeField] private string requiredQuestId = "Q_TAKE_OUT_TRASH_1";

    public bool CanInteract
    {
        get
        {
            if (noteUI == null || noteUI.IsOpen)
                return false;

            if (QuestManagerV2.Instance == null)
                return false;

            return QuestManagerV2.Instance.IsQuestActive(requiredQuestId);
        }
    }

    private void Awake()
    {
        if (outline == null)
            outline = GetComponent<OutlineInteractable>();

        if (pulseHighlight == null)
            pulseHighlight = GetComponent<PulseHighlight>();
    }

   private bool lastAvailable = false;

    private void Start()
    {
        RefreshAvailability();
    }

    private void Update()
    {
        bool available =
            QuestManagerV2.Instance != null &&
            QuestManagerV2.Instance.IsQuestActive(requiredQuestId);

        if (available != lastAvailable)
        {
            RefreshAvailability();
        }
    }

    public void RefreshAvailability()
    {
        bool available = QuestManagerV2.Instance != null &&
                         QuestManagerV2.Instance.IsQuestActive(requiredQuestId);

        if (outline != null)
        {
            outline.isBlocked = !available;
            if (!available)
                outline.Hide();
        }

        if (pulseHighlight != null)
        {
            if (available)
                pulseHighlight.Show();
            else
                pulseHighlight.Hide();
        }

        lastAvailable = available;
    }

    public bool TryInteract()
    {
        if (!CanInteract)
            return false;

        noteUI.Open();
        return true;
    }
}