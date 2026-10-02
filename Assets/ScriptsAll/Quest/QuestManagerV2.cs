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

    [SerializeField]
    private bool autoStartQuestSequence = true;
    private bool sequenceStarted = false;

    private bool isAdvancingQuest = false;

    private void RequestQuestAdvance()
    {
        if (isAdvancingQuest)
            return;
        
        StartCoroutine(ActivateNextQuest());
    }


    public void AdvanceCompletedQuest()
    {
        if (!sequenceStarted)
            return;

        if (currentQuestIndex >= questSequence.Count)
            return;

        QuestData current =
            questSequence[currentQuestIndex];

        if (!current.isCompleted)
            return;

        RequestQuestAdvance();
    }

    private void Awake()
    {
        Instance = this;

        InitializeQuests();

        if (autoStartQuestSequence)
        {
            StartQuestSequence();
        }
    }

    private void InitializeQuests()
    {
        if (questSequence == null)
            return;

        foreach (QuestData quest in questSequence)
        {
            if (quest != null)
                quest.Initialize(false);
        }
    }


    public void StartQuestSequence()
    {
        if (sequenceStarted)
            return;

        if (questSequence == null ||
            questSequence.Count == 0)
            return;

        sequenceStarted = true;

        currentQuestIndex = 0;

        questSequence[currentQuestIndex].Initialize(true);

        questUI.ShowNewQuest(
            questSequence[currentQuestIndex]
        );
    }
    

    public bool IsGoalRequired(
    string id,
    GoalType type)
    {
        if (!sequenceStarted)
            return false;

        if (currentQuestIndex >= questSequence.Count)
            return false;

        QuestData current =
            questSequence[currentQuestIndex];

        return
            current.isActive &&
            current.type == type &&
            current.targetID.Contains(id);
    }

    public void ProcessAction(
    string id,
    GoalType type,
    bool deferQuestAdvance = false)
{
    if (!sequenceStarted)
        return;

    if (currentQuestIndex >= questSequence.Count)
        return;

    QuestData current =
        questSequence[currentQuestIndex];

    if (current.isActive &&
        current.type == type)
    {
        current.CheckTarget(id);

        questUI.UpdateProgressUI(current);

        if (current.isCompleted)
        {
            if (current.questID == "Q5")
            {
                MusicManagerv2.Instance.SetMusicState(1);
            }

            if (!deferQuestAdvance)
            {
                RequestQuestAdvance();
            }
        }
    }
}
    public bool IsQuestActive(string id)
    {
    if (!sequenceStarted)
        return false;

    if (currentQuestIndex < 0 ||
        currentQuestIndex >= questSequence.Count)
        return false;

    QuestData current =
        questSequence[currentQuestIndex];

    return
        current.isActive &&
        id == current.questID;
    }

  private IEnumerator ActivateNextQuest()
{
    isAdvancingQuest = true;

    yield return null;

    int completedQuestIndex =
        currentQuestIndex;

    currentQuestIndex++;

    if (currentQuestIndex >= questSequence.Count)
    {
        isAdvancingQuest = false;
        yield break;
    }

    QuestData completedQuest =
        questSequence[completedQuestIndex];

    QuestData nextQuest =
        questSequence[currentQuestIndex];

    nextQuest.isActive = true;

    RuntimeManager.PlayOneShot(
        questCompletedSound
    );

    StartCoroutine(
        questUI.CompleteAndSwitchRoutine(
            completedQuest,
            nextQuest
        )
    );

    switch (nextQuest.questID)
    {
        case "Q2":
            TutorialManager.Instance.ShowHint(
                HintType.Interact
            );
            break;

        case "Q3":
            StartCoroutine(
                QuestEvents.Instance.QuestEvent3()
            );
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

            StartCoroutine(
                QuestEvents.Instance.QuestEvent9()
            );
            break;

        case "Q10":
            MusicManagerv2.Instance.SetMusicState(3);

            StartCoroutine(
                QuestEvents.Instance.QuestEvent10()
            );
            break;

        case "Q11":
            MusicManagerv2.Instance.StopMusic();

            StartCoroutine(
                QuestEvents.Instance.QuestEvent11()
            );
            break;
    }

    isAdvancingQuest = false;
}

    public void SetCurrentQuestText(string text)
    {
        if (questUI != null)
        {
            questUI.SetQuestText(text);
        }
    }


    public void RefreshCurrentQuestUI()
    {
        if (!sequenceStarted)
            return;

        if (currentQuestIndex < 0 ||
            currentQuestIndex >= questSequence.Count)
            return;

        if (questUI != null)
        {
            questUI.UpdateProgressUI(
                questSequence[currentQuestIndex]
            );
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
        sequenceStarted = true;
        isAdvancingQuest = false;
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