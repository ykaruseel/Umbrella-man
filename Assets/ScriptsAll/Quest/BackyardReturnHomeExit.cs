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
            if (isBusy)
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


        // ---------------------------------
        // COMPLETE BACKYARD RETURN HOME
        // ---------------------------------

        if (QuestManagerV2.Instance != null)
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
            mainSceneQuestID,
            false
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