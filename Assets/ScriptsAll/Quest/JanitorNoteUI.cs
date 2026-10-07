using TMPro;
using UnityEngine;

public class JanitorNoteUI : MonoBehaviour
{
    public static JanitorNoteUI CurrentOpenNote { get; private set; }

    [SerializeField]
    private GameObject panelRoot;

    [SerializeField]
    private CanvasGroup panelCanvasGroup;

    [SerializeField]
    private TextMeshProUGUI bodyText;

    [TextArea(8, 20)]
    [SerializeField]
    private string noteText;

    [SerializeField]
    private PlayerController playerController;


    private bool isOpen;

    public bool IsOpen => isOpen;


    private void Awake()
    {
        if (bodyText != null)
        {
            bodyText.text = noteText;
            bodyText.ForceMeshUpdate();
        }

        HideImmediate();
    }


    public void Open()
    {
        if (isOpen)
            return;

        Debug.Log("[JanitorNoteUI] OPEN");

        isOpen = true;
        CurrentOpenNote = this;


        if (panelRoot != null)
        {
            panelRoot.SetActive(true);
            panelRoot.transform.SetAsLastSibling();
        }


        if (bodyText != null)
        {
            bodyText.text = noteText;
            bodyText.ForceMeshUpdate();
        }


        if (panelCanvasGroup != null)
        {
            panelCanvasGroup.alpha = 1f;
            panelCanvasGroup.interactable = true;
            panelCanvasGroup.blocksRaycasts = true;
        }


        Canvas.ForceUpdateCanvases();


        if (playerController != null)
        {
            playerController.SetCanMove(false);
            playerController.isCinematic = true;
        }

        Pause.canPause = false;
    }


    public void Close()
    {
        if (!isOpen)
            return;

        Debug.Log("[JanitorNoteUI] CLOSE");

        isOpen = false;

        if (CurrentOpenNote == this)
            CurrentOpenNote = null;


        HideImmediate();


        if (playerController != null)
        {
            playerController.isCinematic = false;
            playerController.SetCanMove(true);
        }

        Pause.canPause = true;
    }


    private void HideImmediate()
    {
        if (panelCanvasGroup != null)
        {
            panelCanvasGroup.alpha = 0f;
            panelCanvasGroup.interactable = false;
            panelCanvasGroup.blocksRaycasts = false;
        }
    }
}