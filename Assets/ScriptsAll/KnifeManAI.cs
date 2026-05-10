using FMOD.Studio;
using FMODUnity;
using UnityEngine;
using UnityEngine.AI;

public class KnifeManAI : MonoBehaviour
{
    public Transform playerTransform;

    private NavMeshAgent agent;
    private Animator animator;

    public bool isChasing = false;

    private float stopDistance = 2f;

    [Header("FMOD")]
    [SerializeField] private EventReference attackSound;
    [SerializeField] private EventReference footstepEvent;
    [SerializeField] private EventReference heartbeatEvent;
    [SerializeField] private EventReference breathingEvent;

    [SerializeField] private float attackCooldown = 2f;
    [SerializeField] private float maxHeartbeatDistance = 15f;

    private EventInstance heartbeatInstance;
    private bool heartbeatStarted = false;
    private EventInstance breathingInstance;

    private bool breathingStarted = false;

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
        UpdateHeartbeat();

        if (isChasing)
        {
            agent.SetDestination(playerTransform.position);

            float currentSpeed = agent.velocity.magnitude;
            animator.SetFloat("Speed", currentSpeed);
                
            TryAttack();

            StartBreathing();
            StartHeartbeat();

            UpdateHeartbeat();
        }
        else
        {
            StopBreathing();
            StopHeartbeat();
        }
    }

    private void StartBreathing()
    {
        if (breathingStarted || breathingEvent.IsNull)
            return;

        breathingInstance = RuntimeManager.CreateInstance(breathingEvent);

        RuntimeManager.AttachInstanceToGameObject(
            breathingInstance,
            transform,
            GetComponent<Rigidbody>()
        );

        breathingInstance.start();

        breathingStarted = true;
    }

    private void StopBreathing()
    {
        if (!breathingStarted)
            return;

        if (breathingInstance.isValid())
        {
            breathingInstance.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
            breathingInstance.release();
        }

        breathingStarted = false;
    }
    private void StartHeartbeat()
    {
        if (heartbeatStarted || heartbeatEvent.IsNull)
            return;

        heartbeatInstance = RuntimeManager.CreateInstance(heartbeatEvent);

        heartbeatInstance.start();

        heartbeatStarted = true;
    }

    private void StopHeartbeat()
    {
        if (!heartbeatStarted)
            return;

        if (heartbeatInstance.isValid())
        {
            heartbeatInstance.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
            heartbeatInstance.release();
        }

        heartbeatStarted = false;
    }
    private void UpdateHeartbeat()
    {
        if (!heartbeatInstance.isValid())
            return;

        float distance = Vector3.Distance(transform.position, playerTransform.position);

        float normalized = 1f - Mathf.Clamp01(distance / maxHeartbeatDistance);

        heartbeatInstance.setParameterByName("Intensity", normalized);
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

            if (playerHealth <= 0)
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
        isChasing = false;

        agent.Warp(originalLocalPos);

        transform.rotation = originalLocalRot;
    }

    private void OnDestroy()
    {
        if (heartbeatInstance.isValid())
        {
            heartbeatInstance.stop(FMOD.Studio.STOP_MODE.IMMEDIATE);
            heartbeatInstance.release();
        }

        if (breathingInstance.isValid())
        {
            breathingInstance.stop(FMOD.Studio.STOP_MODE.IMMEDIATE);
            breathingInstance.release();
        }
    }
}