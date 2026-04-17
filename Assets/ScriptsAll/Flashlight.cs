using FMODUnity;
using UnityEngine;

public class Flashlight : MonoBehaviour
{
    public string ID;

    [Header("Основные компоненты")]
    [SerializeField] private Light lightSource;
    [SerializeField] private GameObject flashLight;
    [SerializeField] private Transform triggerHandle;

    [Range(0, 1)][SerializeField] private float currentEnergy = 0f;
    [SerializeField] private float maxIntensity = 5f;
    [SerializeField] private float maxRange = 25f;
    [SerializeField] private float decayRate = 0.15f;
    [SerializeField] private float chargePerClick = 0.12f;
    [SerializeField] private float chargePerHold = 0.2f;
    [SerializeField] private float smoothness = 4f;

    [Header("FMOD")]
    [SerializeField] private EventReference handleEvent;
    [SerializeField] private EventReference reelEvent;

    private FMOD.Studio.EventInstance reelInstance;

    [SerializeField] private Vector3 handleIdlePos;
    [SerializeField] private Vector3 handlePressedPos;

    private bool isEquipped = false;
    private bool isBlocked = false;

    private void OnEnable()
    {
        QuestManagerV2.Instance.ProcessAction(ID, GoalType.ReturnItem);

        flashLight.SetActive(true);
        isEquipped = true;
    }

    private void Start()
    {
        reelInstance = RuntimeManager.CreateInstance(reelEvent);
        reelInstance.start();

        RuntimeManager.AttachInstanceToGameObject(
            reelInstance,
            flashLight.transform,
            flashLight.GetComponent<Rigidbody>()
        );

        if (lightSource != null)
        {
            lightSource.intensity = 0;
            lightSource.type = LightType.Spot;
        }
    }

    private void Update()
    {
        if (Time.timeScale == 0 || isBlocked) return;

        HandleInput();
        CalculateEnergy();
        ApplyVisuals();
        UpdateSound();
    }

    private void HandleInput()
    {
        if (Input.GetKeyDown(KeyCode.F))
        {
            isEquipped = !isEquipped;
            flashLight.SetActive(!flashLight.activeSelf);
        }

        if (!isEquipped) return;

        if (Input.GetMouseButton(0))
        {
            currentEnergy += chargePerHold * Time.deltaTime;

            triggerHandle.localPosition = Vector3.Lerp(
                triggerHandle.localPosition,
                handlePressedPos,
                Time.deltaTime * 25f
            );
        }
        else
        {
            triggerHandle.localPosition = Vector3.Lerp(
                triggerHandle.localPosition,
                handleIdlePos,
                Time.deltaTime * 12f
            );
        }

        if (Input.GetMouseButtonDown(0))
        {
            currentEnergy += chargePerClick;

            RuntimeManager.PlayOneShot(handleEvent, flashLight.transform.position);
        }
    }

    private void UpdateSound()
    {
        float target = isEquipped ? currentEnergy : 0f;
        reelInstance.setParameterByName("Intensity", target);
    }

    private void CalculateEnergy()
    {
        if (currentEnergy > 0)
        {
            currentEnergy -= decayRate * Time.deltaTime;
        }

        currentEnergy = Mathf.Clamp01(currentEnergy);
    }

    private void ApplyVisuals()
    {
        float targetIntensity = currentEnergy * maxIntensity;
        float targetRange = currentEnergy * maxRange;

        lightSource.intensity = Mathf.Lerp(
            lightSource.intensity,
            targetIntensity,
            Time.deltaTime * smoothness
        );

        lightSource.range = Mathf.Lerp(
            lightSource.range,
            targetRange,
            Time.deltaTime * smoothness
        );
    }

    public void SetBlocked(bool state)
    {
        isBlocked = state;
        if (isBlocked) isEquipped = false;
    }

    public void ResetState()
    {
        currentEnergy = 0;
        isEquipped = false;
        lightSource.intensity = 0;
    }

    private void OnDisable()
    {
        reelInstance.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
        reelInstance.release();
    }
}
