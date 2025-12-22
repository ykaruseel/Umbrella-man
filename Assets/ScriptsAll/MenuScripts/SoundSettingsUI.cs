using UnityEngine;
using UnityEngine.UI;

public class SoundSettingsUI : MonoBehaviour
{
    [SerializeField] private Slider masterSlider;
    [SerializeField] private Slider musicSlider;
    [SerializeField] private Slider sfxSlider;

    private void Start()
    {
        masterSlider.value = SoundSettingsManager.Instance.GetMasterVolume();
        musicSlider.value = SoundSettingsManager.Instance.GetMusicVolume();
        sfxSlider.value = SoundSettingsManager.Instance.GetSFXVolume();

        masterSlider.onValueChanged.AddListener(SoundSettingsManager.Instance.SetMasterVolume);
        musicSlider.onValueChanged.AddListener(SoundSettingsManager.Instance.SetMusicVolume);
        sfxSlider.onValueChanged.AddListener(SoundSettingsManager.Instance.SetSFXVolume);
    }
}
