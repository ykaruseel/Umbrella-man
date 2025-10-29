// Файл: QuestManager.cs
using UnityEngine;
using System.Collections.Generic;
using FMODUnity;

public class QuestManager : MonoBehaviour
{
    public static QuestManager instance;
    public QuestUI questUI;
    public Quest currentQuest;
    public Quest firstQuest;

    [Header("Системные ссылки")]
    public LightFlickerController lightController;
    public QTESystem qteSystem;
    public PlayerController playerController;
    public GameObject gameOverUI;
    public EventReference knockSound;

    [Header("FMOD")]
    public EventReference questCompleteSound;

    [Header("Заглушки")]
    public GameObject umbrellaManNear;
    public GameObject umbrellaManFar;

    private Dictionary<string, bool> placedItems = new Dictionary<string, bool>();

    // --- Добавленные поля для периодического стука ---
    private Coroutine knockRoutine;
    private bool doorInteracted = false;
    // --------------------------------------------------

    void Awake()
    {
        if (instance == null) instance = this;
        else { Destroy(gameObject); return; }

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
        if (firstQuest != null) StartQuest(firstQuest);
        else Debug.LogError("Первый квест не назначен в QuestManager!");
    }

    public void StartQuest(Quest questToStart)
    {
        currentQuest = questToStart;
        currentQuest.isComplete = false;
        currentQuest.currentObjectiveIndex = 0;

        foreach (var obj in currentQuest.objectives)
        {
            obj.currentAmount = 0;
            obj.isComplete = false;
        }

        placedItems.Clear();
        questUI.ShowQuestUpdate(currentQuest);
    }

    public void UpdateQuestProgress(string itemID_or_TargetID, ObjectiveType type)
    {
        if (currentQuest == null || currentQuest.isComplete) return;

        var objective = currentQuest.GetCurrentObjective();
        if (objective == null || objective.isComplete) return;

        if (objective.objectiveType == ObjectiveType.Place && objective.targetID == "PlaceStuff")
        {
            if (!placedItems.ContainsKey(itemID_or_TargetID))
            {
                placedItems.Add(itemID_or_TargetID, true);
                objective.currentAmount = placedItems.Count;
                questUI.ShowQuestUpdate(currentQuest);

                if (objective.currentAmount >= objective.requiredAmount)
                    CompleteCurrentObjective();
            }
        }
        else if (objective.objectiveType == ObjectiveType.Interact && objective.targetID == itemID_or_TargetID)
        {
            CompleteCurrentObjective();
        }
    }

    void CompleteCurrentObjective()
    {
        var objective = currentQuest.GetCurrentObjective();
        if (objective != null)
        {
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

        // ---- Звук при выполнении квеста ----
        if (!questCompleteSound.IsNull)
            RuntimeManager.PlayOneShot(questCompleteSound);
        // ------------------------------------

        TriggerQuestEvent(completedQuest.questID);

        if (completedQuest.nextQuest != null)
            Invoke("StartNextQuest", 4f);
        else
            currentQuest = null;
    }

    void StartNextQuest()
    {
        if (currentQuest != null && currentQuest.nextQuest != null)
            StartQuest(currentQuest.nextQuest);
    }

    void TriggerQuestEvent(string questID)
    {
        switch (questID)
        {
            case "Quest1_Placement":
                if (!knockSound.IsNull)
                {
                    // Первый стук
                    RuntimeManager.PlayOneShot(knockSound);

                    // Повторяющийся стук, пока игрок не взаимодействует с дверью
                    if (knockRoutine == null)
                        knockRoutine = StartCoroutine(RepeatKnockUntilDoorInteraction(10f)); // интервал 10 секунд
                }
                break;

            case "Quest2_Door":
                // Игрок взаимодействовал с дверью — останавливаем стук
                OnDoorInteracted();

                if (lightController != null)
                    StartCoroutine(lightController.FlickerSequence(StartQuest3));
                break;

            case "Quest3_Panel":
                if (qteSystem != null)
                    qteSystem.StartQTE(3f, KeyCode.E, OnQTESuccess, OnQTEFailure);
                break;
        }
    }

    void StartQuest3() { }

    void OnQTESuccess()
    {
        if (lightController) lightController.TurnOffAllLights();
        if (umbrellaManFar) umbrellaManFar.SetActive(true);
        if (playerController) playerController.enabled = false;
    }

    void OnQTEFailure()
    {
        if (lightController) lightController.MaxOutLights();
        if (umbrellaManNear) StartCoroutine(ShowUmbrellaManNearBriefly(1f));
        if (gameOverUI) gameOverUI.SetActive(true);
        if (playerController) playerController.enabled = false;
    }

    System.Collections.IEnumerator ShowUmbrellaManNearBriefly(float duration)
    {
        umbrellaManNear.SetActive(true);
        yield return new WaitForSeconds(duration);
        umbrellaManNear.SetActive(false);
    }

    // --- Повторяющийся стук в дверь ---
    private System.Collections.IEnumerator RepeatKnockUntilDoorInteraction(float interval)
    {
        yield return new WaitForSeconds(interval);

        while (!doorInteracted)
        {
            RuntimeManager.PlayOneShot(knockSound);
            yield return new WaitForSeconds(interval);
        }

        knockRoutine = null;
    }

    public void OnDoorInteracted()
    {
        doorInteracted = true;

        if (knockRoutine != null)
        {
            StopCoroutine(knockRoutine);
            knockRoutine = null;
        }
    }
}


