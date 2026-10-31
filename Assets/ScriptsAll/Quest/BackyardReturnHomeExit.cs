using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class BackyardReturnHomeExit : MonoBehaviour
{
    [Header("Quest")]
    [SerializeField]
    private string requiredQuestID =
        "Q_RETURN_HOME";

    [SerializeField]
    private string questTargetID =
        "enter_building_after_trash";


    [Header("Destination")]
    [SerializeField]
    private string targetSceneName =
        "MainScene";

    [SerializeField]
    private string targetSpawnPointID =
        "MainSceneEntranceFromBackyard";

    // Пока используем существующий Return Home
    // из MainScene.
    [SerializeField]
    private string mainSceneQuestID =
        "Q3";


    [Header("References")]
    [SerializeField]
    private PlayerController playerController;

    [SerializeField]
    private CameraFade cameraFade;

    [SerializeField]
    private OutlineInteractable outline;


    private bool isBusy = false;



    public bool CanInteract
    {
        get
        {
            if (isBusy ||
                QuestManagerV2.Instance == null)
            {
                return false;
            }

            return
                QuestManagerV2.Instance.IsQuestActive(
                    requiredQuestID
                ) ||
                QuestManagerV2.Instance.IsQuestActive(
                    "Q_TAKE_OUT_TRASH_1"
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
            $"[BackyardReturnHomeExit] " +
            $"TryInteract. CanInteract = {CanInteract}"
        );


        if (!CanInteract)
            return false;


        StartCoroutine(
            EnterBuildingRoutine()
        );


        return true;
    }


    private IEnumerator EnterBuildingRoutine()
    {
        isBusy = true;

        Pause.canPause = false;


        if (playerController != null)
        {
            playerController.SetCanMove(false);
            playerController.isCinematic = true;
        }


        if (outline != null)
        {
            outline.Hide();
            outline.isBlocked = true;
        }

        bool returningWithTrash =
        QuestManagerV2.Instance != null &&
        QuestManagerV2.Instance.IsQuestActive(
            "Q_TAKE_OUT_TRASH_1"
        );

        // ---------------------------------
        // COMPLETE BACKYARD RETURN HOME
        // ---------------------------------

        if (!returningWithTrash &&
            QuestManagerV2.Instance != null)
        {
            QuestManagerV2.Instance.ProcessAction(
                questTargetID,
                GoalType.ReachPoint,
                true
            );
        }


        // ---------------------------------
        // FADE OUT
        // ---------------------------------

        if (cameraFade != null)
        {
            yield return StartCoroutine(
                cameraFade.FadeOut()
            );
        }


        // ---------------------------------
        // PREPARE MAIN SCENE
        // ---------------------------------

        SceneTransitionState.Prepare(
            targetSpawnPointID,
            returningWithTrash
                ? "Q_TAKE_OUT_TRASH_1"
                : mainSceneQuestID,
            returningWithTrash
        );


        // ---------------------------------
        // LOAD MAIN SCENE
        // ---------------------------------

        if (!Application.CanStreamedLevelBeLoaded(
            targetSceneName
        ))
        {
            Debug.LogError(
                $"[BackyardReturnHomeExit] " +
                $"Scene '{targetSceneName}' " +
                "is not available in Build Profile."
            );


            isBusy = false;
            yield break;
        }


        SceneManager.LoadScene(
            targetSceneName
        );
    }
}