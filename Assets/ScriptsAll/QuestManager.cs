// Файл: QuestManager.cs (ПОЛНАЯ ЧИСТАЯ ВЕРСИЯ ДЛЯ ПОГОНИ)
using UnityEngine;
using System.Collections; // <-- ВОТ ИСПРАВЛЕНИЕ
using System.Collections.Generic;
using FMODUnity; // Можешь закомментировать, если нет FMOD

public class QuestManager : MonoBehaviour
{
    public static QuestManager instance;
    public QuestUI questUI; // Поле для "Лица" (QuestPanel)
    public Quest currentQuest;
    public Quest firstQuest;
    
    [Header("Квесты по цепочке")]
    public Quest quest_FollowLight; // Ассет "Иди за светом"
    public Quest quest_RepairPanel; // Ассет "Почини свет"
    public FollowLightController followLightController; 

    [Header("Системные ссылки")]
    public LightFlickerController lightController;
    public QTESystem qteSystem;
    public PlayerController playerController;
    public InteractableObject shieldInteractable;
    public GameObject gameOverUI;
    public EventReference knockSound;
    public EventReference questCompleteSound; // Звук завершения
    public EnemyLightDistortion enemyLightDistortion;

    [Header("Заглушки")]
    public GameObject umbrellaManNear;
    public GameObject umbrellaManFar;
    
    private Dictionary<string, bool> placedItems = new Dictionary<string, bool>();
    [Header("FMOD")]
    [SerializeField] private EventReference umbrellaAppearEvent;

    void Awake()
    {
        // Настройка Singleton
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(gameObject);
            return; 
        }

