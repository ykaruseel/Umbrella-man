using FMODUnity;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class QuestManagerV2 : MonoBehaviour
{
    public static QuestManagerV2 Instance;

    public PlayerController playerController;
    public QuestUIV2 questUI;

    [SerializeField] private List<QuestData> questSequence;
    [SerializeField] private int currentQuestIndex = 0;

    [SerializeField] private EventReference questCompletedSound;

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
            RuntimeManager.PlayOneShot(questCompletedSound);
            StartCoroutine(questUI.CompleteAndSwitchRoutine(questSequence[currentQuestIndex - 1], questSequence[currentQuestIndex]));
        }

        switch (questSequence[currentQuestIndex].questID)
        {
            case "Q2":
                TutorialManager.Instance.ShowHint(HintType.Interact);
                break;
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