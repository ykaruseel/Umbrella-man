using FMOD.Studio;
using FMODUnity;
using UnityEngine;

public class SoundSettingsManager : MonoBehaviour
{
    public static SoundSettingsManager Instance;

    [SerializeField] private string masterVCAPath = "vca:/Master";
    [SerializeField] private string musicVCAPath = "vca:/Music";
    [SerializeField] private string sfxVCAPath = "vca:/SFX";

    private VCA masterVCA;
    private VCA musicVCA;
    private VCA sfxVCA;

    private const string MasterVolume = "MasterVolume";
    private const string MusicVolume = "MusicVolume";
    private const string SFXVolume = "SFXVolume";

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        masterVCA = RuntimeManager.GetVCA(masterVCAPath);
        musicVCA = RuntimeManager.GetVCA(musicVCAPath);
        sfxVCA = RuntimeManager.GetVCA(sfxVCAPath);

        LoadVolumeSettings();
    }

    private void LoadVolumeSettings()
    {
        float master = PlayerPrefs.GetFloat(MasterVolume, 0.5f);
        float music = PlayerPrefs.GetFloat(MusicVolume, 0.5f);
        float sfx = PlayerPrefs.GetFloat(SFXVolume, 0.5f);

        SetMasterVolume(master);
        SetMusicVolume(music);
        SetSFXVolume(sfx);
    }

    public void SetMasterVolume(float value)
    {
        value = Mathf.Clamp01(value);
        masterVCA.setVolume(value);
        PlayerPrefs.SetFloat(MasterVolume, value);
    }

    public void SetMusicVolume(float value)
    {
        value = Mathf.Clamp01(value);
        musicVCA.setVolume(value);
        PlayerPrefs.SetFloat(MusicVolume, value);
    }

    public void SetSFXVolume(float value)
    {
        value = Mathf.Clamp01(value);
        sfxVCA.setVolume(value);
        PlayerPrefs.SetFloat(SFXVolume, value);
    }

    public float GetMasterVolume() => PlayerPrefs.GetFloat(MasterVolume, 0.5f);
    public float GetMusicVolume() => PlayerPrefs.GetFloat(MusicVolume, 0.5f);
    public float GetSFXVolume() => PlayerPrefs.GetFloat(SFXVolume, 0.5f);
}
