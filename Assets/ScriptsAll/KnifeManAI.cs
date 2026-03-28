using UnityEngine;
using UnityEngine.AI;
using UnityEngine.SceneManagement;

public class KnifeManAI : MonoBehaviour
{
    public Transform playerTransform;
    
    private NavMeshAgent agent;
    private bool isChasing = false;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
    }

    void Update()
    {
        if (isChasing)
        {
            agent.SetDestination(playerTransform.position);
        }
    }

    public void StartChase() => isChasing = true;
    public void StopChase() => isChasing = false;

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            AttackPlayer();
        }
    }

    void AttackPlayer()
    {
           // hz kak pokazat` czto nas udarili
    }
}
