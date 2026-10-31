using System.Collections;
using UnityEngine;

public class MainSceneBackyardEntryController : MonoBehaviour
{
    [Header("Player")]
    [SerializeField]
    private PlayerController playerController;

    [SerializeField]
    private Transform entranceSpawnPoint;


    [Header("Fade")]
    [SerializeField]
    private CameraFade cameraFade;


    [Header("Settings")]
    [SerializeField]
    private string expectedSpawnPointID =
        "MainSceneEntranceFromBackyard";

    [Header("Completed Trash Collection")]
    [SerializeField]
    private CollectionController collectionController;

    [SerializeField]
    private TrashCarryVisual trashCarryVisual;


    private IEnumerator Start()
    {
        // MainScene открыт напрямую в Editor.
        if (!SceneTransitionState.HasPendingTransition)
        {
            yield break;
        }


        // Transition существует,
        // но пришли не из Backyard.
        if (SceneTransitionState.SpawnPointID !=
            expectedSpawnPointID)
        {
            yield break;
        }


        Pause.canPause = false;


        if (playerController != null)
        {
            playerController.SetCanMove(false);
            playerController.isCinematic = true;
        }


        // -------------------------
        // PLAYER SPAWN
        // -------------------------

        TeleportPlayer();


        // -------------------------
        // RESTORE QUEST
        // -------------------------

        string questID =
            SceneTransitionState.ActiveQuestID;


        if (QuestManagerV2.Instance != null &&
            !string.IsNullOrEmpty(questID))
        {
            bool success =
                QuestManagerV2.Instance
                    .ActivateQuestByID(
                        questID,
                        false
                    );


            Debug.Log(
                $"[MainSceneBackyardEntry] " +
                $"Activate quest '{questID}': " +
                $"{success}"
            );
        }

        // -----------------------------------
        // RESTORE COMPLETED TRASH COLLECTION
        // -----------------------------------

        if (collectionController != null)
        {
            
            bool returningWithTrash =
                questID == "Q_TAKE_OUT_TRASH_1";

            if (collectionController != null)
            {
                if (returningWithTrash)
                {
                    collectionController.RestoreAfterTrashCollection();
                }
                else
                {
                    collectionController.RestoreAfterTrashDisposal();
                }
            }

            if (trashCarryVisual != null)
            {
                if (returningWithTrash)
                {
                    trashCarryVisual.ShowFullBag();
                }
                else
                {
                    trashCarryVisual.HideFullBag();
                }
            }

        }

        if (trashCarryVisual != null)
        {
            trashCarryVisual.HideFullBag();
}
        SceneTransitionState.Clear();

        // Wait one frame so scene objects
        // finish their Start() initialization.
        yield return null;

        if (questID == "Q3" &&
            QuestEvents.Instance != null)
        {
            QuestEvents.Instance.ApplyQuest3SceneState();
        }


        // -------------------------
        // FADE IN
        // -------------------------

        if (cameraFade != null)
        {
            cameraFade.SetFadeImageActive(
                true
            );

            cameraFade.SetFadeAlpha(
                1f
            );


            yield return StartCoroutine(
                cameraFade.FadeIn()
            );
        }


        // -------------------------
        // CONTROL BACK
        // -------------------------

        if (playerController != null)
        {
            playerController.isCinematic = false;
            playerController.SetCanMove(true);
        }


        Pause.canPause = true;
    }


    private void TeleportPlayer()
    {
        if (playerController == null ||
            entranceSpawnPoint == null)
        {
            return;
        }


        CharacterController controller =
            playerController.GetComponent<
                CharacterController
            >();


        if (controller != null)
        {
            controller.enabled = false;
        }


        playerController.transform.position =
            entranceSpawnPoint.position;


        playerController.SetRotation(
            entranceSpawnPoint.eulerAngles.y,
            0f
        );


        if (controller != null)
        {
            controller.enabled = true;
        }
    }
}