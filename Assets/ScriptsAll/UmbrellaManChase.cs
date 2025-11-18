using UnityEngine;
using UnityEngine.AI;

public class UmbrellaManChase : MonoBehaviour
{
    [Header("References")]
    public Transform player;            // Player (объект с PlayerController)

    [Header("Chase Settings")]
    public float walkSpeed = 1.5f;
    public float catchDistance = 1.0f;  // можно оставить как резерв

    [Header("Game Over UI")]
    public GameObject heGotYouUI;       // сюда перетащить объект с надписью "He got you" (панель/текст на Canvas)

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

        Debug.Log("[UmbrellaManChase] Погоня ЗАПУЩЕНА");
    }

    public void StopChase()
    {
        if (agent == null) return;

        isChasing = false;
        agent.isStopped = true;
        agent.enabled = false;

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
}
