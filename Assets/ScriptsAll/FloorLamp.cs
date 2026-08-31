using UnityEngine;
using FMODUnity; 

public class FloorLamp : MonoBehaviour
{
    [Header("Settings")]
    [Tooltip("Light")]
    public Light lampLight; 

    [Tooltip("(MeshRenderer)")]
    public Renderer bulbRenderer; 

    [Tooltip("FMOD Event")]
    public EventReference clickSound; 

    private bool isPlayerNear = false; 
    private bool isOn = false;         

    void Start()
    {
        
        UpdateLampState();
    }

    void Update()
    {
        
        if (isPlayerNear && Input.GetKeyDown(KeyCode.E))
        {
            ToggleLamp();
        }
    }

    void ToggleLamp()
    {
        isOn = !isOn; 
        
        
        if (!clickSound.IsNull) 
        {
            RuntimeManager.PlayOneShot(clickSound, transform.position);
        }
        
        UpdateLampState();
    }

    void UpdateLampState()
    {
        
        if (lampLight != null) lampLight.enabled = isOn;

        
        if (bulbRenderer != null)
        {
            if (isOn) bulbRenderer.material.EnableKeyword("_EMISSION");
            else bulbRenderer.material.DisableKeyword("_EMISSION");
        }
    }

    
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player")) isPlayerNear = true;
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player")) isPlayerNear = false;
    }
}
