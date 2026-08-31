using UnityEngine;
using UnityEngine.AI;
using System.Collections;

public class GhostAppearanceEvent : MonoBehaviour
{
    [Header("Отображение персонажа")]
    [Tooltip("Ссылка на объект маньяка на сцене (должен быть заранее выключен)")]
    public GameObject ghostObject; 
    [Tooltip("Конечная точка за углом, где маньяк должен исчезнуть")]
    public Transform targetPoint;

    [Header("Настройки")]
    [Tooltip("Дистанция до конечной точки, при которой маньяк исчезнет")]
    public float despawnDistance = 1.2f;
    
    [Tooltip("Скорость ходьбы маньяка по лестнице")]
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
        
        yield return new WaitForSeconds(0.3f);

        while (isGhostMoving)
        {
            if (ghostObject != null && ghostAgent != null)
            {
                
                float distance = Vector3.Distance(ghostObject.transform.position, targetPoint.position);
                
                
                float remainingDist = ghostAgent.remainingDistance;

                
                bool pathCompleted = !ghostAgent.pathPending && 
                                     ghostAgent.pathStatus == NavMeshPathStatus.PathComplete && 
                                     remainingDist <= despawnDistance;

                
                if (distance <= despawnDistance || pathCompleted || (ghostAgent.velocity.sqrMagnitude <= 0.05f))
                {
                    isGhostMoving = false;
                    ghostObject.SetActive(false);
                    
                    Destroy(gameObject);
                    yield break; 
                }
            }
            yield return new WaitForSeconds(0.05f);
        }
    }
}
