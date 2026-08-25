using UnityEngine;
using FMODUnity;
using FMOD.Studio;

public class YardSnapshot : MonoBehaviour
{
    [SerializeField] private EventReference yardSnapshot;

    private EventInstance snapshotInstance;

    private void Start()
    {
        snapshotInstance = RuntimeManager.CreateInstance(yardSnapshot);
        snapshotInstance.start();
    }

    private void OnDestroy()
    {
        snapshotInstance.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
        snapshotInstance.release();
    }
}