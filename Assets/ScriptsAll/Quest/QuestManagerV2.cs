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

        if (current.isActive && current.type == type)
        {
            current.CheckTarget(id);

            if (current.isCompleted)
            {
                StartCoroutine(ActivateNextQuest());
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

        //torze nado peredelat
        if(questSequence[currentQuestIndex].questID == "Q3")
        {
            if (playerController)
            {
                CharacterController cc = playerController.transform.GetComponent<CharacterController>();
                if (cc) cc.enabled = false;

                playerController.SetCanMove(false);
                playerController.isCinematic = true;

                playerController.transform.position = new Vector3(-22.77f, -7.87f, -1.53f);
                playerController.transform.rotation = Quaternion.Euler(0f, 180f, 0f);
                playerController.SetRotation(180f, 0f);

                QuestManager.instance.FadeOut(cc);
            }

            QuestEvents.Instance.VremennoQ3();
        }
    }
}
