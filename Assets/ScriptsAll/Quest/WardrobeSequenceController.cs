using System.Collections;
using FMODUnity;
using UnityEngine;
using UnityEngine.Events;

public class WardrobeSequenceController : MonoBehaviour
{
    [Header("Quest")]
    [SerializeField]
    private string requiredQuestID =
        "Q_TAKE_OUT_TRASH_1";

    [SerializeField]
    private bool debugIgnoreQuest = false;


    [Header("Main References")]
    [SerializeField]
    private PlayerController playerController;

    [SerializeField]
    private WardrobeFearController fearController;

    [SerializeField]
    private Transform wardrobeCameraTarget;

    [SerializeField]
    private WardrobeAnimation wardrobeAnimation;


    [Header("Wardrobe Doors")]
    [SerializeField]
    private Transform leftDoor;

    [SerializeField]
    private Transform rightDoor;

    [SerializeField]
    private Vector3 leftDoorOpenEuler =
        new Vector3(0f, -105f, 0f);

    [SerializeField]
    private Vector3 rightDoorOpenEuler =
        new Vector3(0f, 105f, 0f);

    [SerializeField]
    private float doorOpenDuration = 0.15f;


    [Header("Rat")]
    [SerializeField]
    private GameObject rat;

    [SerializeField]
    private Transform[] ratPath;

    [SerializeField]
    private float ratSpeed = 4.5f;

    [SerializeField]
    private float ratTurnSpeed = 12f;


    [Header("Camera")]
    [SerializeField]
    private float cameraFollowSpeed = 8f;

    [SerializeField]
    private float cameraReturnDuration = 0.8f;


    [Header("Daniel Comment")]
    [SerializeField]
    private PlayerComments afterRatComment;


    [Header("Fade")]
    [SerializeField]
    private CameraFade cameraFade;


    [Header("FMOD")]
    [SerializeField]
    private EventReference wardrobeOpenSound;

    [SerializeField]
    private EventReference ratSound;


    [Header("Finish")]
    [SerializeField]
    private UnityEvent onSequenceFinished;


    [Header("Testing")]
    [SerializeField]
    private bool restoreControlAfterSequenceForTesting = true;

    [Header("Rat Jump Scare")]
    [SerializeField]
    private float ratJumpDuration = 0.35f;

    [SerializeField]
    private float ratJumpHeight = 0.8f;

    [SerializeField]
    private float ratCameraDistance = 0.7f;

    [SerializeField]
    private float ratCameraVerticalOffset = -0.35f;

    [Header("Interaction")]
    [SerializeField]
    private Collider wardrobeInteractionCollider;



    private Quaternion leftDoorClosedRotation;
    private Quaternion rightDoorClosedRotation;

    private bool sequenceStarted = false;

    private Quaternion leftDoorOpenRotation;
    private Quaternion rightDoorOpenRotation;


    private void Awake()
    {
        if (leftDoor != null)
        {
            leftDoorClosedRotation =
                leftDoor.localRotation;
        }

        if (rightDoor != null)
        {
            rightDoorClosedRotation =
                rightDoor.localRotation;
        }


        if (rat != null)
        {
            rat.SetActive(false);
        }

    }


    public bool CanInteract
    {
        get
        {
            if (sequenceStarted)
                return false;


            if (fearController != null &&
                fearController.InteractionStarted)
            {
                return false;
            }


            if (debugIgnoreQuest)
                return true;


            if (QuestManagerV2.Instance == null)
                return false;


            return QuestManagerV2.Instance.IsQuestActive(
                requiredQuestID
            );
        }
    }


    public bool TryInteract()
    {
        Debug.Log(
            $"[WardrobeSequence] TryInteract. " +
            $"CanInteract = {CanInteract}"
        );


        if (!CanInteract)
            return false;


        StartCoroutine(
            SequenceRoutine()
        );


        return true;
    }


