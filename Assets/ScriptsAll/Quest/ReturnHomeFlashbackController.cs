using System.Collections;
using FMODUnity;
using UnityEngine;
using UnityEngine.Rendering;

public class ReturnHomeFlashbackController : MonoBehaviour
{
    [Header("Quest")]
    [SerializeField]
    private string flashbackQuestID =
        "Q_RETURN_HOME_FLASHBACK";

    [SerializeField]
    private string flashbackEndTargetID =
        "return_home_flashback_end";


    [Header("Player")]
    [SerializeField]
    private PlayerController playerController;

    [SerializeField]
    private Transform flashbackSpawn;


    [Header("Trash Bag")]
    [SerializeField]
    private TrashCarryVisual trashCarryVisual;


    [Header("Lighting")]
    [SerializeField]
    private GameObject[] presentLightingObjects;

    [SerializeField]
    private GameObject[] flashbackLightingObjects;


    [Header("Cement")]
    [SerializeField]
    private GameObject cementBag;

    [SerializeField]
    private Transform cementSpawn;

    [SerializeField]
    private Transform cementLookTarget;

    [SerializeField]
    private float cementFallDuration = 0.55f;

    [SerializeField]
    private Vector3 cementFallRotation =
        new Vector3(160f, 70f, 40f);


    [Header("Umbrella Man")]
    [SerializeField]
    private GameObject umbrellaMan;

    [SerializeField]
    private Transform umbrellaLookTarget;

    [SerializeField]
    private float umbrellaAppearLeadTime = 0.12f;


    [Header("Fear")]
    [SerializeField]
    private Volume flashbackFearVolume;

    [Range(0f, 1f)]
    [SerializeField]
    private float cementFear = 0.5f;

    [Range(0f, 1f)]
    [SerializeField]
    private float umbrellaFear = 1f;

    [SerializeField]
    private float fearFadeDuration = 0.6f;


    [Header("Camera")]
    [SerializeField]
    private float cementCameraDuration = 0.5f;

    [SerializeField]
    private float umbrellaCameraDuration = 1.2f;

    [SerializeField]
    private float emptyLandingHoldTime = 2.5f;


    [Header("Comments")]
    [SerializeField]
    private PlayerComments introComment;

    [SerializeField]
    private PlayerComments cementComment;

    [SerializeField]
    private PlayerComments umbrellaComment;


    [Header("Fade")]
    [SerializeField]
    private CameraFade cameraFade;

    [SerializeField]
    private float blinkFadeInDuration = 0.12f;

    [SerializeField]
    private float blinkBlackDuration = 0.12f;

    [SerializeField]
    private float blinkFadeOutDuration = 0.15f;


    [Header("FMOD")]
    [SerializeField]
    private EventReference cementImpactSound;


    [Header("Debug")]
    [SerializeField]
    private bool debugStartOnPlay = false;


    private bool flashbackActive = false;
    private bool cementTriggered = false;

    private Vector3 presentPlayerPosition;

    private float presentPlayerYaw;
    private float presentPlayerPitch;

    private bool hadTrashBag = false;


    public bool IsFlashbackActive =>
        flashbackActive;


    private void Awake()
    {
        if (flashbackFearVolume != null)
        {
            flashbackFearVolume.weight = 0f;
        }


        if (umbrellaMan != null)
        {
            umbrellaMan.SetActive(false);
        }


        if (cementBag != null)
        {
            cementBag.SetActive(false);
        }


        SetFlashbackLighting(false);
    }


    private IEnumerator Start()
    {
        if (!debugStartOnPlay)
            yield break;


        if (cameraFade != null)
        {
            cameraFade.SetFadeImageActive(true);
            cameraFade.SetFadeAlpha(1f);
        }


        yield return null;

        StartFlashback();
    }


    public void StartFlashback()
    {
        if (flashbackActive)
            return;


        StartCoroutine(
            StartFlashbackRoutine()
        );
    }


