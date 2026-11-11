
using UnityEngine;
using FMODUnity;
using FMOD.Studio;

public class QuestMusicController : MonoBehaviour
{
    [Header("FMOD Music")]
    [SerializeField] private EventReference musicEvent;
    [SerializeField] private string parameterName = "MusicSwitch";
    [SerializeField] private float parameterValue = 1f;

    private EventInstance musicInstance;

    public void StartMusic()
    {
        if (musicInstance.isValid())
            return;

        musicInstance = RuntimeManager.CreateInstance(musicEvent);
        musicInstance.setParameterByName(parameterName, parameterValue);
        musicInstance.start();
    }

    public void StopMusic()
    {
        if (!musicInstance.isValid())
            return;

        musicInstance.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
        musicInstance.release();
        musicInstance.clearHandle();
    }

    private void OnDestroy()
    {
        if (!musicInstance.isValid())
            return;

        musicInstance.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
        musicInstance.release();
        musicInstance.clearHandle();
    }
}
