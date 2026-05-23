using UnityEngine;
using UnityEngine.AI;
using System.Collections;

public class GhostAppearanceEvent : MonoBehaviour
{
    [Header("Character display")]
    [Tooltip("Link to the maniac object on the stage (must be turned off beforehand))")]
    public GameObject ghostObject; 
    [Tooltip("The end point is around the corner where the maniac should disappear.")]
    public Transform targetPoint;

    
    
    [Header("Settings")]
    [Tooltip("The distance to the final point at which the maniac will disappear (not visible to the player)")]
    public float despawnDistance = 1.0f;
    
    
    [Tooltip("The speed at which the maniac walks on the stairs (set in NavMeshAgent)")]
    public float walkSpeed = 3.5f;

    private bool isTriggered = false;
    private NavMeshAgent ghostAgent;
    private bool isGhostMoving = false;

    private void Start()
    {
        
        GetComponent<Collider>().isTrigger = true;
    }

    private void OnTriggerEnter(Collider other)
    {
        
        if (other.CompareTag("Player") && !isTriggered)
        {
            StartEvent();
        }
    }

    private void StartEvent()
    {
        isTriggered = true;

        if (ghostObject != null && targetPoint != null)
        {
            
            ghostObject.SetActive(true);

            
            ghostAgent = ghostObject.GetComponent<NavMeshAgent>();
            if (ghostAgent != null)
            {
                
                ghostAgent.speed = walkSpeed;

                
                ghostAgent.SetDestination(targetPoint.position);
                isGhostMoving = true;
                
                
                StartCoroutine(MonitorGhostArrival());
            }
        }
    }

    
    private IEnumerator MonitorGhostArrival()
    {
        while (isGhostMoving)
        {
            if (ghostObject != null && ghostAgent != null)
            {
                
                float distance = Vector3.Distance(ghostObject.transform.position, targetPoint.position);
                
                
                if (distance <= despawnDistance)
                {
                    isGhostMoving = false;
                    
                    
                    ghostObject.SetActive(false); 
                    
                    
                    Destroy(gameObject);
                    
                    yield break;
                }
            }
            
            yield return new WaitForSeconds(0.2f);
        }
    }
}
