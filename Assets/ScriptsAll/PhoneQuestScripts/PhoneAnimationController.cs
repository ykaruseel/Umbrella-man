using System.Collections;
using UnityEngine;

public class PhoneAnimationController : MonoBehaviour
{
    [Header("Model references")]
    [SerializeField] private Transform phoneRoot;
    [SerializeField] private Transform handset;
    [SerializeField] private Transform earPoint;

    [Header("Ringing vibration (wardrobe-style)")]
    [SerializeField, Min(0f)] private float vibrationAmplitude = 0.008f;
    [SerializeField, Min(0f)] private float vibrationDegrees = 2.5f;
    [SerializeField, Min(0.1f)] private float vibrationFrequency = 9f;
    [SerializeField, Min(0.1f)] private float ringPulseSeconds = 0.85f;
    [SerializeField, Range(0.1f, 1f)] private float activePulseFraction = 0.55f;

    [Header("Handset movement")]
    [SerializeField, Min(0.05f)] private float pickupDuration = 1f;
    [SerializeField, Min(0.05f)] private float returnDuration = 0.8f;
    [SerializeField, Min(0f)] private float liftArcHeight = 0.12f;

    [Header("Handset held pose (relative to Ear Point)")]
    [SerializeField] private Vector3 heldEulerOffset = new Vector3(0f, 0f, 90f);
    [SerializeField, Range(0.2f, 1.5f)] private float heldScaleMultiplier = 0.65f;

    private Vector3 rootRestPosition;
    private Quaternion rootRestRotation;
    private Vector3 handsetRestPosition;
    private Quaternion handsetRestRotation;
    private Vector3 handsetRestScale;
    private bool ringing;
    private float vibrationTime;

    private void Awake()
    {
        if (phoneRoot == null)
            phoneRoot = transform;

        rootRestPosition = phoneRoot.localPosition;
        rootRestRotation = phoneRoot.localRotation;

        if (handset != null)
        {
            handsetRestPosition = handset.localPosition;
            handsetRestRotation = handset.localRotation;
            handsetRestScale = handset.localScale;
        }
    }

    private void Update()
    {
        if (!ringing || phoneRoot == null)
            return;

        vibrationTime += Time.deltaTime;

        // One short rattle followed by a pause, like an old telephone ringing.
        float phase = (vibrationTime % ringPulseSeconds) / ringPulseSeconds;
        float pulse = 0f;
        if (phase < activePulseFraction)
        {
            float activeT = phase / activePulseFraction;
            pulse = Mathf.Sin(activeT * Mathf.PI);
        }

        // Mix gentle Perlin movement with a rhythmic shake, as in WardrobeAnimation.
        float noiseX = (Mathf.PerlinNoise(vibrationTime * vibrationFrequency, 0.0f) - 0.5f) * 2f;
        float noiseZ = (Mathf.PerlinNoise(vibrationTime * vibrationFrequency, 11.0f) - 0.5f) * 2f;
        float wave = Mathf.Sin(vibrationTime * vibrationFrequency * Mathf.PI * 2f);

        phoneRoot.localPosition = rootRestPosition +
            new Vector3(
                (noiseX * 0.7f + wave * 0.3f) * vibrationAmplitude * pulse,
                0f,
                noiseZ * vibrationAmplitude * 0.55f * pulse);

        phoneRoot.localRotation = rootRestRotation * Quaternion.Euler(
            noiseZ * vibrationDegrees * 0.25f * pulse,
            noiseX * vibrationDegrees * 0.30f * pulse,
            (noiseX * 0.55f + wave * 0.45f) * vibrationDegrees * pulse);
    }

    public void StartRinging()
    {
        StopRinging();
        vibrationTime = 0f;
        ringing = true;
    }

    public void StopRinging()
    {
        ringing = false;
        if (phoneRoot != null)
        {
            phoneRoot.localPosition = rootRestPosition;
            phoneRoot.localRotation = rootRestRotation;
        }
    }

    public IEnumerator PickUp()
    {
        StopRinging();
        if (handset == null || earPoint == null)
        {
            Debug.LogError("[PhoneAnimation] Assign Handset and Ear Point.");
            yield break;
        }

        Quaternion heldRotation =
            earPoint.rotation * Quaternion.Euler(heldEulerOffset);

        yield return MoveHandset(
            earPoint.position,
            heldRotation,
            pickupDuration,
            heldScaleMultiplier);
    }

    public IEnumerator PutBack()
    {
        if (handset == null)
            yield break;

        Transform parent = handset.parent;
        Vector3 goalPosition = parent != null
            ? parent.TransformPoint(handsetRestPosition)
            : handsetRestPosition;
        Quaternion goalRotation = parent != null
            ? parent.rotation * handsetRestRotation
            : handsetRestRotation;

        yield return MoveHandset(goalPosition, goalRotation, returnDuration, 1f);

        // Ensure that repeated calls do not accumulate pose or scale errors.
        handset.localPosition = handsetRestPosition;
        handset.localRotation = handsetRestRotation;
        handset.localScale = handsetRestScale;
    }

    private IEnumerator MoveHandset(
        Vector3 endPosition,
        Quaternion endRotation,
        float duration,
        float finalScaleMultiplier)
    {
        Vector3 startPosition = handset.position;
        Quaternion startRotation = handset.rotation;
        Vector3 startScale = handset.localScale;
        Vector3 goalScale = handsetRestScale * finalScaleMultiplier;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.SmoothStep(0f, 1f, Mathf.Clamp01(elapsed / duration));

            handset.position = Vector3.Lerp(startPosition, endPosition, t) +
                Vector3.up * (Mathf.Sin(t * Mathf.PI) * liftArcHeight);
            handset.rotation = Quaternion.Slerp(startRotation, endRotation, t);
            handset.localScale = Vector3.Lerp(startScale, goalScale, t);
            yield return null;
        }

        handset.SetPositionAndRotation(endPosition, endRotation);
        handset.localScale = goalScale;
    }

    public void ResetVisuals()
    {
        StopRinging();
        if (handset != null)
        {
            handset.localPosition = handsetRestPosition;
            handset.localRotation = handsetRestRotation;
            handset.localScale = handsetRestScale;
        }
    }

    private void OnDisable()
    {
        ResetVisuals();
    }
}