    private IEnumerator StartFlashbackRoutine()
    {
        flashbackActive = true;
        cementTriggered = false;


        // -----------------------------------
        // SAVE PRESENT STATE
        // -----------------------------------

        if (playerController != null)
        {
            presentPlayerPosition =
                playerController.transform.position;


            playerController.GetRotation(
                out presentPlayerYaw,
                out presentPlayerPitch
            );


            playerController.SetCanMove(false);
            playerController.isCinematic = true;
        }


        Pause.canPause = false;


        if (trashCarryVisual != null)
        {
            hadTrashBag =
                trashCarryVisual.HasBag;

            trashCarryVisual.HideFullBag();
        }


        // -----------------------------------
        // RESET FLASHBACK OBJECTS
        // -----------------------------------

        if (umbrellaMan != null)
        {
            umbrellaMan.SetActive(false);
        }


        if (cementBag != null)
        {
            cementBag.SetActive(false);
        }


        if (flashbackFearVolume != null)
        {
            flashbackFearVolume.weight = 0f;
        }


        // -----------------------------------
        // DAY
        // -----------------------------------

        SetFlashbackLighting(true);


        // -----------------------------------
        // MOVE PLAYER
        // -----------------------------------

        TeleportPlayer(
            flashbackSpawn
        );


        // -----------------------------------
        // FADE INTO MEMORY
        // -----------------------------------

        if (cameraFade != null)
        {
            cameraFade.SetFadeImageActive(true);
            cameraFade.SetFadeAlpha(1f);


            yield return StartCoroutine(
                cameraFade.FadeIn()
            );
        }


        // -----------------------------------
        // INTRO COMMENT
        // -----------------------------------

        if (introComment != null)
        {
            introComment.StartDialogue();


            while (
                introComment.IsDialogueActive()
            )
            {
                yield return null;
            }
        }


        // -----------------------------------
        // START QUEST
        // -----------------------------------

        if (QuestManagerV2.Instance != null)
        {
            QuestManagerV2.Instance.ActivateQuestByID(
                flashbackQuestID,
                true
            );
        }


        // -----------------------------------
        // RETURN CONTROL
        // -----------------------------------

        if (playerController != null)
        {
            playerController.isCinematic = false;
            playerController.SetCanMove(true);
        }


        Pause.canPause = true;
    }


    public bool TryStartCementSequence()
    {
        if (!flashbackActive)
            return false;


        if (cementTriggered)
            return false;


        cementTriggered = true;


        StartCoroutine(
            CementSequenceRoutine()
        );


        return true;
    }


