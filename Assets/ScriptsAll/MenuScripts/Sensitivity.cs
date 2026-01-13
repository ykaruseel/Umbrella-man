using UnityEngine;
using UnityEngine.UI;

public class Sensitivity : MonoBehaviour
{
    private float minSens = 1f;

    private float maxSens = 3f;

    private float defaultSens = 2f;

    [SerializeField] private Slider sensSlider;

    [SerializeField] private PlayerController playerController;

    private const string Sens = "Sensitivity";

    private void Awake()
    {
        float savedSens = PlayerPrefs.GetFloat(Sens, defaultSens);

        if (playerController != null)
        {
            playerController.lookSpeed = Mathf.Clamp(savedSens, minSens, maxSens);
        }

        if (sensSlider != null)
        {
            sensSlider.minValue = minSens;
            sensSlider.maxValue = maxSens;
            sensSlider.value = savedSens;

            sensSlider.onValueChanged.AddListener(OnSliderChanged);
        }
    }

    public void OnSliderChanged(float value)
    {
        if (playerController != null)
            playerController.lookSpeed = Mathf.Clamp(value, minSens, maxSens);

        PlayerPrefs.SetFloat(Sens, value);
        PlayerPrefs.Save();
    }

    private void OnDestroy()
    {
        if (sensSlider != null)
            sensSlider.onValueChanged.RemoveListener(OnSliderChanged);
    }
}
