using UnityEngine;
using FMODUnity;
using FMOD.Studio;

public class NeighbourDoorAmbient : MonoBehaviour
{
    [SerializeField] private EventReference ambientEvent;

    [SerializeField] private string ambienceLabel = "Value A";

    private EventInstance instance;

    private void Start()
    {
        instance = RuntimeManager.CreateInstance(ambientEvent);

        RuntimeManager.AttachInstanceToGameObject(
            instance,
            transform
        );

        instance.setParameterByNameWithLabel(
            "NeighbourDoor",
            ambienceLabel
        );

        instance.start();
    }

    private void OnDestroy()
    {
        if (instance.isValid())
        {
            instance.stop(FMOD.Studio.STOP_MODE.IMMEDIATE);
            instance.release();
        }
    }
}