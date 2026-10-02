using UnityEngine;

public class CollectionPickup : MonoBehaviour
{
    [SerializeField]
    private CollectionController collectionController;

    [SerializeField]
    private OutlineInteractable outline;

    [SerializeField]
    private PulseHighlight pulseHighlight;

    private bool isAvailable;

    public bool CanInteract =>
        isAvailable &&
        collectionController != null &&
        collectionController.CanAcquireContainer;


    private void Awake()
    {
        if (outline == null)
            outline = GetComponent<OutlineInteractable>();

        if (pulseHighlight == null)
            pulseHighlight = GetComponent<PulseHighlight>();

        SetAvailable(false);
    }


    public void SetAvailable(bool value)
    {
        isAvailable = value;

        if (outline != null)
        {
            outline.isBlocked = !value;

            if (!value)
                outline.Hide();
        }

        if (pulseHighlight != null)
        {
            if (value)
                pulseHighlight.Show();
            else
                pulseHighlight.Hide();
        }
    }


    public bool TryInteract()
    {
        if (!CanInteract)
            return false;

        if (!collectionController.AcquireContainer())
            return false;

        SetAvailable(false);

        gameObject.SetActive(false);

        return true;
    }


    public void RestoreState(
        bool alreadyPickedUp,
        bool shouldBeAvailable)
    {
        gameObject.SetActive(!alreadyPickedUp);

        if (!alreadyPickedUp)
            SetAvailable(shouldBeAvailable);
    }
}