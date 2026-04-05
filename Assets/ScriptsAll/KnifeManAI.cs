using FMOD.Studio;
using FMODUnity;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Audio;
using UnityEngine.SceneManagement;

public class KnifeManAI : MonoBehaviour
{
    public Transform playerTransform;
    
    private NavMeshAgent agent;
    private Animator animator;
    private bool isChasing = false;

    private float stopDistance = 2f; // Дистанция, на которой враг остановится

    [SerializeField] private EventReference attackSound;
    [SerializeField] private float attackCooldown = 2f;

    private float lastAttackTime;

    private void Awake()
    {
        agent = GetComponent<NavMeshAgent>();

        animator = GetComponent<Animator>();

        agent.stoppingDistance = stopDistance;
    }

    private void Update()
    {
        if (isChasing)
        {
            agent.SetDestination(playerTransform.position);
            float currentSpeed = agent.velocity.magnitude;
            animator.SetFloat("Speed", currentSpeed);
            TryAttack();
        }
    }

    private void TryAttack()
    {
        float distance = Vector3.Distance(transform.position, playerTransform.position);

        if (distance <= stopDistance + 0.1f)
        {
            Attack();
        }
    }

    private void Attack()
    {
        if (Time.time >= lastAttackTime + attackCooldown)
        {
            EventInstance inst = RuntimeManager.CreateInstance(attackSound);
            RuntimeManager.AttachInstanceToGameObject(inst, transform);
            inst.start();
            inst.release();
            lastAttackTime = Time.time;
        }
    }

    public void StartChasing()
    {
        isChasing = true;
    }
}
