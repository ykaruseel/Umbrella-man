// Файл: QuestManager.cs (ЧИСТАЯ ВЕРСИЯ)
using UnityEngine;
using System.Collections.Generic;
using FMODUnity; // Можешь закомментировать, если нет FMOD

public class QuestManager : MonoBehaviour
{
    public static QuestManager instance;
    public QuestUI questUI; // Поле для "Лица" (QuestPanel)
    public Quest currentQuest;
    public Quest firstQuest;

    [Header("Системные ссылки")]
    public LightFlickerController lightController;
    public QTESystem qteSystem;
    public PlayerController playerController;
    public GameObject gameOverUI;
    public EventReference knockSound;
    
    [Header("Заглушки")]
    public GameObject umbrellaManNear;
    public GameObject umbrellaManFar;
    
    private Dictionary<string, bool> placedItems = new Dictionary<string, bool>();

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
            return; // Важно: выходим, если мы дубликат
        }

        // "Костыль": если мы ЗАБЫЛИ перетащить QuestUI в инспектор, ищем его кодом
        // Но это сработает, только если QuestPanel ВКЛЮЧЕНА при старте
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

        placedItems.Clear();

        Debug.Log("Начат квест: " + questToStart.questTitle);
        questUI.ShowQuestUpdate(currentQuest); // <-- Здесь была ошибка NullReference
    }

    // ИСПРАВЛЕННАЯ ВЕРСИЯ (только ОДНА)
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
                
                questUI.ShowQuestUpdate(currentQuest); // <-- Здесь была ошибка NullReference

                if (objective.currentAmount >= objective.requiredAmount)
                {
                    CompleteCurrentObjective();
                }
            }
        }
        // --- ЛОГИКА ДЛЯ КВЕСТА 2 и 3 ("Door", "Panel") ---
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

    void CompleteQuest(Quest completedQuest)
    {
        Debug.Log("КВЕСТ ВЫПОЛНЕН: " + completedQuest.questTitle);
        questUI.ShowQuestCompleted(completedQuest); 

        TriggerQuestEvent(completedQuest.questID);

        if (completedQuest.nextQuest != null)
        {
             Invoke("StartNextQuest", 4f);
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

     void TriggerQuestEvent(string questID)
     {
         switch (questID)
         {
             case "Quest1_Placement":
                 // Играем звук стука
                 if(!knockSound.IsNull) RuntimeManager.PlayOneShot(knockSound);
                 Debug.Log("Играет звук стука...");
                 break;

             case "Quest2_Door":
                 Debug.Log("Попытка запустить мигание света..."); // Лог

                 if (lightController != null) 
                 {
                     // Передаем StartNextQuest (чтобы запустить Квест 3)
                     StartCoroutine(lightController.FlickerSequence(StartNextQuest)); 
                 }
                 else 
                 {
                     Debug.LogError("LightFlickerController не назначен!");
                     StartNextQuest(); 
                 }
                 break;

             case "Quest3_Panel":
                 // Запускаем QTE
                 Debug.Log("Запуск QTE...");
                 if (qteSystem != null)
                 {
                     qteSystem.StartQTE(3f, KeyCode.E, OnQTESuccess, OnQTEFailure);
                 } else { Debug.LogError("QTESystem не назначен!"); }
                 break;
         }
     }
    
    void StartQuest3() { }

    void OnQTESuccess()
    {
        Debug.Log("QTE Успех!");
        if(lightController) lightController.TurnOffAllLights();
        if(umbrellaManFar) umbrellaManFar.SetActive(true);
        if(playerController) playerController.enabled = false; 
    }

    void OnQTEFailure()
    {
        Debug.Log("QTE Провал!");
        if(lightController) lightController.MaxOutLights();
        if(umbrellaManNear) StartCoroutine(ShowUmbrellaManNearBriefly(1f));
        if(gameOverUI) gameOverUI.SetActive(true);
        if(playerController) playerController.enabled = false;
    }

     System.Collections.IEnumerator ShowUmbrellaManNearBriefly(float duration)
     {
         umbrellaManNear.SetActive(true);
         yield return new WaitForSeconds(duration);
         umbrellaManNear.SetActive(false);
     }
}