    private IEnumerator SequenceRoutine()
    {
        sequenceStarted = true;

        if (wardrobeInteractionCollider != null)
        {
            wardrobeInteractionCollider.enabled = false;
        }

        if (wardrobeAnimation != null)
        {
            wardrobeAnimation.StopAllCoroutines();
            wardrobeAnimation.CancelInvoke();
            wardrobeAnimation.enabled = false;
        }


        // -------------------------
        // LOCK PLAYER
        // -------------------------

        Pause.canPause = false;


        if (playerController != null)
        {
            playerController.SetCanMove(false);
            playerController.isCinematic = true;
        }


        // -------------------------
        // STOP WARDROBE IDLE EVENT
        // -------------------------

        if (fearController != null)
        {
            fearController.BeginInteraction();

            // Scare becomes immediately stronger.
            fearController.SetCinematicFear(1f);
        }


        // -------------------------
        // OPEN DOORS
        // -------------------------

        if (!wardrobeOpenSound.IsNull)
        {
            RuntimeManager.PlayOneShotAttached(
                wardrobeOpenSound,
                gameObject
            );
        }


        yield return StartCoroutine(
            OpenDoors()
        );


        // -------------------------
        // RAT APPEARS
        // -------------------------

        if (rat != null &&
            ratPath != null &&
            ratPath.Length > 0 &&
            ratPath[0] != null)
        {
            rat.transform.position =
                ratPath[0].position;

            rat.transform.rotation =
                ratPath[0].rotation;

            rat.SetActive(true);
        }


        if (!ratSound.IsNull &&
            rat != null)
        {
            RuntimeManager.PlayOneShotAttached(
                ratSound,
                rat
            );
        }


        // -------------------------
        // RAT JUMP SCARE
        // -------------------------

        yield return StartCoroutine(
            RatJumpScare()
        );


        // -------------------------
        // RAT RUNS AWAY
        // CAMERA FOLLOWS
        // -------------------------

        yield return StartCoroutine(
            RunRatPathFromIndex(1)
        );



        if (rat != null)
        {
            rat.SetActive(false);
        }
        ForceDoorsOpen();


        // -------------------------
        // CAMERA BACK TO WARDROBE
        // -------------------------

        if (wardrobeCameraTarget != null)
        {
            yield return StartCoroutine(
                PanCameraToTarget(
                    wardrobeCameraTarget,
                    cameraReturnDuration
                )
            );
        }


        SyncPlayerRotationWithCamera();


        // -------------------------
        // FEAR RETURNS TO NORMAL
        // -------------------------

        yield return StartCoroutine(
            FadeFear(
                1f,
                0f,
                1.0f
            )
        );


        // -------------------------
        // DANIEL COMMENT
        // -------------------------

        if (afterRatComment != null)
        {
            afterRatComment.StartDialogue();


            while (
                afterRatComment.IsDialogueActive()
            )
            {
                yield return null;
            }
        }


        // -------------------------
        // FADE TO BLACK
        // -------------------------

        if (cameraFade != null)
        {
            yield return StartCoroutine(
                cameraFade.FadeOut()
            );
        }

        ForceDoorsOpen();
        // Hook for Return Home (Flashback)
        onSequenceFinished?.Invoke();


        // TEMPORARY:
        // until Return Home (Flashback)
        // is implemented.
        if (restoreControlAfterSequenceForTesting)
        {
            if (cameraFade != null)
            {
                yield return StartCoroutine(
                    cameraFade.FadeIn()
                );
            }


            if (playerController != null)
            {
                playerController.isCinematic = false;
                playerController.SetCanMove(true);
            }


            Pause.canPause = true;
        }
    }


    private IEnumerator OpenDoors()
    {
        if (leftDoor == null ||
            rightDoor == null)
        {
            yield break;
        }


        Quaternion leftStart =
            leftDoor.localRotation;

        Quaternion rightStart =
            rightDoor.localRotation;


        Quaternion leftTarget =
            leftDoorClosedRotation *
            Quaternion.Euler(
                leftDoorOpenEuler
            );


        Quaternion rightTarget =
            rightDoorClosedRotation *
            Quaternion.Euler(
                rightDoorOpenEuler
            );

        leftDoorOpenRotation = leftTarget;
        rightDoorOpenRotation = rightTarget;


        float elapsed = 0f;


        while (elapsed < doorOpenDuration)
        {
            elapsed += Time.deltaTime;


            float t =
                Mathf.Clamp01(
                    elapsed /
                    doorOpenDuration
                );


            // Quick aggressive opening.
            float smoothT =
                Mathf.SmoothStep(
                    0f,
                    1f,
                    t
                );


            leftDoor.localRotation =
                Quaternion.Slerp(
                    leftStart,
                    leftTarget,
                    smoothT
                );


            rightDoor.localRotation =
                Quaternion.Slerp(
                    rightStart,
                    rightTarget,
                    smoothT
                );


            yield return null;
        }


        leftDoor.localRotation =
            leftTarget;

        rightDoor.localRotation =
            rightTarget;
    }

