using System.Collections;
using UnityEngine;

public class WardrobeAnimation : MonoBehaviour
{
    [SerializeField] private Transform leftDoor;
    [SerializeField] private Transform rightDoor;
    [SerializeField] private ParticleSystem dustParticles;

    [SerializeField] private float duration = 4f;
    [SerializeField] private float shakeAmount = 0.05f;
    [SerializeField] private float rotationAmount = 5f;
    [SerializeField] private float doorOpenAngle = 15f;
    [SerializeField] private float intensity = 1f;

    [SerializeField] private float minDoorInterval = 0.25f;
    [SerializeField] private float maxDoorInterval = 0.6f;
    [SerializeField] private float doorSlamSpeed = 25f;

    [SerializeField] private float minDustInterval = 0.25f;
    [SerializeField] private float maxDustInterval = 0.7f;

    private Vector3 initialPosition;
    private Quaternion initialRotation;
    private Quaternion leftDoorInitialRotation;
    private Quaternion rightDoorInitialRotation;

    private Coroutine animationCoroutine;

    private float leftDoorTarget;
    private float rightDoorTarget;
    private float nextDoorAction;
    private float nextDustAction;

    private void Awake()
    {
        initialPosition = transform.localPosition;
        initialRotation = transform.localRotation;

        if (leftDoor != null)
            leftDoorInitialRotation = leftDoor.localRotation;

        if (rightDoor != null)
            rightDoorInitialRotation = rightDoor.localRotation;

        nextDoorAction = Time.time + Random.Range(minDoorInterval, maxDoorInterval);
        nextDustAction = Time.time + Random.Range(minDustInterval, maxDustInterval);
    }

    private void Start()
    {
        StartAnimation();
    }

    public void StartAnimation()
    {
        if (animationCoroutine != null)
            StopCoroutine(animationCoroutine);

        animationCoroutine = StartCoroutine(WardrobeRoutine());
    }

    private IEnumerator WardrobeRoutine()
    {
        float startTime = Time.time;

        nextDoorAction = Time.time + 0.2f;
        nextDustAction = Time.time + 0.2f;

        if (dustParticles != null)
            dustParticles.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);

        while (Time.time - startTime < duration)
        {
            float elapsed = Time.time - startTime;
            float normalizedTime = elapsed / duration;

            float energy = Mathf.Lerp(0.7f, 1f, normalizedTime);

            float noiseX = Mathf.PerlinNoise(Time.time * 20f, 0f) - 0.5f;
            float noiseY = Mathf.PerlinNoise(Time.time * 25f, 10f) - 0.5f;
            float noiseZ = Mathf.PerlinNoise(Time.time * 22f, 20f) - 0.5f;

            Vector3 shake = new Vector3(
                noiseX * shakeAmount,
                noiseY * shakeAmount,
                noiseZ * shakeAmount
            );

            float rotationX = Mathf.Sin(Time.time * 18f) * rotationAmount;
            float rotationY = Mathf.Sin(Time.time * 8f) * rotationAmount;
            float rotationZ = Mathf.Sin(Time.time * 13f) * rotationAmount;

            transform.localPosition =
                initialPosition + shake * intensity * energy;

            transform.localRotation =
                initialRotation *
                Quaternion.Euler(
                    rotationX * intensity * energy,
                    rotationY * intensity * energy,
                    rotationZ * intensity * energy
                );

            HandleDoors();
            HandleDust();

            if (leftDoor != null)
            {
                Quaternion targetRotation =
                    leftDoorInitialRotation *
                    Quaternion.Euler(0f, leftDoorTarget, 0f);

                leftDoor.localRotation = Quaternion.Lerp(
                    leftDoor.localRotation,
                    targetRotation,
                    Time.deltaTime * doorSlamSpeed
                );
            }

            if (rightDoor != null)
            {
                Quaternion targetRotation =
                    rightDoorInitialRotation *
                    Quaternion.Euler(0f, rightDoorTarget, 0f);

                rightDoor.localRotation = Quaternion.Lerp(
                    rightDoor.localRotation,
                    targetRotation,
                    Time.deltaTime * doorSlamSpeed
                );
            }

            yield return null;
        }

        yield return StartCoroutine(ReturnToOriginal());

        if (dustParticles != null)
            dustParticles.Stop(true, ParticleSystemStopBehavior.StopEmitting);

        animationCoroutine = null;
    }

    private void HandleDoors()
    {
        if (Time.time < nextDoorAction)
            return;

        if (Random.value > 0.5f)
        {
            if (Mathf.Abs(leftDoorTarget) < 0.1f)
                leftDoorTarget = Random.Range(0.7f, 1f) * doorOpenAngle;
            else
                leftDoorTarget = 0f;
        }
        else
        {
            if (Mathf.Abs(rightDoorTarget) < 0.1f)
                rightDoorTarget = Random.Range(-1f, -0.7f) * doorOpenAngle;
            else
                rightDoorTarget = 0f;
        }

        nextDoorAction =
            Time.time +
            Random.Range(minDoorInterval, maxDoorInterval);
    }

    private void HandleDust()
    {
        if (dustParticles == null)
            return;

        if (Time.time < nextDustAction)
            return;

        dustParticles.Play();

        nextDustAction =
            Time.time +
            Random.Range(minDustInterval, maxDustInterval);
    }

    private IEnumerator ReturnToOriginal()
    {
        float timer = 0f;
        float returnDuration = 0.5f;

        Vector3 startPosition = transform.localPosition;
        Quaternion startRotation = transform.localRotation;

        Quaternion leftStart =
            leftDoor != null ? leftDoor.localRotation : Quaternion.identity;

        Quaternion rightStart =
            rightDoor != null ? rightDoor.localRotation : Quaternion.identity;

        while (timer < returnDuration)
        {
            timer += Time.deltaTime;

            float t = Mathf.Clamp01(timer / returnDuration);
            t = 1f - Mathf.Pow(1f - t, 3f);

            transform.localPosition =
                Vector3.Lerp(startPosition, initialPosition, t);

            transform.localRotation =
                Quaternion.Slerp(startRotation, initialRotation, t);

            if (leftDoor != null)
            {
                leftDoor.localRotation =
                    Quaternion.Slerp(leftStart, leftDoorInitialRotation, t);
            }

            if (rightDoor != null)
            {
                rightDoor.localRotation =
                    Quaternion.Slerp(rightStart, rightDoorInitialRotation, t);
            }

            yield return null;
        }

        transform.localPosition = initialPosition;
        transform.localRotation = initialRotation;

        if (leftDoor != null)
            leftDoor.localRotation = leftDoorInitialRotation;

        if (rightDoor != null)
            rightDoor.localRotation = rightDoorInitialRotation;

        leftDoorTarget = 0f;
        rightDoorTarget = 0f;
    }
}