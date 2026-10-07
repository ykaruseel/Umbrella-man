using UnityEngine;

public class CollectibleItem : MonoBehaviour
{
    [Header("ID")]
    [SerializeField]
    private string collectibleID;

    [Header("Interaction")]
    [SerializeField]
    private Collider interactionCollider;

    [SerializeField]
    private OutlineInteractable outline;

    [SerializeField]
    private PulseHighlight pulseHighlight;

    private Rigidbody rb;

    private Vector3 initialScale;

    public string ID => collectibleID;

    public bool IsCollected { get; private set; }

    public bool IsAvailable { get; private set; }

    public bool CanInteract =>
        IsAvailable && !IsCollected;


    private void Awake()
    {
        if (interactionCollider == null)
            interactionCollider = GetComponentInChildren<Collider>();

        if (outline == null)
            outline = GetComponent<OutlineInteractable>();

        if (pulseHighlight == null)
            pulseHighlight = GetComponent<PulseHighlight>();

        rb = GetComponent<Rigidbody>();

        initialScale = transform.localScale;

        SetAvailable(false);
    }


    public void SetAvailable(bool value)
    {
        if (IsCollected)
            value = false;

        IsAvailable = value;

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


    public void BeginCollection()
    {
        if (IsCollected)
            return;

        IsCollected = true;
        IsAvailable = false;

        if (outline != null)
        {
            outline.Hide();
            outline.isBlocked = true;
        }

        if (pulseHighlight != null)
            pulseHighlight.Hide();

        if (interactionCollider != null)
            interactionCollider.enabled = false;

        if (rb != null)
        {
            rb.useGravity = false;
            rb.isKinematic = true;
        }
    }


    public void HideAfterCollection()
    {
        gameObject.SetActive(false);
    }


    public void RestoreCollected()
    {
        IsCollected = true;
        IsAvailable = false;

        gameObject.SetActive(false);
    }


    public void RestoreUncollected(bool available)
    {
        gameObject.SetActive(true);

        IsCollected = false;

        transform.localScale = initialScale;

        if (interactionCollider != null)
            interactionCollider.enabled = true;

        SetAvailable(available);
    }
}