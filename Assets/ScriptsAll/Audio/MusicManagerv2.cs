using UnityEngine;
using FMODUnity;
using FMOD.Studio;

public class MusicManagerv2 : MonoBehaviour
{
    public static MusicManagerv2 Instance;

    [SerializeField] private EventReference musicEvent;

    private EventInstance musicInstance;

    private void Awake()
    {
        Instance = this;
    }

    public void StartMusic()
    {
        if (musicInstance.isValid())
            return;

        musicInstance = RuntimeManager.CreateInstance(musicEvent);
        musicInstance.start();
    }

    public void SetMusicState(int value)
    {
        if (musicInstance.isValid())
        {
            musicInstance.setParameterByName("MusicSwitch", value);
        }
    }

    public void StopMusic()
    {
        if (musicInstance.isValid())
        {
            musicInstance.stop(FMOD.Studio.STOP_MODE.IMMEDIATE);
            musicInstance.release();
            musicInstance.clearHandle();
        }
    }

    private void OnDestroy()
    {
        if (musicInstance.isValid())
        {
            musicInstance.stop(FMOD.Studio.STOP_MODE.IMMEDIATE);
            musicInstance.release();
        }
    }
}