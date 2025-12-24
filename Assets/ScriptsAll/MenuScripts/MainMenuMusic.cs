using FMOD.Studio;
using FMODUnity;
using UnityEngine;

public class MainMenuMusic : MonoBehaviour
{
    public EventReference musicEvent;
    private EventInstance musicInstance;
    private void Start()
    {
        musicInstance = RuntimeManager.CreateInstance(musicEvent);
        musicInstance.start();
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
