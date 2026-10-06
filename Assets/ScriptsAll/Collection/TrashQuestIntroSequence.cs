using System.Collections;
using UnityEngine;

public class TrashQuestIntroSequence : MonoBehaviour
{
    [Header("References")]
    [SerializeField]
    private PlayerController playerController;

    [SerializeField]
    private Transform bottlesFocusTarget;

    [SerializeField]
    private PlayerComments introComment;


    [Header("Stand Up Animation")]
    [SerializeField]
    private float sittingCameraOffset = 0.65f;

    [SerializeField]
    private float standUpDuration = 1.5f;


    [Header("Look At Bottles")]
    [SerializeField]
    private float lookAtDuration = 1.3f;

    [SerializeField]
    private float pauseBeforeComment = 0.4f;


public IEnumerator PlaySequence()
{
    Debug.Log("[TRASH INTRO] PlaySequence START");

    if (playerController == null)
    {
        Debug.LogError(
            "[TRASH INTRO] PlayerController is NULL"
        );

        yield break;
    }

    Debug.Log("[TRASH INTRO] PlayerController OK");

    if (playerController.virtualCam == null)
    {
        Debug.LogError(
            "[TRASH INTRO] virtualCam is NULL"
        );

        yield break;
    }

    Debug.Log("[TRASH INTRO] virtualCam OK");

        Pause.canPause = false;

        playerController.SetCanMove(false);
        playerController.isCinematic = true;


        // Cinemachine camera already used by PlayerController
        Transform cameraTransform =
            playerController.virtualCam.transform;


        // Normal camera position for the standing player
        Vector3 standingLocalPosition =
            cameraTransform.localPosition;


        // Simulate a seated position:
        // simply lower the camera downward
        Vector3 sittingLocalPosition =
            standingLocalPosition +
            Vector3.down * sittingCameraOffset;


        cameraTransform.localPosition =
            sittingLocalPosition;


        // Smoothly "stand up"
        yield return StartCoroutine(
            MoveCamera(
                cameraTransform,
                sittingLocalPosition,
                standingLocalPosition,
                standUpDuration
            )
        );


        // Now look at the bottles
        if (bottlesFocusTarget != null)
        {
            playerController.StartCinematicPan(
                bottlesFocusTarget,
                lookAtDuration
            );

            yield return new WaitForSeconds(
                lookAtDuration
            );
        }


        yield return new WaitForSeconds(
            pauseBeforeComment
        );
        Debug.Log("[TRASH INTRO] Camera stand-up finished");

        // Daniel's comment
        if (introComment != null)
        {
            Debug.Log("[TRASH INTRO] Starting Daniel comment");
            introComment.StartDialogue();

            while (introComment.IsDialogueActive())
            {
                yield return null;
            }
        }


        // Only AFTER the comment do we begin the first quest
        if (QuestManagerV2.Instance != null)
        {
            Debug.Log("[TRASH INTRO] Starting quest");
            QuestManagerV2.Instance.StartQuestSequence();
        }


        playerController.isCinematic = false;
        playerController.SetCanMove(true);

        Pause.canPause = true;
    }


    private IEnumerator MoveCamera(
        Transform cameraTransform,
        Vector3 from,
        Vector3 to,
        float duration)
    {
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;

            float t =
                Mathf.Clamp01(elapsed / duration);

            float smoothT =
                Mathf.SmoothStep(0f, 1f, t);

            cameraTransform.localPosition =
                Vector3.Lerp(
                    from,
                    to,
                    smoothT
                );

            yield return null;
        }

        cameraTransform.localPosition = to;
    }
}