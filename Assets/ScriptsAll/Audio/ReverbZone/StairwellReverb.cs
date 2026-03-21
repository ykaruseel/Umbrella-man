using UnityEngine;
using FMODUnity;
using FMOD.Studio;

public class StairwellReverbTrigger : MonoBehaviour
{
    [SerializeField] private EventReference stairwellSnapshot;

    private EventInstance snapshotInstance;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            snapshotInstance = RuntimeManager.CreateInstance(stairwellSnapshot);
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