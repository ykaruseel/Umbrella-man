using UnityEngine;
using System.Collections;
using FMODUnity;

public class CreepyDoorEvent : MonoBehaviour
{
    [Header("Objects")]
    [Tooltip("The actual door model that will rotate")]
    public Transform doorTransform;
    [Tooltip("Player to measure distance (The Radar target)")]
    public Transform player;

    [Header("Settings")]
    [Tooltip("Distance to trigger the slow opening (e.g., 8 meters)")]
    public float triggerDistance = 8f;
    [Tooltip("Distance to trigger the aggressive slam (e.g., 2.5 meters)")]
    public float slamDistance = 2.5f;
    [Tooltip("How many degrees the door opens (use -15 if it opens the wrong way)")]
    public float peekAngle = 15f;
    [Tooltip("How fast the door opens (lower is slower)")]
    public float openSpeed = 0.5f;
    
    [Header("Slam Settings")]
    [Tooltip("How fast the door slams shut (higher is faster)")]
    public float slamSpeed = 15f;

    [SerializeField] private EventReference doorEvent;

    private bool hasTriggeredOpen = false;
    private bool hasSlammed = false;
    
    private Quaternion closedRotation;
    private Quaternion peekRotation;

    void Start()
    {
        if (doorTransform != null)
        {
            
            closedRotation = doorTransform.localRotation;
            
            peekRotation = closedRotation * Quaternion.Euler(0, peekAngle, 0);
        }
    }

    void Update()
    {
        if (doorTransform == null || player == null) return;

        float dist = Vector3.Distance(doorTransform.position, player.position);

        if (dist <= triggerDistance && !hasTriggeredOpen)
        {
            hasTriggeredOpen = true;

            PlayDoorSound(1);

            StartCoroutine(OpenDoorSlowly());
        }

        if (dist <= slamDistance && hasTriggeredOpen && !hasSlammed)
        {
            SlamDoor();
        }
    }

    IEnumerator OpenDoorSlowly()
    {
        float t = 0;
        while (t < 1f && !hasSlammed)
        {
            t += Time.deltaTime * openSpeed;
            doorTransform.localRotation = Quaternion.Slerp(closedRotation, peekRotation, t);
            yield return null;
        }
    }

    void SlamDoor()
    {
        hasSlammed = true;
        
        
        StopAllCoroutines();


        PlayDoorSound(2);


        StartCoroutine(SlamDoorFast());
    }

    IEnumerator SlamDoorFast()
    {
        float t = 0;
        
        Quaternion currentRot = doorTransform.localRotation;
        
        while (t < 1f)
        {
            t += Time.deltaTime * slamSpeed;
            doorTransform.localRotation = Quaternion.Slerp(currentRot, closedRotation, t);
            yield return null;
        }
        
        
        doorTransform.localRotation = closedRotation;

        
        this.enabled = false;
    }
    private void PlayDoorSound(int state)
    {
        if (doorEvent.IsNull) return;

        var instance = RuntimeManager.CreateInstance(doorEvent);
        instance.set3DAttributes(RuntimeUtils.To3DAttributes(doorTransform.position));
        instance.setParameterByName("Door", state);
        instance.start();
        instance.release();
    }
}
