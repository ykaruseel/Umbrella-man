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
    [SerializeField] private int currentQuestIndex = 0;

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
        Debug.Log($"<color=yellow>[Manager]</color> Выдача квеста: {questSequence[currentQuestIndex].questID}");
        questUI.ShowNewQuest(questSequence[currentQuestIndex]);

        
        if (TutorialManager.Instance != null)
        {
            TutorialManager.Instance.ShowHint(TutorialManager.HintType.Task_Q);
        }
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
        Debug.Log($"<color=yellow>[Manager]</color> �������� ��������: {id} ��� ���� {type} � ������ {current.questID}");
        if (current.isActive && current.type == type)
        {
            current.CheckTarget(id);

            if (current.isCompleted)
            {
                if (current.questID == "Q5")
                {
                    MusicManagerv2.Instance.SetMusicState(1);
                }

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
            Debug.Log($"<color=yellow>[Manager]</color> ��������� �����: {questSequence[currentQuestIndex].questID}");
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
                MusicManagerv2.Instance.StopMusic();
                QuestEvents.Instance.QuestEvent7();
                break;

            case "Q9":
                MusicManagerv2.Instance.StartMusic();
                MusicManagerv2.Instance.SetMusicState(4);
                StartCoroutine(QuestEvents.Instance.QuestEvent9());
                break;

            case "Q10":
                MusicManagerv2.Instance.SetMusicState(3);
                StartCoroutine(QuestEvents.Instance.QuestEvent10());
                break;

            case "Q11":
                MusicManagerv2.Instance.StopMusic();
                StartCoroutine(QuestEvents.Instance.QuestEvent11());
                break;
        }
    }

    public int GetCurrentQuest()
    {
        return currentQuestIndex;
    }

    public List<string> GetCompletedGoals()
    {
        if (currentQuestIndex < questSequence.Count)
        {
            return questSequence[currentQuestIndex].GetCompletedTargetsList();
        }
        return new List<string>();
    }

    public void SetQuestFromLoad(int index, List<string> completedGoals)
    {
        currentQuestIndex = index;

        for (int i = 0; i < questSequence.Count; i++)
        {
            questSequence[i].Initialize(i == currentQuestIndex);
        }

        if (currentQuestIndex < questSequence.Count)
        {
            questSequence[currentQuestIndex].RestoreProgress(completedGoals);
            questUI.ShowNewQuest(questSequence[currentQuestIndex]);
        }
    }
}
