// UmbrellaManChase.cs (updated + HEARTBEAT)
// Добавлены:
// • Стук дыхания (как в твоей версии)
// • Шаги (как в твоей версии)
// • Сердцебиение игрока (2D loop) — новый функционал

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
    [Tooltip("FMOD Event, в котором крутится петля дыхания (3D)")]
    [SerializeField] private EventReference breathingLoopEvent;
    private EventInstance breathingInstance;

    [Header("FMOD – шаги человека с зонтом")]
    [Tooltip("FMOD Event для шагов.")]
    [SerializeField] private EventReference footstepEvent;

    [Tooltip("Фиксированный интервал между шагами")]
    public float footstepInterval = 0.6f;

    [Tooltip("Если true — интервал будет рандомным")]
    public bool useRandomStepInterval = false;

    [Tooltip("Диапазон рандомного интервала")]
    public float minFootstepInterval = 0.45f;
    public float maxFootstepInterval = 0.8f;

    private Coroutine footstepCoroutine = null;

    // --------------------------
    // 🔥 НОВОЕ – FMOD HEARTBEAT
    // --------------------------
    [Header("FMOD – Сердцебиение игрока во время погони")]
    [SerializeField] private EventReference heartbeatEvent;
    private EventInstance heartbeatInstance;
    // --------------------------

    private NavMeshAgent agent;
    private bool isChasing = false;
    private bool hasCaughtPlayer = false;

    void Awake()
    {
        agent = GetComponent<NavMeshAgent>();

        if (agent == null)
        {
            Debug.LogError("[UmbrellaManChase] Нет NavMeshAgent на объекте!");
            return;
        }

        agent.enabled = false;
        agent.updateRotation = true;
        agent.updateUpAxis = true;
    }

    public void StartChase()
    {
        if (agent == null || player == null)
        {
            Debug.LogError("[UmbrellaManChase] Ошибка запуска погони: агент или игрок не назначен");
            return;
        }

        NavMeshHit hit;
        if (NavMesh.SamplePosition(transform.position, out hit, 2f, NavMesh.AllAreas))
        {
            agent.Warp(hit.position);
        }
        else
        {
            Debug.LogError("[UmbrellaManChase] Не найден NavMesh рядом!");
        }

        agent.enabled = true;
        agent.isStopped = false;
        agent.speed = walkSpeed;

        isChasing = true;
        hasCaughtPlayer = false;

        StartBreathingLoop();
        StartFootsteps();

        // 🔥 Запуск сердцебиения
        StartHeartbeat();

        Debug.Log("[UmbrellaManChase] Погоня ЗАПУЩЕНА");
    }

    public void StopChase()
    {
        if (agent == null) return;

        isChasing = false;
        agent.isStopped = true;
        agent.enabled = false;

        StopBreathingLoop();
        StopFootsteps();

        // 🔥 Остановка сердцебиения
        StopHeartbeat();

        Debug.Log("[UmbrellaManChase] Погоня ОСТАНОВЛЕНА");
    }

    void Update()
    {
        if (!isChasing || hasCaughtPlayer) return;
        if (agent == null || !agent.enabled || player == null) return;

        agent.SetDestination(player.position);

        float distance = Vector3.Distance(transform.position, player.position);
        if (distance <= catchDistance)
        {
            HandleCatch();
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!isChasing || hasCaughtPlayer) return;

        if (other.transform == player || other.GetComponent<PlayerController>() != null)
        {
            HandleCatch();
        }
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

        // 🔥 Остановка сердцебиения
        StopHeartbeat();

        Debug.Log("[UmbrellaManChase] Игрок пойман!");

        if (heGotYouUI != null)
        {
            heGotYouUI.SetActive(true);
        }
        else
        {
            QuestManager qm = QuestManager.instance;
            if (qm != null && qm.gameOverUI != null)
                qm.gameOverUI.SetActive(true);
        }

        if (player != null)
        {
            var pc = player.GetComponent<PlayerController>();
            if (pc != null)
                pc.enabled = false;
        }

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    // -----------------------------
    // FMOD: ДЫХАНИЕ
    // -----------------------------
    private void StartBreathingLoop()
    {
        if (breathingLoopEvent.IsNull)
        {
            Debug.LogWarning("[UmbrellaManChase] breathingLoopEvent не назначен");
            return;
        }

        if (breathingInstance.isValid())
        {
            PLAYBACK_STATE state;
            breathingInstance.getPlaybackState(out state);
            if (state != PLAYBACK_STATE.STOPPED)
                return;
        }
        else
        {
            breathingInstance = RuntimeManager.CreateInstance(breathingLoopEvent);
            var rb = GetComponent<Rigidbody>();
            RuntimeManager.AttachInstanceToGameObject(breathingInstance, transform, rb);
        }

        breathingInstance.start();
    }

    private void StopBreathingLoop()
    {
        if (!breathingInstance.isValid()) return;

        breathingInstance.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
        breathingInstance.release();
        breathingInstance.clearHandle();
    }

    // -----------------------------
    // FMOD: ШАГИ
    // -----------------------------
    private void StartFootsteps()
    {
        if (footstepEvent.IsNull) return;

        if (footstepCoroutine == null)
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
            if (!footstepEvent.IsNull)
                RuntimeManager.PlayOneShotAttached(footstepEvent, gameObject);

            float delay = useRandomStepInterval
                ? Random.Range(minFootstepInterval, maxFootstepInterval)
                : Mathf.Max(0.01f, footstepInterval);

            yield return new WaitForSeconds(delay);
        }

        footstepCoroutine = null;
    }

    // -----------------------------
    // 🔥 FMOD: HEARTBEAT (2D)
    // -----------------------------
    public void StartHeartbeat()
    {
        if (heartbeatEvent.IsNull)
        {
            Debug.LogWarning("[UmbrellaManChase] heartbeatEvent не назначен");
            return;
        }

        if (heartbeatInstance.isValid())
        {
            PLAYBACK_STATE state;
            heartbeatInstance.getPlaybackState(out state);
            if (state != PLAYBACK_STATE.STOPPED)
                return;
        }
        else
        {
            heartbeatInstance = RuntimeManager.CreateInstance(heartbeatEvent);
            heartbeatInstance.start(); // 2D звук – не привязываем к объекту
        }

        Debug.Log("[UmbrellaManChase] Heartbeat START");
    }

    public void StopHeartbeat()
    {
        if (!heartbeatInstance.isValid()) return;

        heartbeatInstance.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
        heartbeatInstance.release();
        heartbeatInstance.clearHandle();

        Debug.Log("[UmbrellaManChase] Heartbeat STOP");
    }

    // -----------------------------

    private void OnDestroy()
    {
        if (breathingInstance.isValid())
        {
            breathingInstance.stop(FMOD.Studio.STOP_MODE.IMMEDIATE);
            breathingInstance.release();
        }

        if (heartbeatInstance.isValid())
        {
            heartbeatInstance.stop(FMOD.Studio.STOP_MODE.IMMEDIATE);
            heartbeatInstance.release();
        }

        if (footstepCoroutine != null)
        {
            StopCoroutine(footstepCoroutine);
            footstepCoroutine = null;
        }
    }
}
