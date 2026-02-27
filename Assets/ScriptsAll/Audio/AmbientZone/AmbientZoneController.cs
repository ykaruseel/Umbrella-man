using UnityEngine;
using FMODUnity;
using FMOD.Studio;

public class AmbientZoneController : MonoBehaviour
{
    [SerializeField] private EventReference apartmentAmbient;
    [SerializeField] private EventReference stairwellAmbient;

    private EventInstance apartmentInstance;
    private EventInstance stairwellInstance;

    private void Start()
    {
        apartmentInstance = RuntimeManager.CreateInstance(apartmentAmbient);
        stairwellInstance = RuntimeManager.CreateInstance(stairwellAmbient);

        apartmentInstance.start();
    }

    public void EnterApartment()
    {
        stairwellInstance.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
        apartmentInstance.start();
    }

    public void EnterStairwell()
    {
        apartmentInstance.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
        stairwellInstance.start();
    }

    private void OnDestroy()
    {
        apartmentInstance.release();
        stairwellInstance.release();
    }
}