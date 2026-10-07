using System.Collections;
using UnityEngine;

public class BackyardEntryController : MonoBehaviour
{
    [Header("Player")]
    [SerializeField]
    private PlayerController playerController;

    [SerializeField]
    private Transform entranceSpawnPoint;


    [Header("Trash")]
    [SerializeField]
    private TrashCarryVisual trashCarryVisual;


    [Header("Entry Sequence")]
    [SerializeField]
    private CameraFade cameraFade;

    [SerializeField]
    private PlayerComments entryComment;

    [SerializeField]
    private float delayBeforeComment = 0.3f;


    private IEnumerator Start()
    {
        // Backyard был открыт напрямую из Editor.
        // Тогда автоматическую entry sequence не запускаем.
        if (!SceneTransitionState.HasPendingTransition)
        {
            Debug.Log(
                "[BackyardEntryController] " +
                "No transition data."
            );

            yield break;
        }


        Pause.canPause = false;


        if (playerController != null)
        {
            playerController.SetCanMove(false);
            playerController.isCinematic = true;
        }


        // --------------------
        // Spawn
        // --------------------

        if (SceneTransitionState.SpawnPointID ==
            "BackyardEntrance")
        {
            TeleportPlayerToEntrance();
        }


        // --------------------
        // Quest
        // --------------------

        if (QuestManagerV2.Instance != null &&
            !string.IsNullOrEmpty(
                SceneTransitionState.ActiveQuestID
            ))
        {
            QuestManagerV2.Instance.ActivateQuestByID(
                SceneTransitionState.ActiveQuestID,
                false
            );
        }


        // --------------------
        // Trash bag
        // --------------------

        if (trashCarryVisual != null)
        {
            if (SceneTransitionState.HasFullTrashBag)
            {
                trashCarryVisual.ShowFullBag();
            }
            else
            {
                trashCarryVisual.HideFullBag();
            }
        }


        // Теперь данные уже использованы.
        SceneTransitionState.Clear();


        // --------------------
        // Fade in
        // --------------------

        if (cameraFade != null)
        {
            cameraFade.SetFadeImageActive(true);
            cameraFade.SetFadeAlpha(1f);

            yield return StartCoroutine(
                cameraFade.FadeIn()
            );
        }


        if (delayBeforeComment > 0f)
        {
            yield return new WaitForSeconds(
                delayBeforeComment
            );
        }


        // --------------------
        // Daniel comment
        // --------------------

        if (entryComment != null)
        {
            entryComment.StartDialogue();

            while (entryComment.IsDialogueActive())
            {
                yield return null;
            }
        }


        // --------------------
        // Control back
        // --------------------

        if (playerController != null)
        {
            playerController.isCinematic = false;
            playerController.SetCanMove(true);
        }

        Pause.canPause = true;
    }


    private void TeleportPlayerToEntrance()
    {
        if (playerController == null ||
            entranceSpawnPoint == null)
        {
            return;
        }


        CharacterController characterController =
            playerController.GetComponent<CharacterController>();


        if (characterController != null)
        {
            characterController.enabled = false;
        }


        playerController.transform.position =
            entranceSpawnPoint.position;

        playerController.transform.rotation =
            entranceSpawnPoint.rotation;


        playerController.SetRotation(
            entranceSpawnPoint.eulerAngles.y,
            0f
        );


        if (characterController != null)
        {
            characterController.enabled = true;
        }
    }
}