    private void ForceDoorsOpen()
    {
        if (leftDoor != null)
        {
            leftDoor.localRotation =
                leftDoorOpenRotation;
        }

        if (rightDoor != null)
        {
            rightDoor.localRotation =
                rightDoorOpenRotation;
        }
    }


    private IEnumerator RunRatPathFromIndex(
    int startIndex)
{
    if (rat == null ||
        ratPath == null ||
        ratPath.Length <= startIndex)
    {
        yield break;
    }


    for (
        int i = startIndex;
        i < ratPath.Length;
        i++
    )
    {
        Transform target =
            ratPath[i];


        if (target == null)
            continue;


        while (
            Vector3.Distance(
                rat.transform.position,
                target.position
            ) > 0.05f
        )
        {
            Vector3 direction =
                target.position -
                rat.transform.position;


            direction.y = 0f;


            if (direction.sqrMagnitude >
                0.001f)
            {
                Quaternion targetRotation =
                    Quaternion.LookRotation(
                        direction.normalized,
                        Vector3.up
                    );


                rat.transform.rotation =
                    Quaternion.Slerp(
                        rat.transform.rotation,
                        targetRotation,
                        Time.deltaTime *
                        ratTurnSpeed
                    );
            }


            rat.transform.position =
                Vector3.MoveTowards(
                    rat.transform.position,
                    target.position,
                    ratSpeed *
                    Time.deltaTime
                );


            LookCameraAtRat();


            yield return null;
        }
    }
}

    private void LookCameraAtRat()
    {
        if (playerController == null ||
            playerController.virtualCam == null ||
            rat == null)
        {
            return;
        }


        Transform cam =
            playerController.virtualCam.transform;


        Vector3 direction =
            rat.transform.position -
            cam.position;


        if (direction.sqrMagnitude <
            0.001f)
        {
            return;
        }


        Quaternion targetRotation =
            Quaternion.LookRotation(
                direction.normalized,
                Vector3.up
            );


        cam.rotation =
            Quaternion.Slerp(
                cam.rotation,
                targetRotation,
                Time.deltaTime *
                cameraFollowSpeed
            );
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
            playerController.virtualCam.transform;


        Quaternion startRotation =
            cam.rotation;


        Vector3 direction =
            target.position -
            cam.position;


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


    private IEnumerator FadeFear(
        float from,
        float to,
        float duration)
    {
        if (fearController == null)
            yield break;


        float elapsed = 0f;


        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;


            float t =
                Mathf.Clamp01(
                    elapsed /
                    duration
                );


            fearController.SetCinematicFear(
                Mathf.Lerp(
                    from,
                    to,
                    t
                )
            );


            yield return null;
        }


        fearController.SetCinematicFear(
            to
        );
    }


    private void SyncPlayerRotationWithCamera()
    {
        if (playerController == null ||
            playerController.virtualCam == null)
        {
            return;
        }


        Vector3 cameraEuler =
            playerController
            .virtualCam
            .transform
            .eulerAngles;


        float yaw =
            cameraEuler.y;


        float pitch =
            cameraEuler.x;


        if (pitch > 180f)
        {
            pitch -= 360f;
        }


        playerController.SetRotation(
            yaw,
            pitch
        );
    }

    private IEnumerator RatJumpScare()
{
    if (rat == null ||
        playerController == null ||
        playerController.virtualCam == null)
    {
        yield break;
    }


    Transform cameraTransform =
        playerController.virtualCam.transform;


    Vector3 startPosition =
        rat.transform.position;


    Vector3 targetPosition =
        cameraTransform.position +
        cameraTransform.forward *
        ratCameraDistance +
        Vector3.up *
        ratCameraVerticalOffset;


    float elapsed = 0f;


    while (elapsed < ratJumpDuration)
    {
        elapsed += Time.deltaTime;

        float t =
            Mathf.Clamp01(
                elapsed /
                ratJumpDuration
            );


        Vector3 position =
            Vector3.Lerp(
                startPosition,
                targetPosition,
                t
            );


        float jump =
            4f *
            ratJumpHeight *
            t *
            (1f - t);


        position.y += jump;


        rat.transform.position =
            position;


        Vector3 direction =
            targetPosition -
            rat.transform.position;


        if (direction.sqrMagnitude > 0.001f)
        {
            rat.transform.rotation =
                Quaternion.LookRotation(
                    direction.normalized,
                    Vector3.up
                );
        }


        yield return null;
    }
}
}