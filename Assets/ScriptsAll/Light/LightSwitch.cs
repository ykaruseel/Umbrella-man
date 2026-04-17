using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FMODUnity;

public class LightSwitch : MonoBehaviour
{
    public List<GameObject> lights;

    [SerializeField] private Animator animator;

    [Header("FMOD")]
    [SerializeField] private EventReference switchSound;

    private bool isAnimating = false;

    public void Interact()
    {
        if (isAnimating) return;
        StartCoroutine(ToggleLights());
    }

    public IEnumerator ToggleLights()
    {
        isAnimating = true;

        RuntimeManager.PlayOneShot(switchSound, transform.position);

        if (lights[0].activeSelf)
        {
            animator.SetBool("lightOn", false);
        }
        else
        {
            animator.SetBool("lightOn", true);
        }

        foreach (GameObject light in lights)
        {
            if (light != null)
                light.SetActive(!light.activeSelf);
        }

        yield return new WaitForSeconds(0.5f);

        isAnimating = false;
    }
}
