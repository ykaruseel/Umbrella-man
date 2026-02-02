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
    // ЭТО ПОЛЕ БОЛЬШЕ НЕ НУЖНО ИСПОЛЬЗОВАТЬ ЗДЕСЬ, НО Я ОСТАВИЛ ЧТОБЫ НЕ СБИЛАСЬ ССЫЛКА
    // ТЕПЕРЬ ЗА ЭКРАН ОТВЕЧАЕТ DeathHandler НА ИГРОКЕ
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
        if (MusicManager.Instance != null)
        {
            MusicManager.Instance.EnsureMusicPlaying();
            MusicManager.Instance.SetSection("Value D");
        }

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

    // --- ВОТ ТУТ ГЛАВНЫЕ ИЗМЕНЕНИЯ ---
    private void HandleCatch()
    {
        if (hasCaughtPlayer) return;

        hasCaughtPlayer = true;
        isChasing = false;

        // 1. Останавливаем врага
        if (agent != null)
            agent.isStopped = true;

        // 2. Глушим звуки шагов и дыхания (чтобы не мешали звукам скримера, если будут)
        StopBreathingLoop();
        StopFootsteps();
        StopHeartbeat();

        // 3. ВМЕСТО ВКЛЮЧЕНИЯ ЭКРАНА НАПРЯМУЮ, ВЫЗЫВАЕМ DEATHHANDLER
        if (player != null)
        {
            var deathHandler = player.GetComponent<DeathHandler>();
            
            if (deathHandler != null)
            {
                // Передаем 'transform' (себя), чтобы камера игрока повернулась на нас
                deathHandler.TriggerDeath(transform);
            }
            else
            {
                // Если забыл повесить скрипт на игрока — сработает старый метод (как страховка)
                Debug.LogWarning("DeathHandler не найден на игроке! Использую старый метод.");
                if (heGotYouUI != null) heGotYouUI.SetActive(true);
                var pc = player.GetComponent<PlayerController>();
                if (pc != null) pc.enabled = false;
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
            }
        }
    }
    // ---------------------------------

    private void StartBreathingLoop()
    {
        if (breathingLoopEvent.IsNull) return;
        if (breathingInstance.isValid()) return;
        breathingInstance = RuntimeManager.CreateInstance(breathingLoopEvent);
        RuntimeManager.AttachInstanceToGameObject(breathingInstance, transform, GetComponent<Rigidbody>());
        breathingInstance.start();
    }

    public void StopBreathingLoop()
    {
        if (!breathingInstance.isValid()) return;
        breathingInstance.stop(FMOD.Studio.STOP_MODE.IMMEDIATE);
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
        while (isChasing && !hasCaughtPlayer && !Pause.isPaused)
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
        if (heartbeatInstance.isValid()) return;
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

    public void ResetChase()
    {
        StopBreathingLoop();
        StopFootsteps();
        StopHeartbeat();
        StopChase();

        // Сброс позиции в невидимую зону (или стартовую точку)
        transform.position = new Vector3(-0.64f, 0.6480125f, -31.8f);
        
        hasCaughtPlayer = false;
        isChasing = false;

        if (agent != null)
            agent.enabled = false;

        // ВАЖНО: Выключаем самого человека, чтобы CinematicReveal мог включить его заново
        gameObject.SetActive(false); 
    }

    public void PauseChase()
    {
        if (!isChasing) return;

        if (agent != null && agent.enabled)
            agent.isStopped = true;

        PauseBreathing(true);
        PauseHeartbeat(true);
    }

    public void ResumeChase()
    {
        if (!isChasing || hasCaughtPlayer) return;

        if (agent != null && agent.enabled)
            agent.isStopped = false;

        StartCoroutine(FootstepLoop());
        PauseBreathing(false);
        PauseHeartbeat(false);
    }

    private void PauseBreathing(bool pause)
    {
        if (!breathingInstance.isValid()) return;
        breathingInstance.setPaused(pause);
    }

    private void PauseHeartbeat(bool pause)
    {
        if (!heartbeatInstance.isValid()) return;
        heartbeatInstance.setPaused(pause);
    }

    private void OnDestroy()
    {
        StopChase();
    }
}