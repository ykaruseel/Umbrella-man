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
        musicInstance = RuntimeManager.CreateInstance(musicEvent);
        musicInstance.start();
    }

    public void SetMusicState(int value)
    {
        musicInstance.setParameterByName("MusicSwitch", value);
    }
    public void StopMusic()
    {
        musicInstance.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
    }
}