    private IEnumerator CementSequenceRoutine()
    {
        // -----------------------------------
        // LOCK PLAYER
        // -----------------------------------

        Pause.canPause = false;


        if (playerController != null)
        {
            playerController.SetCanMove(false);
            playerController.isCinematic = true;
        }


        // -----------------------------------
        // CEMENT FALL
        // -----------------------------------

        yield return StartCoroutine(
            DropCement()
        );


        if (!cementImpactSound.IsNull &&
            cementBag != null)
        {
            RuntimeManager.PlayOneShotAttached(
                cementImpactSound,
                cementBag
            );
        }


        // -----------------------------------
        // CAMERA LOOKS AT CEMENT
        // -----------------------------------

        if (cementLookTarget != null)
        {
            yield return StartCoroutine(
                PanCameraToTarget(
                    cementLookTarget,
                    cementCameraDuration
                )
            );
        }


        // -----------------------------------
        // FIRST FEAR
        // -----------------------------------

        StartCoroutine(
            FadeFear(
                flashbackFearVolume != null
                    ? flashbackFearVolume.weight
                    : 0f,
                cementFear,
                fearFadeDuration
            )
        );


        // -----------------------------------
        // COMMENT ABOUT CEMENT
        // -----------------------------------

        if (cementComment != null)
        {
            cementComment.StartDialogue();


            while (
                cementComment.IsDialogueActive()
            )
            {
                yield return null;
            }
        }


        // -----------------------------------
        // UMBRELLA MAN APPEARS
        // BEFORE CAMERA LOOKS UP
        // -----------------------------------

        if (umbrellaMan != null)
        {
            umbrellaMan.SetActive(true);
        }


        if (umbrellaAppearLeadTime > 0f)
        {
            yield return new WaitForSeconds(
                umbrellaAppearLeadTime
            );
        }


        // -----------------------------------
        // CAMERA LOOKS UP
        // -----------------------------------

        StartCoroutine(
            FadeFear(
                cementFear,
                umbrellaFear,
                umbrellaCameraDuration
            )
        );


        if (umbrellaLookTarget != null)
        {
            yield return StartCoroutine(
                PanCameraToTarget(
                    umbrellaLookTarget,
                    umbrellaCameraDuration
                )
            );
        }


        // -----------------------------------
        // UMBRELLA MAN MONOLOGUE
        // -----------------------------------

        if (umbrellaComment != null)
        {
            umbrellaComment.StartDialogue();


            while (
                umbrellaComment.IsDialogueActive()
            )
            {
                yield return null;
            }
        }


        // -----------------------------------
        // BLINK
        // UMBRELLA MAN DISAPPEARS IN BLACK
        // -----------------------------------

        yield return StartCoroutine(
            BlinkAndRemoveUmbrellaMan()
        );


        // -----------------------------------
        // LOOK AT EMPTY FIRE ESCAPE
        // -----------------------------------

        yield return new WaitForSeconds(
            emptyLandingHoldTime
        );


        // -----------------------------------
        // FINAL FADE
        // -----------------------------------

        if (cameraFade != null)
        {
            yield return StartCoroutine(
                cameraFade.FadeOut()
            );
        }


        // -----------------------------------
        // RETURN TO PRESENT
        // -----------------------------------

        RestorePresentState();


        // -----------------------------------
        // COMPLETE FLASHBACK QUEST
        // → Q_TAKE_OUT_TRASH_2
        // -----------------------------------

        if (QuestManagerV2.Instance != null)
        {
            QuestManagerV2.Instance.ProcessAction(
                flashbackEndTargetID,
                GoalType.ReachPoint
            );
        }


        // QuestManagerV2 advances on next frame.
        yield return null;
        yield return null;


        // -----------------------------------
        // FADE BACK INTO PRESENT
        // -----------------------------------

        if (cameraFade != null)
        {
            yield return StartCoroutine(
                cameraFade.FadeIn()
            );
        }


        // -----------------------------------
        // CONTROL BACK
        // -----------------------------------

        flashbackActive = false;


        if (playerController != null)
        {
            playerController.isCinematic = false;
            playerController.SetCanMove(true);
        }


        Pause.canPause = true;
    }


    private IEnumerator DropCement()
    {
        if (cementBag == null ||
            cementSpawn == null ||
            cementLookTarget == null)
        {
            yield break;
        }


        cementBag.transform.position =
            cementSpawn.position;

        cementBag.transform.rotation =
            cementSpawn.rotation;

        cementBag.SetActive(true);


        Vector3 startPosition =
            cementSpawn.position;

        Vector3 endPosition =
            cementLookTarget.position;


        Quaternion startRotation =
            cementSpawn.rotation;

        Quaternion endRotation =
            startRotation *
            Quaternion.Euler(
                cementFallRotation
            );


        float elapsed = 0f;


        while (elapsed < cementFallDuration)
        {
            elapsed += Time.deltaTime;


            float t =
                Mathf.Clamp01(
                    elapsed /
                    cementFallDuration
                );


            // Accelerates toward the ground.
            float fallT =
                t * t;


            cementBag.transform.position =
                Vector3.Lerp(
                    startPosition,
                    endPosition,
                    fallT
                );


            cementBag.transform.rotation =
                Quaternion.Slerp(
                    startRotation,
                    endRotation,
                    t
                );


            yield return null;
        }


        cementBag.transform.position =
            endPosition;

        cementBag.transform.rotation =
            endRotation;
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


    private IEnumerator FadeFear(
        float from,
        float to,
        float duration)
    {
        if (flashbackFearVolume == null)
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


            flashbackFearVolume.weight =
                Mathf.Lerp(
                    from,
                    to,
                    t
                );


            yield return null;
        }


        flashbackFearVolume.weight =
            to;
    }


