using UnityEngine;
using FMODUnity;
using FMOD.Studio;

public class PauseAudioSnapshot : MonoBehaviour
{
    public static PauseAudioSnapshot Instance;

    [SerializeField] private string pauseSnapshotPath = "snapshot:/Pause";

    private EventInstance snapshotInstance;

    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        snapshotInstance = RuntimeManager.CreateInstance(pauseSnapshotPath);
    }

    public void EnterPause()
    {
        if (snapshotInstance.isValid())
            snapshotInstance.start();
    }

    public void ExitPause()
    {
        if (snapshotInstance.isValid())
            snapshotInstance.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
    }

    private void OnDestroy()
    {
        if (snapshotInstance.isValid())
            snapshotInstance.release();
    }
}

