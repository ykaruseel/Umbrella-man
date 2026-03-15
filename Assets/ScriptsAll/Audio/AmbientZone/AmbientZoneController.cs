using UnityEngine;
using FMODUnity;
using FMOD.Studio;
using System.Collections;

public class AmbientController : MonoBehaviour
{
    [SerializeField] private EventReference ambientEvent;

    private EventInstance ambientInstance;
    private Coroutine transition;

    private void Start()
    {
        ambientInstance = RuntimeManager.CreateInstance(ambientEvent);
        ambientInstance.start();
    }

    public void SetApartment()
    {
        StartTransition(0f);
    }

    public void SetStairwell()
    {
        StartTransition(1f);
    }

    public void SetBasement()
    {
        StartTransition(2f);
    }

    void StartTransition(float target)
    {
        if (transition != null) StopCoroutine(transition);
        transition = StartCoroutine(ChangeArea(target));
    }

    IEnumerator ChangeArea(float target)
    {
        ambientInstance.getParameterByName("Area", out float current);

        float time = 0f;
        float duration = 2f;

        while (time < duration)
        {
            time += Time.deltaTime;
            float value = Mathf.Lerp(current, target, time / duration);
            ambientInstance.setParameterByName("Area", value);
            yield return null;
        }

        ambientInstance.setParameterByName("Area", target);
    }

    private void OnDestroy()
    {
        ambientInstance.release();
    }
}