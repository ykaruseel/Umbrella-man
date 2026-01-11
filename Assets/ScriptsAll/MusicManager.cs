using UnityEngine;
using System.Collections;
using FMODUnity;
using FMOD.Studio;

public class MusicManager : MonoBehaviour
{
    public static MusicManager Instance;
    public EventReference musicEvent;
    public string sectionParameterName = "MusicSwitch";
    public float defaultVolume = 1f;

    private EventInstance musicInstance;
    private float currentVolume = 1f;
    private Coroutine fadeCoroutine;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        //DontDestroyOnLoad(gameObject);

        if (!musicEvent.IsNull)
        {
            musicInstance = RuntimeManager.CreateInstance(musicEvent);
            musicInstance.start();
            currentVolume = defaultVolume;
            SetVolumeImmediate(currentVolume);
            SetSection("Value A");
        }
        else
        {
            Debug.LogWarning("[MusicManager] musicEvent is not assigned.");
        }
    }

    public void SetSection(string section)
    {
        if (!musicInstance.isValid()) return;
        musicInstance.setParameterByNameWithLabel(sectionParameterName, section);
    }

    public void EnsureMusicPlaying()
    {
        if (!musicInstance.isValid())
            return;

        PLAYBACK_STATE state;
        musicInstance.getPlaybackState(out state);

        if (state != PLAYBACK_STATE.PLAYING)
            musicInstance.start();
    }

    public void FadeToVolume(float targetVolume, float duration)
    {
        if (!musicInstance.isValid()) return;
        if (fadeCoroutine != null) StopCoroutine(fadeCoroutine);
        fadeCoroutine = StartCoroutine(FadeVolumeRoutine(targetVolume, duration));
    }

    IEnumerator FadeVolumeRoutine(float target, float duration)
    {
        float start = currentVolume;
        float t = 0f;
        if (duration <= 0f)
        {
            SetVolumeImmediate(target);
            yield break;
        }
        while (t < duration)
        {
            t += Time.deltaTime;
            currentVolume = Mathf.Lerp(start, target, Mathf.Clamp01(t / duration));
            musicInstance.setVolume(currentVolume);
            yield return null;
        }
        currentVolume = target;
        musicInstance.setVolume(currentVolume);
        fadeCoroutine = null;
    }

    public void SetVolumeImmediate(float v)
    {
        if (!musicInstance.isValid()) return;
        currentVolume = v;
        musicInstance.setVolume(currentVolume);
    }

    public void StopMusicAllowFade()
    {
        if (!musicInstance.isValid()) return;
        musicInstance.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
        musicInstance.release();
    }

    public void StopMusicImmediate()
    {
        if (!musicInstance.isValid()) return;
        musicInstance.stop(FMOD.Studio.STOP_MODE.IMMEDIATE);
        musicInstance.release();
    }
    public void RestartMusic()
    {
        if (musicInstance.isValid())
        {
            musicInstance.stop(FMOD.Studio.STOP_MODE.IMMEDIATE);
            musicInstance.release();
        }

        musicInstance = RuntimeManager.CreateInstance(musicEvent);
        musicInstance.start();
    }

    void OnDestroy()
    {
        if (musicInstance.isValid())
        {
            musicInstance.stop(FMOD.Studio.STOP_MODE.IMMEDIATE);
            musicInstance.release();
        }
    }
}