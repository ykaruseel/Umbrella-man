// Файл: UmbrellaMan_AI.cs
using UnityEngine;
using UnityEngine.AI; // Не забудь добавить AI

[RequireComponent(typeof(NavMeshAgent))]
public class UmbrellaMan_AI : MonoBehaviour
{
    private NavMeshAgent agent;
    private Transform playerTarget;
    private bool isChasing = false;

    void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        agent.enabled = false; // Выключен по умолчанию
    }

    // 1. Вызывается из QuestManager, чтобы НАЧАТЬ погоню
    public void StartChase(Transform target)
    {
        playerTarget = target;
        agent.enabled = true;
        isChasing = true;
    }

    // 2. Вызывается из QuestManager, чтобы ОСТАНОВИТЬ погоню (перед QTE)
    public void StopChase()
    {
        isChasing = false;
        if(agent.isOnNavMesh) // Проверка, что он на сетке
            agent.enabled = false;
    }

    void Update()
    {
        // Если погоня активна и цель есть
        if (isChasing && playerTarget != null && agent.enabled)
        {
            // Постоянно обновляем цель (положение игрока)
            agent.SetDestination(playerTarget.position);
        }
    }
}
