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


        // Данные уже использованы.
        SceneTransitionState.Clear();


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