using UnityEngine;
using System.Collections.Generic;
using System;

[CreateAssetMenu(fileName = "New Quest", menuName = "Quests/Quest")]
public class QuestData : ScriptableObject
{
    public string title;
    public string questID;
    public GoalType type;

    [Header("Targets")]
    public List<string> targetID;

    [Header("Collection Settings")]
    [Min(1)]
    public int requiredAmount = 1;

    [Header("UI")]
    public bool keepVisibleWhileActive = false;

    [NonSerialized] public bool isActive;
    [NonSerialized] public bool isCompleted;

    [NonSerialized]
    private HashSet<string> completedTargets = new HashSet<string>();


    public void Initialize(bool active)
    {
        isActive = active;
        isCompleted = false;
        completedTargets.Clear();
    }


    public void CheckTarget(string id)
    {
        if (!isActive || isCompleted)
            return;

        if (targetID == null || !targetID.Contains(id))
            return;

        if (!completedTargets.Contains(id))
        {
            completedTargets.Add(id);
        }

        if (completedTargets.Count >= GetRequiredAmount())
        {
            Complete();
        }
    }


    private void Complete()
    {
        isCompleted = true;
        isActive = false;
    }


    public int GetCurrentAmount()
    {
        return completedTargets.Count;
    }


    public int GetRequiredAmount()
    {
        if (type == GoalType.CollectItems)
        {
            return Mathf.Max(1, requiredAmount);
        }

        return targetID != null ? targetID.Count : 0;
    }


    public string GetTitleWithProgress()
    {
        bool showProgress =
            type == GoalType.CollectItems ||
            (type == GoalType.ReturnItem &&
             targetID != null &&
             targetID.Count > 1);

        if (showProgress)
        {
            return $"{title} ({GetCurrentAmount()}/{GetRequiredAmount()})";
        }

        return title;
    }


    public List<string> GetCompletedTargetsList()
    {
        return new List<string>(completedTargets);
    }


    public void RestoreProgress(List<string> savedGoals)
    {
        completedTargets.Clear();

        if (savedGoals != null)
        {
            foreach (string id in savedGoals)
            {
                if (targetID.Contains(id))
                    completedTargets.Add(id);
            }
        }

        if (completedTargets.Count >= GetRequiredAmount())
        {
            isCompleted = true;
            isActive = false;
        }
    }


#if UNITY_EDITOR
    private void OnValidate()
    {
        if (type == GoalType.CollectItems &&
            targetID != null &&
            targetID.Count > 0)
        {
            requiredAmount =
                Mathf.Clamp(requiredAmount, 1, targetID.Count);
        }
    }
#endif
}