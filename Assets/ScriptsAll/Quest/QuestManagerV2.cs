using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class QuestManagerV2 : MonoBehaviour
{
    public static QuestManagerV2 Instance;

    //Vremenno
    public PlayerController playerController;
    public QuestUIV2 questUI;

    [SerializeField] private List<QuestData> questSequence;
    private int currentQuestIndex = 0;

    private void Awake()
    {
        Instance = this;
        SetupQuests();
    }

    private void SetupQuests()
    {
        if (questSequence.Count == 0) return;

        for (int i = 0; i < questSequence.Count; i++)
        {
            questSequence[i].Initialize(i == 0);
        }
        Debug.Log($"<color=yellow>[Manager]</color> Следующий квест: {questSequence[currentQuestIndex].questID}");
        questUI.ShowNewQuest(questSequence[currentQuestIndex]);
    }

    public bool IsGoalRequired(string id, GoalType type)
    {
        if (currentQuestIndex >= questSequence.Count) return false;

        QuestData current = questSequence[currentQuestIndex];
        return current.isActive && current.type == type && current.targetID.Contains(id);
    }

    public void ProcessAction(string id, GoalType type)
    {
        if (currentQuestIndex >= questSequence.Count) return;

        QuestData current = questSequence[currentQuestIndex];
        Debug.Log($"<color=yellow>[Manager]</color> Получено действие: {id} для типа {type} в квесте {current.questID}");
        if (current.isActive && current.type == type)
        {
            current.CheckTarget(id);

            if (current.isCompleted)
            {
                Debug.Log($"<color=yellow>[Manager]</color> Квест {current.questID} завершён!");
                StartCoroutine(ActivateNextQuest());
            }
            else
            {
                questUI.UpdateProgressUI(current);
            }
        }
    }

    public bool IsQuestActive(string id)
    {
        QuestData current = questSequence[currentQuestIndex];
        if (id == current.questID) 
        {
            return true;
        }
        return false;
    }

    private IEnumerator ActivateNextQuest()
    {
        yield return null;

        currentQuestIndex++;
        if (currentQuestIndex < questSequence.Count)
        {
            questSequence[currentQuestIndex].isActive = true;
            Debug.Log($"<color=yellow>[Manager]</color> Следующий квест: {questSequence[currentQuestIndex].questID}");
            StartCoroutine(questUI.CompleteAndSwitchRoutine(questSequence[currentQuestIndex - 1], questSequence[currentQuestIndex]));
        }
        else
        {
            // All quests completed
        }

        switch (questSequence[currentQuestIndex].questID)
        {
            case "Q3":
                StartCoroutine(QuestEvents.Instance.QuestEvent3());
                break;

            case "Q5":
                QuestEvents.Instance.QuestEvent5();
                break;

            case "Q7":
                QuestEvents.Instance.QuestEvent7();
                break;

            case "Q9":
                StartCoroutine(QuestEvents.Instance.QuestEvent9());
                break;

            case "Q10":
                StartCoroutine(QuestEvents.Instance.QuestEvent10());
                break;

            case "Q11":
                StartCoroutine(QuestEvents.Instance.QuestEvent11());
                break;
        }
    }
}
