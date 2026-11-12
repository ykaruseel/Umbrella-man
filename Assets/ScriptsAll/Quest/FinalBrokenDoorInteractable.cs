
using System.Collections;
using UnityEngine;
using FMODUnity;
using FMOD.Studio;

public class FinalBrokenDoorInteractable : MonoBehaviour
{
    [Header("Door")]
    [SerializeField] private Transform doorPivot;

    [Header("Animation")]
    [SerializeField] private float openAngle = -90f;
    [SerializeField] private float openDuration = 1.2f;

    [Header("FMOD")]
    [SerializeField] private EventReference doorSound;

    private Quaternion closedRotation;
    private Quaternion openedRotation;

    private bool isOpen;
    private bool isAnimating;

    private void Awake()
    {
        if (doorPivot == null)
            doorPivot = transform;

        closedRotation = doorPivot.localRotation;

        openedRotation = closedRotation *
            Quaternion.Euler(0f, openAngle, 0f);
    }

    public bool TryInteract()
    {
        if (isAnimating)
            return true;

        bool shouldOpen = !isOpen;

        StartCoroutine(
            AnimateDoor(shouldOpen)
        );

        return true;
    }

    private IEnumerator AnimateDoor(bool shouldOpen)
    {
        isAnimating = true;

        Quaternion startRotation =
            doorPivot.localRotation;

        Quaternion targetRotation =
            shouldOpen
                ? openedRotation
                : closedRotation;

        PlayDoorSound(shouldOpen);

        float elapsed = 0f;
        float duration = Mathf.Max(0.05f, openDuration);

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;

            float t = Mathf.SmoothStep(
                0f,
                1f,
                Mathf.Clamp01(elapsed / duration)
            );

            doorPivot.localRotation =
                Quaternion.Slerp(
                    startRotation,
                    targetRotation,
                    t
                );

            yield return null;
        }

        doorPivot.localRotation = targetRotation;

        isOpen = shouldOpen;
        isAnimating = false;
    }

    private void PlayDoorSound(bool opening)
    {
        if (doorSound.IsNull)
            return;

        EventInstance instance =
            RuntimeManager.CreateInstance(doorSound);

        RuntimeManager.AttachInstanceToGameObject(
            instance,
            gameObject
        );

        instance.setParameterByName(
            "Door",
            opening ? 0f : 1f
        );

        instance.start();
        instance.release();
    }
}
