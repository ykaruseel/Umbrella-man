using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.UI;

public class BrightnessManager : MonoBehaviour
{
    public Slider brightnessSlider;

    public Volume postProcessVolume;

    private ColorAdjustments colorAdjustments;
    private const string BrightnessVolume = "Brightness";

    private void Awake()
    {
        if (postProcessVolume == null)
        {
            return;
        }

        if (!postProcessVolume.profile.TryGet<ColorAdjustments>(out colorAdjustments))
        {
            return;
        }

        colorAdjustments.active = true;
        colorAdjustments.postExposure.overrideState = true;

        float savedBrightness = PlayerPrefs.GetFloat(BrightnessVolume, 0f);

        ApplyBrightness(savedBrightness);

        if (brightnessSlider != null)
        {
            brightnessSlider.minValue = -2f;
            brightnessSlider.maxValue = 2f;
            brightnessSlider.value = savedBrightness;
            brightnessSlider.onValueChanged.AddListener(OnSliderChanged);
        }
    }

    private void OnSliderChanged(float value)
    {
        ApplyBrightness(value);

        PlayerPrefs.SetFloat(BrightnessVolume, value);
        PlayerPrefs.Save();
    }

    private void ApplyBrightness(float value)
    {
        if (colorAdjustments != null)
            colorAdjustments.postExposure.value = value;
    }
}