    private IEnumerator BlinkAndRemoveUmbrellaMan()
    {
        if (cameraFade == null)
        {
            if (umbrellaMan != null)
            {
                umbrellaMan.SetActive(false);
            }

            yield break;
        }


        cameraFade.SetFadeImageActive(true);


        // Quick close of Daniel's eyes.
        yield return StartCoroutine(
            FadeBlackAlpha(
                0f,
                1f,
                blinkFadeInDuration
            )
        );


        if (umbrellaMan != null)
        {
            umbrellaMan.SetActive(false);
        }


        yield return new WaitForSeconds(
            blinkBlackDuration
        );


        // Eyes open again.
        yield return StartCoroutine(
            FadeBlackAlpha(
                1f,
                0f,
                blinkFadeOutDuration
            )
        );


        cameraFade.SetFadeImageActive(false);
    }


    private IEnumerator FadeBlackAlpha(
        float from,
        float to,
        float duration)
    {
        if (cameraFade == null)
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


            cameraFade.SetFadeAlpha(
                Mathf.Lerp(
                    from,
                    to,
                    t
                )
            );


            yield return null;
        }


        cameraFade.SetFadeAlpha(to);
    }


    private void RestorePresentState()
    {
        SetFlashbackLighting(false);


        if (umbrellaMan != null)
        {
            umbrellaMan.SetActive(false);
        }


        if (cementBag != null)
        {
            cementBag.SetActive(false);
        }


        if (flashbackFearVolume != null)
        {
            flashbackFearVolume.weight = 0f;
        }


        if (trashCarryVisual != null)
        {
            if (hadTrashBag)
            {
                trashCarryVisual.ShowFullBag();
            }
            else
            {
                trashCarryVisual.HideFullBag();
            }
        }


        TeleportPlayerToPresentPosition();
    }


    private void TeleportPlayer(
        Transform point)
    {
        if (playerController == null ||
            point == null)
        {
            return;
        }


        CharacterController controller =
            playerController.GetComponent<CharacterController>();


        if (controller != null)
        {
            controller.enabled = false;
        }


        playerController.transform.position =
            point.position;


        playerController.SetRotation(
            point.eulerAngles.y,
            0f
        );


        if (controller != null)
        {
            controller.enabled = true;
        }
    }


    private void TeleportPlayerToPresentPosition()
    {
        if (playerController == null)
            return;


        CharacterController controller =
            playerController.GetComponent<CharacterController>();


        if (controller != null)
        {
            controller.enabled = false;
        }


        playerController.transform.position =
            presentPlayerPosition;


        playerController.SetRotation(
            presentPlayerYaw,
            presentPlayerPitch
        );


        if (controller != null)
        {
            controller.enabled = true;
        }
    }


    private void SetFlashbackLighting(
        bool flashback)
    {
        if (presentLightingObjects != null)
        {
            foreach (
                GameObject obj
                in presentLightingObjects)
            {
                if (obj != null)
                {
                    obj.SetActive(
                        !flashback
                    );
                }
            }
        }


        if (flashbackLightingObjects != null)
        {
            foreach (
                GameObject obj
                in flashbackLightingObjects)
            {
                if (obj != null)
                {
                    obj.SetActive(
                        flashback
                    );
                }
            }
        }
    }
}