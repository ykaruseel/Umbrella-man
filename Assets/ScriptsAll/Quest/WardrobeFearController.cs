using FMOD.Studio;
using FMODUnity;
using UnityEngine;
using UnityEngine.Rendering;

public class WardrobeFearController : MonoBehaviour
{
    [Header("Quest")]
    [SerializeField]
    private string requiredQuestID =
        "Q_TAKE_OUT_TRASH_1";

    [SerializeField]
    private bool debugIgnoreQuest = false;


    [Header("References")]
    [SerializeField]
    private Transform player;

    [SerializeField]
    private Transform wardrobeVisual;

    [SerializeField]
    private Volume fearVolume;


    [Header("Fear Distance")]
    [SerializeField]
    private float fearStartDistance = 9f;

    [SerializeField]
    private float fullFearDistance = 2.5f;

    [SerializeField]
    private float fearSmoothSpeed = 3f;

    [Range(0f, 1f)]
    [SerializeField]
    private float maxFearWeight = 1f;


    [Header("Wardrobe Rocking")]
    [SerializeField]
    private Vector3 rockingEuler =
        new Vector3(0f, 0f, 1.5f);

    [SerializeField]
    private float rockingSpeed = 3f;


    [Header("FMOD")]
    [SerializeField]
    private EventReference wardrobeNoiseEvent;


    private Quaternion baseVisualRotation;

    private EventInstance noiseInstance;

    private float currentFear = 0f;

    private bool interactionStarted = false;

    public bool InteractionStarted =>
        interactionStarted;


    private void Awake()
    {
        if (wardrobeVisual != null)
        {
            baseVisualRotation =
                wardrobeVisual.localRotation;
        }

        if (fearVolume != null)
        {
            fearVolume.weight = 0f;
        }
    }


    private void Start()
    {
        if (player == null)
        {
            PlayerController controller =
                FindFirstObjectByType<PlayerController>();

            if (controller != null)
            {
                player = controller.transform;
            }
        }
    }


    private void Update()
    {
        if (interactionStarted)
        {
            return;
        }


        bool active =
            IsWardrobeEventActive();


        if (!active)
        {
            ReturnToNormalState();
            return;
        }


        UpdateRocking();
        UpdateFear();
        EnsureNoisePlaying();
    }


    private bool IsWardrobeEventActive()
    {
        if (debugIgnoreQuest)
            return true;


        if (QuestManagerV2.Instance == null)
            return false;


        return QuestManagerV2.Instance.IsQuestActive(
            requiredQuestID
        );
    }


    public bool CanInteract()
    {
        return
            !interactionStarted &&
            IsWardrobeEventActive();
    }


    private void UpdateRocking()
    {
        if (wardrobeVisual == null)
            return;


        float wave =
            Mathf.Sin(
                Time.time * rockingSpeed
            );


        Vector3 rotationOffset =
            rockingEuler * wave;


        wardrobeVisual.localRotation =
            baseVisualRotation *
            Quaternion.Euler(
                rotationOffset
            );
    }


    private void UpdateFear()
    {
        if (player == null ||
            fearVolume == null)
        {
            return;
        }


        float distance =
            Vector3.Distance(
                player.position,
                transform.position
            );


        float targetFear = 0f;


        if (distance < fearStartDistance)
        {
            targetFear =
                Mathf.InverseLerp(
                    fearStartDistance,
                    fullFearDistance,
                    distance
                );
        }


        currentFear =
            Mathf.Lerp(
                currentFear,
                targetFear,
                Time.deltaTime *
                fearSmoothSpeed
            );


        fearVolume.weight =
            currentFear *
            maxFearWeight;
    }


    private void EnsureNoisePlaying()
    {
        if (wardrobeNoiseEvent.IsNull)
            return;


        if (noiseInstance.isValid())
            return;


        noiseInstance =
            RuntimeManager.CreateInstance(
                wardrobeNoiseEvent
            );


        RuntimeManager.AttachInstanceToGameObject(
            noiseInstance,
            transform,
            GetComponent<Rigidbody>()
        );


        noiseInstance.start();
    }


    private void StopNoise()
    {
        if (!noiseInstance.isValid())
            return;


        noiseInstance.stop(
            FMOD.Studio.STOP_MODE.ALLOWFADEOUT
        );

        noiseInstance.release();
        noiseInstance.clearHandle();
    }


    private void ReturnToNormalState()
    {
        StopNoise();


        currentFear =
            Mathf.Lerp(
                currentFear,
                0f,
                Time.deltaTime *
                fearSmoothSpeed
            );


        if (fearVolume != null)
        {
            fearVolume.weight =
                currentFear *
                maxFearWeight;
        }


        ReturnWardrobeToBaseRotation();
    }


    private void ReturnWardrobeToBaseRotation()
    {
        if (wardrobeVisual == null)
            return;


        wardrobeVisual.localRotation =
            Quaternion.Lerp(
                wardrobeVisual.localRotation,
                baseVisualRotation,
                Time.deltaTime * 6f
            );
    }


    public void BeginInteraction()
    {
        if (interactionStarted)
            return;

        interactionStarted = true;

        StopNoise();

        if (wardrobeVisual != null)
        {
            wardrobeVisual.localRotation =
                baseVisualRotation;
        }
    }


    public void SetCinematicFear(float value)
    {
        if (fearVolume == null)
            return;


        currentFear =
            Mathf.Clamp01(value);

        fearVolume.weight =
            currentFear *
            maxFearWeight;
    }


    private void OnDisable()
    {
        StopNoise();

        if (fearVolume != null)
        {
            fearVolume.weight = 0f;
        }

        if (wardrobeVisual != null)
        {
            wardrobeVisual.localRotation =
                baseVisualRotation;
        }
    }
}