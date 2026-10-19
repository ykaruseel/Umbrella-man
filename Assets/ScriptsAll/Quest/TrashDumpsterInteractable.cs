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
    private Transform bagOverRimPoint;

    [Header("Throw Arc")]
    [SerializeField, Min(0f)]
    private float throwArcHeight = 0.6f;

    [SerializeField, Range(0.2f, 0.9f)]
    private float throwDurationRatio = 0.65f;

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
            bagOverRimPoint != null &&
            bagEndPoint != null)
        {
            Vector3 start = bagStartPoint.position;
            Vector3 overRim = bagOverRimPoint.position;
            Vector3 end = bagEndPoint.position;

            Quaternion startRotation = bagStartPoint.rotation;
            Quaternion endRotation =
                startRotation * Quaternion.Euler(80f, 40f, 20f);

            Transform bag = disposalBagVisual.transform;

            bag.SetPositionAndRotation(start, startRotation);
            disposalBagVisual.SetActive(true);

            float totalDuration = Mathf.Max(0.1f, bagDropDuration);

            float throwDuration =
                totalDuration * throwDurationRatio;

            float fallDuration =
                totalDuration - throwDuration;

            // PHASE 1: Throw upward, over the container rim.
            float elapsed = 0f;

            while (elapsed < throwDuration)
            {
                elapsed += Time.deltaTime;

                float t = Mathf.Clamp01(
                    elapsed / throwDuration
                );

                float smoothT = Mathf.SmoothStep(0f, 1f, t);

                Vector3 position =
                    Vector3.Lerp(start, overRim, smoothT);

                // Vertical arc above the base trajectory.
                position.y +=
                    Mathf.Sin(Mathf.PI * t) * throwArcHeight;

                bag.position = position;

                bag.rotation = Quaternion.Slerp(
                    startRotation,
                    endRotation,
                    t * 0.6f
                );

                yield return null;
            }

            bag.position = overRim;

            // PHASE 2: Drop vertically into the container.
            elapsed = 0f;

            Quaternion fallStartRotation = bag.rotation;

            while (elapsed < fallDuration)
            {
                elapsed += Time.deltaTime;

                float t = Mathf.Clamp01(
                    elapsed / fallDuration
                );

                // Accelerating fall.
                float fallT = t * t;

                bag.position = Vector3.Lerp(
                    overRim,
                    end,
                    fallT
                );

                bag.rotation = Quaternion.Slerp(
                    fallStartRotation,
                    endRotation,
                    t
                );

                yield return null;
            }

            bag.position = end;
            bag.rotation = endRotation;

            yield return new WaitForSeconds(0.25f);

            disposalBagVisual.SetActive(false);
        }
        else
        {
            Debug.LogWarning(
                "[TrashDumpster] Throw references are missing."
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