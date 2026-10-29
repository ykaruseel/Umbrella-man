
using UnityEngine;
using FMODUnity;

public class FloorLamp : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private Light lampLight;
    [SerializeField] private Renderer bulbRenderer;
    [SerializeField] private EventReference clickSound;

    [Header("Interaction")]
    [SerializeField] private Camera playerCamera;
    [SerializeField] private float interactionDistance = 2f;
    [SerializeField] private LayerMask interactionLayers = ~0;

    private bool isOn = false;

    private void Start()
    {
        UpdateLampState();
    }

    private void Update()
    {
        if (playerCamera == null)
            return;

        if (Input.GetKeyDown(KeyCode.E) && IsLookingAtLamp())
        {
            ToggleLamp();
        }
    }

    private bool IsLookingAtLamp()
    {
        Ray ray = new Ray(
            playerCamera.transform.position,
            playerCamera.transform.forward
        );

        if (Physics.Raycast(
            ray,
            out RaycastHit hit,
            interactionDistance,
            interactionLayers,
            QueryTriggerInteraction.Ignore))
        {
            Debug.Log("Raycast hit: " + hit.transform.name);

            return hit.transform == transform ||
                   hit.transform.IsChildOf(transform);
        }

        Debug.Log("Raycast hit nothing");
        return false;
    }

    private void ToggleLamp()
    {
        isOn = !isOn;

        if (!clickSound.IsNull)
        {
            RuntimeManager.PlayOneShot(
                clickSound,
                transform.position
            );
        }

        UpdateLampState();
    }

    private void UpdateLampState()
    {
        if (lampLight != null)
            lampLight.enabled = isOn;

        if (bulbRenderer != null)
        {
            if (isOn)
                bulbRenderer.material.EnableKeyword("_EMISSION");
            else
                bulbRenderer.material.DisableKeyword("_EMISSION");
        }
    }
}
