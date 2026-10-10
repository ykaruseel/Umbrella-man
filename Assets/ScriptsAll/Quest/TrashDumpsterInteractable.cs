using System.Collections;
using UnityEngine;

public class TrashDumpsterInteractable : MonoBehaviour
{
    [Header("Quest")]
    [SerializeField]
    private string requiredQuestID =
        "Q_TAKE_OUT_TRASH_2";

    [SerializeField]
    private string questEndTargetID =
        "take_out_trash_2_end";


    [Header("Player")]
    [SerializeField]
    private PlayerController playerController;


    [Header("Trash")]
    [SerializeField]
    private TrashCarryVisual trashCarryVisual;

    [SerializeField]
    private GameObject disposalBagVisual;

    [SerializeField]
    private Transform bagStartPoint;

    [SerializeField]
    private Transform bagEndPoint;

    [SerializeField]
    private float bagDropDuration =
        1.2f;


    [Header("Camera")]
    [SerializeField]
    private Transform cameraTarget;

    [SerializeField]
    private float cameraPanDuration =
        0.5f;


    [Header("Highlight")]
    [SerializeField]
    private OutlineInteractable outline;


    private bool used = false;


    public bool CanInteract
    {
        get
        {
            if (used)
                return false;


            if (QuestManagerV2.Instance == null)
                return false;


            if (!QuestManagerV2.Instance.IsQuestActive(
                requiredQuestID
            ))
            {
                return false;
            }


            if (trashCarryVisual == null)
                return false;


            return trashCarryVisual.HasBag;
        }
    }


    private void Awake()
    {
        if (outline == null)
        {
            outline =
                GetComponent<OutlineInteractable>();
        }


        if (disposalBagVisual != null)
        {
            disposalBagVisual.SetActive(
                false
            );
        }
    }


    public bool TryInteract()
    {
        if (!CanInteract)
            return false;


        used = true;


        StartCoroutine(
            DisposalRoutine()
        );


        return true;
    }


    private IEnumerator DisposalRoutine()
    {
        Pause.canPause = false;


        if (outline != null)
        {
            outline.Hide();
            outline.isBlocked = true;
        }


        if (playerController != null)
        {
            playerController.SetCanMove(false);
            playerController.isCinematic = true;
        }


        // ------------------------
        // CAMERA → DUMPSTER
        // ------------------------

        if (cameraTarget != null)
        {
            yield return StartCoroutine(
                PanCameraToTarget(
                    cameraTarget,
                    cameraPanDuration
                )
            );
        }


        // ------------------------
        // BAG LEAVES PLAYER HAND
        // ------------------------

        if (trashCarryVisual != null)
        {
            trashCarryVisual.HideFullBag();
        }


        // ------------------------
        // WORLD BAG APPEARS
        // ------------------------

        if (disposalBagVisual != null &&
            bagStartPoint != null &&
            bagEndPoint != null)
        {
            disposalBagVisual.transform.position =
                bagStartPoint.position;

            disposalBagVisual.transform.rotation =
                bagStartPoint.rotation;

            disposalBagVisual.SetActive(
                true
            );


            Vector3 startPosition =
                bagStartPoint.position;

            Vector3 endPosition =
                bagEndPoint.position;


            Quaternion startRotation =
                bagStartPoint.rotation;

            Quaternion endRotation =
                startRotation *
                Quaternion.Euler(
                    80f,
                    40f,
                    20f
                );


            float elapsed = 0f;


            while (
                elapsed <
                bagDropDuration
            )
            {
                elapsed +=
                    Time.deltaTime;


                float t =
                    Mathf.Clamp01(
                        elapsed /
                        bagDropDuration
                    );


                float smoothT =
                    Mathf.SmoothStep(
                        0f,
                        1f,
                        t
                    );


                disposalBagVisual
                    .transform
                    .position =
                    Vector3.Lerp(
                        startPosition,
                        endPosition,
                        smoothT
                    );


                disposalBagVisual
                    .transform
                    .rotation =
                    Quaternion.Slerp(
                        startRotation,
                        endRotation,
                        smoothT
                    );


                yield return null;
            }


            disposalBagVisual
                .transform
                .position =
                endPosition;


            yield return new WaitForSeconds(
                0.25f
            );


            // По QDD после завершения
            // мешок больше не отображается.
            disposalBagVisual.SetActive(
                false
            );
        }


        // ------------------------
        // COMPLETE QUEST
        // ------------------------

        if (QuestManagerV2.Instance != null)
        {
            QuestManagerV2.Instance.ProcessAction(
                questEndTargetID,
                GoalType.ReachPoint
            );
        }


        // Give QuestManager time to activate
        // Q_RETURN_HOME.
        yield return null;
        yield return null;


        SyncPlayerRotationWithCamera();


        // ------------------------
        // CONTROL BACK
        // ------------------------

        if (playerController != null)
        {
            playerController.isCinematic = false;
            playerController.SetCanMove(true);
        }


        Pause.canPause = true;
    }


    private IEnumerator PanCameraToTarget(
        Transform target,
        float duration)
    {
        if (playerController == null ||
            playerController.virtualCam == null ||
            target == null)
        {
            yield break;
        }


        Transform cam =
            playerController
            .virtualCam
            .transform;


        Quaternion startRotation =
            cam.rotation;


        Vector3 direction =
            target.position -
            cam.position;


        if (direction.sqrMagnitude <
            0.001f)
        {
            yield break;
        }


        Quaternion targetRotation =
            Quaternion.LookRotation(
                direction.normalized,
                Vector3.up
            );


        float elapsed = 0f;


        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;


            float t =
                Mathf.Clamp01(
                    elapsed /
                    duration
                );


            float smoothT =
                Mathf.SmoothStep(
                    0f,
                    1f,
                    t
                );


            cam.rotation =
                Quaternion.Slerp(
                    startRotation,
                    targetRotation,
                    smoothT
                );


            yield return null;
        }


        cam.rotation =
            targetRotation;
    }


    private void SyncPlayerRotationWithCamera()
    {
        if (playerController == null ||
            playerController.virtualCam == null)
        {
            return;
        }


        Vector3 euler =
            playerController
            .virtualCam
            .transform
            .eulerAngles;


        float yaw =
            euler.y;


        float pitch =
            euler.x;


        if (pitch > 180f)
        {
            pitch -= 360f;
        }


        playerController.SetRotation(
            yaw,
            pitch
        );
    }
}