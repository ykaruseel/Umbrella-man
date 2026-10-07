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

    private Vector3 standingCameraPosition;
    private bool seatedPosePrepared = false;

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


        if (!seatedPosePrepared)
        {
            // Fallback
            standingCameraPosition =
                cameraTransform.localPosition;

            cameraTransform.localPosition =
                standingCameraPosition +
                Vector3.down * sittingCameraOffset;
        }


        Vector3 sittingLocalPosition =
            cameraTransform.localPosition;


        // stand up
        yield return StartCoroutine(
            MoveCamera(
                cameraTransform,
                sittingLocalPosition,
                standingCameraPosition,
                standUpDuration
            )
        );

        seatedPosePrepared = false;


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

        SyncPlayerRotationWithCamera();

        playerController.isCinematic = false;
        playerController.SetCanMove(true);
        Pause.canPause = true;

        if (QuestManagerV2.Instance != null)
        {
            QuestManagerV2.Instance.StartQuestSequence();
        }
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

    public void PrepareSeatedPose()
    {
        if (playerController == null ||
            playerController.virtualCam == null)
        {
            Debug.LogWarning(
                "[TrashQuestIntroSequence] Cannot prepare seated pose."
            );
            return;
        }

        Transform cameraTransform =
            playerController.virtualCam.transform;

        // Remember the standing position for later
        standingCameraPosition =
            cameraTransform.localPosition;

        // Simulate a seated position by lowering the camera
        cameraTransform.localPosition =
            standingCameraPosition +
            Vector3.down * sittingCameraOffset;

        seatedPosePrepared = true;
    }


    private void SyncPlayerRotationWithCamera()
    {
        if (playerController == null ||
            playerController.virtualCam == null)
            return;

        Transform cameraTransform =
            playerController.virtualCam.transform;

        Vector3 cameraEuler =
            cameraTransform.eulerAngles;

        float yaw = cameraEuler.y;

        float pitch = cameraEuler.x;

        if (pitch > 180f)
            pitch -= 360f;

        playerController.SetRotation(
            yaw,
            pitch
        );
    }
}