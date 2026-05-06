using FMOD.Studio;
using FMODUnity;
using TMPro;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Audio;
using UnityEngine.SceneManagement;

public class KnifeManAI : MonoBehaviour
{
    public Transform playerTransform;
    
    private NavMeshAgent agent;
    private Animator animator;
    public bool isChasing = false;

    private float stopDistance = 2f;

    [SerializeField] private EventReference attackSound;
    [SerializeField] private float attackCooldown = 2f;
    [SerializeField] private EventReference footstepEvent;

    [SerializeField] private DeathHandler deathHandler;

    private int playerHealth = 3;

    private float lastAttackTime;

    private HitEffect hitEffect;

    private Vector3 originalLocalPos;
    private Quaternion originalLocalRot;

    private void Awake()
    {
        agent = GetComponent<NavMeshAgent>();

        animator = GetComponent<Animator>();

        agent.stoppingDistance = stopDistance;

        hitEffect = GetComponent<HitEffect>();

        originalLocalPos = transform.localPosition;
        originalLocalRot = transform.localRotation;
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
            hitEffect.TakeDamageEffect();
            playerHealth--;
            if(playerHealth <= 0)
            {
                deathHandler.TriggerDeath();
                isChasing = false;
                playerHealth = 3;
            }
        }
    }

    public void PlayFootstep()
    {
        if (footstepEvent.IsNull) return;

        RuntimeManager.PlayOneShotAttached(footstepEvent, gameObject);
    }

    public void StartChasing()
    {
        isChasing = true;
    }

    public void ResetChasing()
    {
        agent.Warp(originalLocalPos);
        transform.rotation = originalLocalRot;
    }
}
