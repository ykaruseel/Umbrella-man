using UnityEngine;
using UnityEngine.Events;
using System.Collections;
using Unity.Cinemachine; 

public class DialogueTestSequence : MonoBehaviour
{
    [Header("Камеры (Cinemachine)")]
    public CinemachineCamera playerCam; 
    public CinemachineCamera dialogCam1;
    public CinemachineCamera dialogCam2;

    [Header("Блокировка управления")]
    public UnityEvent onLockPlayer;   
    public UnityEvent onUnlockPlayer; 

    private bool hasTriggered = false;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") && !hasTriggered)
        {
            hasTriggered = true;
            StartCoroutine(RunSequence());
        }
    }

    private IEnumerator RunSequence()
    {
        
        onLockPlayer?.Invoke();

        
        yield return new WaitForSeconds(5f);

        
        if (playerCam != null) playerCam.Priority = 0;
        if (dialogCam1 != null) dialogCam1.Priority = 100;
        if (dialogCam2 != null) dialogCam2.Priority = 0;

        yield return new WaitForSeconds(3f);

        
        if (dialogCam1 != null) dialogCam1.Priority = 0;
        if (dialogCam2 != null) dialogCam2.Priority = 100;

        yield return new WaitForSeconds(3f);

        
        if (dialogCam2 != null) dialogCam2.Priority = 0;
        if (playerCam != null) playerCam.Priority = 100; 
        
        
        onUnlockPlayer?.Invoke();
    }
}
