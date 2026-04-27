using UnityEngine;
using System.Collections.Generic;
using System;

[CreateAssetMenu(fileName = "New Quest", menuName = "Quests/Quest")]
public class QuestData : ScriptableObject
{
    public string title;
    public string questID;
    public GoalType type;
    public List<string> targetID;

    [NonSerialized] public bool isActive;
    [NonSerialized] public bool isCompleted;

    [NonSerialized] private HashSet<string> completedTargets = new HashSet<string>();

    public void Initialize(bool active)
    {
        isActive = active;
        isCompleted = false;
        completedTargets.Clear();
    }

    public void CheckTarget(string id)
    {
        if (!isActive || isCompleted) return;

        if (targetID.Contains(id) && !completedTargets.Contains(id))
        {
            completedTargets.Add(id);
        }

        if (completedTargets.Count >= targetID.Count)
        {
            Complete();
        }
    }

    private void Complete()
    {
        isCompleted = true;
        isActive = false;
    }

    public string GetTitleWithProgress()
    {
        if (type == GoalType.ReturnItem && targetID != null && targetID.Count > 1)
        {
            return $"{title} ({completedTargets.Count}/{targetID.Count})";
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
                completedTargets.Add(id);
            }
        }

        if (completedTargets.Count >= targetID.Count && targetID.Count > 0)
        {
            isCompleted = true;
            isActive = false;
        }
    }
}