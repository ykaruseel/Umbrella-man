
using UnityEngine;
using FMODUnity;
using FMOD.Studio;

public class QuestMusicController : MonoBehaviour
{
    [Header("FMOD Music")]
    [SerializeField] private EventReference musicEvent;
    [SerializeField] private string parameterName = "MusicSwitch";
    [SerializeField] private float introValue = 0f;
    [SerializeField] private float questValue = 1f;

    private EventInstance musicInstance;
    private bool permanentlyStopped;

    public void StartIntroMusic()
    {
        Debug.Log("[Tommy] StartIntroMusic called");

        if (permanentlyStopped)
        {
            Debug.LogWarning("[Tommy] Permanently stopped");
            return;
        }

        if (musicInstance.isValid())
        {
            Debug.Log("[Tommy] Instance already exists");
            return;
        }

        if (musicEvent.IsNull)
        {
            Debug.LogError("[Tommy] FMOD Event is not assigned");
            return;
        }

        musicInstance = RuntimeManager.CreateInstance(musicEvent);

        Debug.Log("[Tommy] Instance created: " + musicInstance.isValid());

        musicInstance.setParameterByName(parameterName, introValue);
        musicInstance.start();

        Debug.Log("[Tommy] Intro music started");
    }
    private void Awake()
    {
        DontDestroyOnLoad(gameObject);
    }
    public void StartQuestMusic()
    {
        if (permanentlyStopped)
            return;

        if (!musicInstance.isValid())
        {
            musicInstance = RuntimeManager.CreateInstance(musicEvent);
            musicInstance.setParameterByName(parameterName, questValue);
            musicInstance.start();

            Debug.Log("[Tommy] Started Quest section B");
            return;
        }

        musicInstance.setParameterByName(parameterName, questValue);
        Debug.Log("[Tommy] Switched to section B");
    }

    public void SetMusicState(float value)
    {
        Debug.Log("[Tommy] SetMusicState called: " + value);

        if (permanentlyStopped)
        {
            Debug.LogWarning("[Tommy] Music is permanently stopped");
            return;
        }

        if (!musicInstance.isValid())
        {
            Debug.LogError("[Tommy] Music instance is invalid");
            return;
        }

        FMOD.RESULT result = musicInstance.setParameterByName(parameterName, value);

        Debug.Log("[Tommy] Parameter result: " + result);
    }
    public void StartMusicAtState(float value)
    {
        if (permanentlyStopped)
            return;

        if (musicInstance.isValid())
        {
            musicInstance.setParameterByName(parameterName, value);
            return;
        }

        if (musicEvent.IsNull)
        {
            Debug.LogError("[Tommy] FMOD Event is not assigned");
            return;
        }

        musicInstance = RuntimeManager.CreateInstance(musicEvent);
        musicInstance.setParameterByName(parameterName, value);
        musicInstance.start();

        Debug.Log("[Tommy] Music restarted at section: " + value);
    }
    public void StopPermanently()
    {
        permanentlyStopped = true;
        StopMusic();
    }
    public void StopMusic()
    {
        Debug.Log("[Tommy] StopMusic called");
        if (!musicInstance.isValid())
            return;

        musicInstance.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
        musicInstance.release();
        musicInstance.clearHandle();
    }

    private void OnDestroy()
    {
        Debug.Log("[Tommy] QuestMusicController destroyed");
        if (!musicInstance.isValid())
            return;

        musicInstance.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
        musicInstance.release();
        musicInstance.clearHandle();
    }
}
