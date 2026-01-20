using UnityEngine;
using System.Collections;

public class DoorController : MonoBehaviour
{
    [SerializeField] private Transform door;
    [SerializeField] private Transform handle;

    [SerializeField] private float openAngle = 90f;
    [SerializeField] private float openDuration = 1f;
    [SerializeField] private float handleRotationAngle = 30f;
    [SerializeField] private Vector3 handleRotationAxis = Vector3.right;

    [SerializeField] private float autoCloseMin = 10f;
    [SerializeField] private float autoCloseMax = 15f;

    private Quaternion closedRotation;
    private Quaternion openRotation;
    private Quaternion handleClosedRotation;
    private Quaternion handleOpenRotation;

    private bool isOpen = false;
    private bool isAnimating = false;
    private Coroutine autoCloseCoroutine;

    private void Start()
    {
        closedRotation = door.localRotation;
        openRotation = closedRotation * Quaternion.Euler(0, openAngle, 0);


        handleClosedRotation = handle.localRotation;
        handleOpenRotation = handleClosedRotation * Quaternion.AngleAxis(handleRotationAngle, handleRotationAxis);
    }

    public void TryOpenDoor()
    {
        if (isAnimating) return;
        Debug.Log("Door interaction triggered.");
        if (!isOpen)
            StartCoroutine(OpenDoor());
        else
            StartCoroutine(CloseDoor());
    }

    IEnumerator OpenDoor()
    {
        isAnimating = true;

        float handleTime = openDuration * 0.3f;
        float doorTime = openDuration;

        float t;

        t = 0f;
        while (t < 1f)
        {
            t += Time.deltaTime / handleTime;
            handle.localRotation = Quaternion.Slerp(
                handleClosedRotation,
                handleOpenRotation,
                t
            );
            yield return null;
        }

        handle.localRotation = handleOpenRotation;

        t = 0f;
        while (t < 1f)
        {
            t += Time.deltaTime / doorTime;

            door.localRotation = Quaternion.Slerp(
                closedRotation,
                openRotation,
                t
            );

            handle.localRotation = Quaternion.Slerp(
                handleOpenRotation,
                handleClosedRotation,
                t
            );

            yield return null;
        }

        door.localRotation = openRotation;
        handle.localRotation = handleClosedRotation;

        isOpen = true;
        isAnimating = false;

        if (autoCloseCoroutine != null)
            StopCoroutine(autoCloseCoroutine);

        autoCloseCoroutine = StartCoroutine(AutoCloseDoor());
    }


    private IEnumerator CloseDoor()
    {
        isAnimating = true;

        float t = 0f;
        while (t < 1f)
        {
            t += Time.deltaTime / openDuration;
            door.localRotation = Quaternion.Slerp(openRotation, closedRotation, t);
            yield return null;
        }

        door.localRotation = closedRotation;

        isOpen = false;
        isAnimating = false;

        if (autoCloseCoroutine != null)
        {
            StopCoroutine(autoCloseCoroutine);
            autoCloseCoroutine = null;
        }
    }

    private IEnumerator AutoCloseDoor()
    {
        float waitTime = Random.Range(autoCloseMin, autoCloseMax);
        yield return new WaitForSeconds(waitTime);

        if (!isAnimating && isOpen)
            StartCoroutine(CloseDoor());
    }
}