        // "Костыль": если мы ЗАБЫЛИ перетащить QuestUI
        if (questUI == null)
        {
            questUI = FindObjectOfType<QuestUI>();
            if (questUI == null)
                Debug.LogError("FATAL: QuestManager не смог найти QuestUI в сцене!");
        }
    }

    void Start()
    {
        if (umbrellaManNear) umbrellaManNear.SetActive(false);
        if (umbrellaManFar) umbrellaManFar.SetActive(false);
        if (gameOverUI) gameOverUI.SetActive(false);

        if (firstQuest != null)
        {
            StartQuest(firstQuest);
        } else {
            Debug.LogError("Первый квест не назначен в QuestManager!");
        }
    }

    public void StartQuest(Quest questToStart)
    {
        if (questToStart == null) return;
        if (questUI == null)
        {
            Debug.LogError("QuestManager не может запустить квест: ссылка на questUI ПУСТАЯ!");
            return;
        }

        currentQuest = questToStart;
        currentQuest.isComplete = false;
        currentQuest.currentObjectiveIndex = 0;
        
        foreach(var obj in currentQuest.objectives)
        {
            obj.currentAmount = 0;
            obj.isComplete = false;
        }

        // Очищаем placedItems ТОЛЬКО для квеста 1
        if(questToStart.questID == "Quest1_Placement")
            placedItems.Clear();

        Debug.Log("Начат квест: " + questToStart.questTitle);
        questUI.ShowQuestUpdate(currentQuest);
    }

    // UpdateQuestProgress (для Квеста 1 и Щитка)
    public void UpdateQuestProgress(string itemID_or_TargetID, ObjectiveType type)
    {
        if (currentQuest == null || currentQuest.isComplete) return;

        QuestObjective objective = currentQuest.GetCurrentObjective();
        if (objective == null || objective.isComplete) return;

        // --- ЛОГИКА ДЛЯ КВЕСТА 1 ("PlaceStuff 0/4") ---
        if (objective.objectiveType == ObjectiveType.Place && objective.targetID == "PlaceStuff")
        {
            if (!placedItems.ContainsKey(itemID_or_TargetID))
            {
                placedItems.Add(itemID_or_TargetID, true); 
                objective.currentAmount = placedItems.Count; 
                
                Debug.Log($"Предмет {itemID_or_TargetID} поставлен. Прогресс: {objective.currentAmount}/{objective.requiredAmount}");
                
                questUI.ShowQuestUpdate(currentQuest); 

                if (objective.currentAmount >= objective.requiredAmount)
                {
                    CompleteCurrentObjective();
                }
            }
        }
        // --- ЛОГИКА ДЛЯ ДВЕРИ (Квест 2) и ЩИТКА (Квест "Repair") ---
        else if (objective.objectiveType == ObjectiveType.Interact && objective.targetID == itemID_or_TargetID)
        {
             Debug.Log($"Взаимодействие с {itemID_or_TargetID} засчитано.");
             CompleteCurrentObjective();
        }
    }

    void CompleteCurrentObjective()
    {
         if (currentQuest == null) return;
         QuestObjective objective = currentQuest.GetCurrentObjective();
         if(objective != null)
         {
            Debug.Log("Выполнена цель: " + objective.objectiveDescription);
            currentQuest.CompleteObjective();
         }

        if (currentQuest.CheckObjectives())
        {
            CompleteQuest(currentQuest);
        }
        else
        {
             questUI.ShowQuestUpdate(currentQuest);
        }
    }

    // Завершает ВЕСЬ квест
    void CompleteQuest(Quest completedQuest)
    {
        Debug.Log("КВЕСТ ВЫПОЛНЕН: " + completedQuest.questTitle);
        questUI.ShowQuestCompleted(completedQuest); 

        // Проигрываем звук, ЕСЛИ это Квест 2 (Дверь)
        if (completedQuest.questID == "Quest2_Door") 
        {
            if (!questCompleteSound.IsNull)
                RuntimeManager.PlayOneShot(questCompleteSound); 
        }

        // Запускаем события (стук, мигание и т.д.)
        TriggerQuestEvent(completedQuest.questID);

        // Запускаем следующий квест
        if (completedQuest.nextQuest != null)
        {
             // *ВАЖНО*: Не запускаем следующий квест по таймеру,
             // если это Квест 2 (т.к. его запустит FollowLightController)
             if(completedQuest.questID != "Quest2_Door")
             {
                Invoke("StartNextQuest", 4f);
             }
        }
        else
        {
             currentQuest = null;
        }
    }

     void StartNextQuest()
     {
         if (currentQuest != null && currentQuest.nextQuest != null)
         {
             StartQuest(currentQuest.nextQuest);
         }
     }

    // --- НОВЫЙ TriggerQuestEvent ДЛЯ ПОГОНИ ---
    void TriggerQuestEvent(string questID)
    {
        switch (questID)
        {
            case "Quest1_Placement":
                // 1. Квест 1 закончен -> Стук
                if(!knockSound.IsNull) RuntimeManager.PlayOneShot(knockSound);
                Debug.Log("Играет звук стука...");
                break;

            case "Quest2_Door":
                // 2. Диалог с дверью закончен -> Запуск "Иди за светом"
                Debug.Log("Диалог с дверью закончен. Запуск Квеста 'Иди за светом'");
                
                StartQuest(quest_FollowLight); 
                
                if(followLightController)
                    followLightController.StartSequence(this);
                else
                    Debug.LogError("FollowLightController не назначен в QuestManager!");
                break;

            case "Quest_RepairPanel": // <-- ИСПОЛЬЗУЙ ЭТОТ ID в ассете Квеста 3
                // 4. Игрок нажал на щиток (после погони)
                Debug.Log("Нажат щиток. Запуск QTE...");

                // Останавливаем погоню
                if (umbrellaManNear && umbrellaManNear.activeInHierarchy)
                {
                    var chase = umbrellaManNear.GetComponent<UmbrellaManChase>();
                    if (chase != null)
                        chase.StopChase();
                }

                // Останавливаем пульсацию света
                if (lightController) 
                    lightController.StopPulsingFlicker();

                // Запускаем QTE
                if (qteSystem != null)
                {
                    qteSystem.StartQTE(3f, KeyCode.E, OnQTESuccess, OnQTEFailure);
                } else { Debug.LogError("QTESystem не назначен!"); }
                break;
        }
    }
    
    // --- НОВЫЙ МЕТОД (Вызывается из FollowLightController) ---
    // 3. Запускает сцену погони (после последней лампы)
    public void TriggerChaseScene()
    {
        StartCoroutine(ChaseSceneSequence());
    }

    IEnumerator ChaseSceneSequence()
    {
        Debug.Log("Последняя лампа погасла. Появление!");

        if (umbrellaManNear)
        {
            umbrellaManNear.SetActive(true);

            // ▶ ЗВУК ПОЯВЛЕНИЯ
            if (!umbrellaAppearEvent.IsNull)
            {
                RuntimeManager.PlayOneShot(umbrellaAppearEvent, umbrellaManNear.transform.position);
            }
        }
        
        // 👇 НОВОЕ: Разблокируем щиток СРАЗУ после появления "зонта"
        if (shieldInteractable != null)
        {
            shieldInteractable.EnableShieldInteraction();
            Debug.Log("Щиток разблокирован.");
        }

        // даём игроку 2 секунды увидеть его
        yield return new WaitForSeconds(2f);

        // запускаем погоню (как у тебя было)
        if (umbrellaManNear)
        {
            var chase = umbrellaManNear.GetComponent<UmbrellaManChase>();
            if (chase != null)
                chase.StartChase();
            else
                Debug.LogWarning("На umbrellaManNear нет UmbrellaManChase!");
        }

        // включаем пульсацию света, если привязан EnemyLightDistortion
        if (enemyLightDistortion != null)
            enemyLightDistortion.SetChaseActive(true);
    }



    // --- НОВЫЕ КОНЦОВКИ QTE ---
    public void OnQTESuccess()
    {
        Debug.Log("QTE Успех! (Финал 1)");

        if(umbrellaManNear) 
            umbrellaManNear.SetActive(false); // Прячем ближнюю фигуру

        if(lightController) 
            lightController.TurnOffAllLights(); // Гасим свет

        if(umbrellaManFar) 
            umbrellaManFar.SetActive(true); // Показываем дальнюю фигуру

        if(playerController) 
        {
            // HARD FREEZE (отключаем скрипт, чтобы не ходил)
            playerController.enabled = false; 

            // ↓↓↓ ЗАПУСКАЕМ ПЛАВНЫЙ ПОВОРОТ КАМЕРЫ ↓↓↓
            playerController.StartCinematicPan(umbrellaManFar.transform, 4.0f); // Поворачиваем за 4 секунды
        }
    }

    public void OnQTEFailure()
    {
        Debug.Log("QTE Провал! (Финал 2)");
        
        if(lightController) 
            lightController.MaxOutLights();
            
        if(gameOverUI) 
            gameOverUI.SetActive(true);
            
        if(playerController) 
            playerController.enabled = false;
    }

     System.Collections.IEnumerator ShowUmbrellaManNearBriefly(float duration)
     {
         umbrellaManNear.SetActive(true);
         yield return new WaitForSeconds(duration);
         umbrellaManNear.SetActive(false);
     }
}
