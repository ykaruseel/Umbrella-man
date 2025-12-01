using UnityEngine;
using UnityEngine.AI;

// FMOD
using FMODUnity;
using FMOD.Studio;

public class UmbrellaManChase : MonoBehaviour
{
    [Header("References")]
    public Transform player;            // Player (объект с PlayerController)

    [Header("Chase Settings")]
    public float walkSpeed = 1.5f;
    public float catchDistance = 1.0f;  // можно оставить как резерв

    [Header("Game Over UI")]
    public GameObject heGotYouUI;       // сюда перетащить объект с надписью "He got you"

    [Header("FMOD – дыхание человека с зонтом")]
    [Tooltip("FMOD Event, в котором крутится петля дыхания")]
    [SerializeField] private EventReference breathingLoopEvent;

    private EventInstance breathingInstance;   // экземпляр петли дыхания

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
        if (agent == null)
        {
            Debug.LogError("[UmbrellaManChase] StartChase() — нет агента");
            return;
        }

        if (player == null)
        {
            Debug.LogError("[UmbrellaManChase] StartChase() — не назначен player!");
            return;
        }

        // ставим агента на NavMesh
        NavMeshHit hit;
        if (NavMesh.SamplePosition(transform.position, out hit, 2f, NavMesh.AllAreas))
        {
            agent.Warp(hit.position);
        }
        else
        {
            Debug.LogError("[UmbrellaManChase] Не удалось найти NavMesh рядом с позицией UmbrellaMan!");
        }

        agent.enabled = true;
        agent.isStopped = false;
        agent.speed = walkSpeed;

        isChasing = true;
        hasCaughtPlayer = false;

        StartBreathingLoop();   // ← запускаем дыхание

        Debug.Log("[UmbrellaManChase] Погоня ЗАПУЩЕНА");
    }

    public void StopChase()
    {
        if (agent == null) return;

        isChasing = false;
        agent.isStopped = true;
        agent.enabled = false;

        StopBreathingLoop();    // ← останавливаем дыхание

        Debug.Log("[UmbrellaManChase] Погоня ОСТАНОВЛЕНА");
    }

    void Update()
    {
        if (!isChasing || hasCaughtPlayer) return;
        if (agent == null || !agent.enabled || player == null) return;

        agent.SetDestination(player.position);

        // резервный вариант, если вдруг триггер не сработал
        float distance = Vector3.Distance(transform.position, player.position);
        if (distance <= catchDistance)
        {
            HandleCatch();
        }
    }

    // ЛОВИМ ИГРОКА ЧЕРЕЗ ТРИГГЕР
    private void OnTriggerEnter(Collider other)
    {
        if (!isChasing || hasCaughtPlayer) return;

        // либо тот же объект, что в поле player,
        // либо любой объект с PlayerController
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

        StopBreathingLoop();    // ← при поимке тоже глушим дыхание

        Debug.Log("[UmbrellaManChase] Игрок пойман (He got you)");

        // --- ПОКАЗЫВАЕМ "He got you" ---
        if (heGotYouUI != null)
        {
            heGotYouUI.SetActive(true);
        }
        else
        {
            // запасной вариант — через QuestManager
            QuestManager qm = QuestManager.instance;
            if (qm != null && qm.gameOverUI != null)
                qm.gameOverUI.SetActive(true);
        }

        // Выключаем управление игроком
        if (player != null)
        {
            var pc = player.GetComponent<PlayerController>();
            if (pc != null)
                pc.enabled = false;
        }

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    // --- FMOD: дыхание ---

    private void StartBreathingLoop()
    {
        if (breathingLoopEvent.IsNull)
        {
            Debug.LogWarning("[UmbrellaManChase] breathingLoopEvent не назначен в инспекторе");
            return;
        }

        // если уже создан и играет — не создаём второй раз
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
            // привязываем 3D-звук к объекту UmbrellaMan
            var rb = GetComponent<Rigidbody>();
            RuntimeManager.AttachInstanceToGameObject(breathingInstance, transform, rb);
        }

        breathingInstance.start();
        Debug.Log("[UmbrellaManChase] Старт петли дыхания");
    }

    private void StopBreathingLoop()
    {
        if (!breathingInstance.isValid()) return;

        breathingInstance.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
        breathingInstance.release();
        breathingInstance.clearHandle();

        Debug.Log("[UmbrellaManChase] Стоп петли дыхания");
    }

    private void OnDestroy()
    {
        // на всякий случай чистим инстанс при уничтожении объекта
        if (breathingInstance.isValid())
        {
            breathingInstance.stop(FMOD.Studio.STOP_MODE.IMMEDIATE);
            breathingInstance.release();
        }
    }
}
