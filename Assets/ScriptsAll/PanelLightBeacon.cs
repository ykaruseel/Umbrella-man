using UnityEngine;

public class PanelLightBeacon : MonoBehaviour
{
    [Header("Настройки")]
    public Light myLight;
    public float minIntensity = 0.5f; 
    public float maxIntensity = 2.5f; 
    public float pulseSpeed = 2.0f;   

    private bool isActive = false;

    void Start()
    {
        
        if (myLight != null) myLight.enabled = false;
    }

    public void ActivateBeacon()
    {
        isActive = true;
        if (myLight != null) myLight.enabled = true;
    }

    void Update()
    {
        if (!isActive || myLight == null) return;
        
        float t = Mathf.PingPong(Time.time * pulseSpeed, 1f);
        myLight.intensity = Mathf.Lerp(minIntensity, maxIntensity, t);
    }
}
