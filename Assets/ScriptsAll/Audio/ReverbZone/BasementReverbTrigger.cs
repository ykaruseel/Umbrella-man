using UnityEngine;
using FMODUnity;
using FMOD.Studio;

public class BasementReverbTrigger : MonoBehaviour
{
    [SerializeField] private EventReference basementSnapshot;

    private EventInstance snapshotInstance;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            snapshotInstance = RuntimeManager.CreateInstance(basementSnapshot);
            snapshotInstance.start();
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            snapshotInstance.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
            snapshotInstance.release();
        }
    }
}