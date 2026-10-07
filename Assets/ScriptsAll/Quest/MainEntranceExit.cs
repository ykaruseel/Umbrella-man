using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainEntranceExit : MonoBehaviour
{
    [SerializeField] private string requiredQuestId = "Q_TAKE_OUT_TRASH_1";
    [SerializeField] private string targetSceneName = "Backyard";

    [SerializeField] private CameraFade cameraFade;
    [SerializeField] private PlayerController playerController;

    [SerializeField] private OutlineInteractable outline;
    [SerializeField] private PulseHighlight pulseHighlight;

    [SerializeField]
    private CollectionController collectionController;

    private bool isBusy;
    private bool lastAvailable = false;

    public bool CanInteract
    {
        get
        {
            if (isBusy)
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

    private void Start()
    {
        RefreshAvailability();
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
        Debug.Log(
            $"[MainEntranceExit] TryInteract. " +
            $"CanInteract = {CanInteract}"
        );

        if (!CanInteract)
            return false;

        StartCoroutine(ExitRoutine());

        return true;
    }

    private IEnumerator ExitRoutine()
    {
        isBusy = true;

        Debug.Log(
            $"[MainEntranceExit] Loading scene: {targetSceneName}"
        );


        if (!Application.CanStreamedLevelBeLoaded(targetSceneName))
        {
            Debug.LogError(
                $"[MainEntranceExit] Scene '{targetSceneName}' " +
                "is NOT available in the Build Profile."
            );

            isBusy = false;
            yield break;
        }


        Pause.canPause = false;

        if (playerController != null)
        {
            playerController.SetCanMove(false);
            playerController.isCinematic = true;
        }


        if (cameraFade != null)
        {
            yield return StartCoroutine(
                cameraFade.FadeOut()
            );
        }

        bool hasFullTrashBag =
            collectionController != null &&
            collectionController.HasFullContainer;



        SceneTransitionState.Prepare(
            "BackyardEntrance",
            requiredQuestId,
            true
        );
        SceneManager.LoadScene(targetSceneName);
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
}