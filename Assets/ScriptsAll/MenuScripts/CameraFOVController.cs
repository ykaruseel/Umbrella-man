using UnityEngine;
using UnityEngine.UI;

public class CameraFOVController : MonoBehaviour
{
    [SerializeField] private float minFOV = 30f;

    [SerializeField] private float maxFOV = 80f;

    [SerializeField] private float defaultFOV = 50f;

    [SerializeField] private Slider fovSlider;

    [SerializeField] private PlayerController playerController;

    private const string FOV = "CameraFOV";

    private void Awake()
    {
        float savedFOV = PlayerPrefs.GetFloat(FOV, defaultFOV);

        if (playerController != null)
        {
            playerController.virtualCam.Lens.FieldOfView = Mathf.Clamp(savedFOV, minFOV, maxFOV);
        }

        Debug.Log(savedFOV);

        if (fovSlider != null)
        {
            fovSlider.minValue = minFOV;
            fovSlider.maxValue = maxFOV;
            fovSlider.value = savedFOV;

            fovSlider.onValueChanged.AddListener(OnSliderChanged);
        }
    }

    public void OnSliderChanged(float value)
    {
        if(playerController != null)
            playerController.virtualCam.Lens.FieldOfView = Mathf.Clamp(value, minFOV, maxFOV);

        PlayerPrefs.SetFloat(FOV, value);
        PlayerPrefs.Save();
    }

    private void OnDestroy()
    {
        if (fovSlider != null)
            fovSlider.onValueChanged.RemoveListener(OnSliderChanged);
    }
}
