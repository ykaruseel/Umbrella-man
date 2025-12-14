using UnityEngine;
using UnityEngine.AI;
using System.Collections;
using FMODUnity;
using FMOD.Studio;

public class UmbrellaManChase : MonoBehaviour
{
    [Header("References")]
    public Transform player;

    [Header("Chase Settings")]
    public float walkSpeed = 1.5f;
    public float catchDistance = 1.0f;

    [Header("Game Over UI")]
    public GameObject heGotYouUI;

    [Header("FMOD – дыхание человека с зонтом")]
    [SerializeField] private EventReference breathingLoopEvent;
    private EventInstance breathingInstance;

    [Header("FMOD – шаги человека с зонтом")]
    [SerializeField] private EventReference footstepEvent;
    public float footstepInterval = 0.6f;
    public bool useRandomStepInterval = false;
    public float minFootstepInterval = 0.45f;
    public float maxFootstepInterval = 0.8f;
    [Header("Chase Timing")]
    public float preChaseDelay = 2.0f;
    private Coroutine footstepCoroutine;

    [Header("FMOD – Heartbeat игрока")]
    [SerializeField] private EventReference heartbeatEvent;
    private EventInstance heartbeatInstance;

    private NavMeshAgent agent;
    private bool isChasing;
    private bool hasCaughtPlayer;
    private Coroutine chaseInitCoroutine;

    void Awake()
    {
        agent = GetComponent<NavMeshAgent>();

        if (agent == null)
        {
            Debug.LogError("[UmbrellaManChase] NavMeshAgent отсутствует");
            return;
        }

        agent.enabled = false;
        isChasing = false;
        hasCaughtPlayer = false;
    }

    public void StartChase()
    {
        if (player == null)
        {
            Debug.LogError("[UmbrellaManChase] Player не назначен");
            return;
        }

        gameObject.SetActive(true);

        if (chaseInitCoroutine != null)
            StopCoroutine(chaseInitCoroutine);

        chaseInitCoroutine = StartCoroutine(InitChaseRoutine());
    }

    private IEnumerator InitChaseRoutine()
    {
        yield return null;

        agent.enabled = true;

        while (!agent.isOnNavMesh)
            yield return null;

        agent.isStopped = true;
        agent.speed = walkSpeed;

        StartBreathingLoop();
        StartFootsteps();
        StartHeartbeat();

        yield return new WaitForSeconds(preChaseDelay);

        agent.isStopped = false;
        isChasing = true;
        hasCaughtPlayer = false;

        StartBreathingLoop();
        StartFootsteps();
        StartHeartbeat();

        Debug.Log("[UmbrellaManChase] Погоня ЗАПУЩЕНА");
    }

    public void StopChase()
    {
        isChasing = false;

        if (agent != null && agent.enabled)
        {
            agent.isStopped = true;
            agent.enabled = false;
        }

        StopBreathingLoop();
        StopFootsteps();
        StopHeartbeat();
    }

    void Update()
    {
        if (!isChasing || hasCaughtPlayer) return;
        if (agent == null || !agent.enabled || player == null) return;

        agent.SetDestination(player.position);

        if (Vector3.Distance(transform.position, player.position) <= catchDistance)
            HandleCatch();
    }

    private void HandleCatch()
    {
        if (hasCaughtPlayer) return;

        hasCaughtPlayer = true;
        isChasing = false;

        if (agent != null)
            agent.isStopped = true;

        StopBreathingLoop();
        StopFootsteps();
        StopHeartbeat();

        if (heGotYouUI != null)
            heGotYouUI.SetActive(true);

        var pc = player.GetComponent<PlayerController>();
        if (pc != null)
            pc.enabled = false;

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    private void StartBreathingLoop()
    {
        if (breathingLoopEvent.IsNull) return;

        breathingInstance = RuntimeManager.CreateInstance(breathingLoopEvent);
        RuntimeManager.AttachInstanceToGameObject(breathingInstance, transform, GetComponent<Rigidbody>());
        breathingInstance.start();
    }

    public void StopBreathingLoop()
    {
        if (!breathingInstance.isValid()) return;
        breathingInstance.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
        breathingInstance.release();
        breathingInstance.clearHandle();
    }

    private void StartFootsteps()
    {
        if (footstepEvent.IsNull) return;
        footstepCoroutine = StartCoroutine(FootstepLoop());
    }

    private void StopFootsteps()
    {
        if (footstepCoroutine != null)
        {
            StopCoroutine(footstepCoroutine);
            footstepCoroutine = null;
        }
    }

    private IEnumerator FootstepLoop()
    {
        while (isChasing && !hasCaughtPlayer)
        {
            RuntimeManager.PlayOneShotAttached(footstepEvent, gameObject);

            float delay = useRandomStepInterval
                ? Random.Range(minFootstepInterval, maxFootstepInterval)
                : footstepInterval;

            yield return new WaitForSeconds(delay);
        }
    }

    public void StartHeartbeat()
    {
        if (heartbeatEvent.IsNull) return;

        heartbeatInstance = RuntimeManager.CreateInstance(heartbeatEvent);
        heartbeatInstance.start();
    }

    public void StopHeartbeat()
    {
        if (!heartbeatInstance.isValid()) return;
        heartbeatInstance.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
        heartbeatInstance.release();
        heartbeatInstance.clearHandle();
    }

    private void OnDestroy()
    {
        StopChase();
    }
}