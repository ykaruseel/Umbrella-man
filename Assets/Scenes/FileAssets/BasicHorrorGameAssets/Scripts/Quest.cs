// Файл: Quest.cs (Оставляем как было)
using UnityEngine;

[CreateAssetMenu(fileName = "New Quest", menuName = "Quest System/New Quest")]
public class Quest : ScriptableObject
{
    public string questID; // Добавим ID для удобства поиска
    public string questTitle;
    [TextArea(3, 10)]
    public string questDescription; // Не используется в UI, но полезно для описания
    public QuestObjective[] objectives;
    public bool isComplete = false;
    public Quest nextQuest; // Ссылка на следующий квест в цепочке

    [HideInInspector] public int currentObjectiveIndex = 0; // Отслеживаем текущую цель

    public bool CheckObjectives()
    {
        // В линейном квесте проверяем только текущую цель
        if (currentObjectiveIndex >= objectives.Length)
        {
             isComplete = true;
             return true;
        }
        return false;
        // Можно добавить проверку всех, если нужно будет
    }

    public QuestObjective GetCurrentObjective()
    {
        if (currentObjectiveIndex < objectives.Length)
        {
            return objectives[currentObjectiveIndex];
        }
        return null;
    }

     public void CompleteObjective()
     {
         if (currentObjectiveIndex < objectives.Length)
         {
             objectives[currentObjectiveIndex].isComplete = true;
             currentObjectiveIndex++;
         }
     }
}

[System.Serializable]
public class QuestObjective
{
    public string objectiveDescription; // "Поставить вазу на стол 0/4"
    public ObjectiveType objectiveType;
    public string targetID;             // ID цели ("Vase", "Door", "Panel")
    public int requiredAmount = 1;      // Для квеста 1 (сколько предметов)
    [HideInInspector] public int currentAmount = 0; // Текущее количество
    [HideInInspector] public bool isComplete = false; // Используем CompleteObjective для установки
}

public enum ObjectiveType
{
    Place,    
    Interact, 
    
}
