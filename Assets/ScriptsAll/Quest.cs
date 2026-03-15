
using UnityEngine;

[CreateAssetMenu(fileName = "New Quest", menuName = "Quest System/New Quest")]
public class Quest : ScriptableObject
{
    public string questID;
    public string questTitle;
    [TextArea(3, 10)]
    public string questDescription;
    public QuestObjective[] objectives;
    public bool isComplete = false;
    public Quest nextQuest;

    [HideInInspector] public int currentObjectiveIndex = 0;

    public bool CheckObjectives()
    {
        
        if (currentObjectiveIndex >= objectives.Length)
        {
             isComplete = true;
             return true;
        }
        return false;
        
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
    public string objectiveDescription;
    public ObjectiveType objectiveType;
    public string targetID;    
    public int requiredAmount = 1;   
    [HideInInspector] public int currentAmount = 0;
    [HideInInspector] public bool isComplete = false;
}

public enum ObjectiveType
{
    Place,    
    Interact, 
    
}
