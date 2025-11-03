using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using FMODUnity;

public class QuestManager : MonoBehaviour
{
    public static QuestManager instance;

    [Header("Quest System")]
    public QuestUI questUI;
    public Quest currentQuest;
    public Quest firstQuest;

    [Header("Gameplay References")]
    public LightFlickerController lightController;
    public PlayerController playerController;
    public QTESystem qteSystem;
    public GameObject gameOverUI;

    [Header("Umbrella Man")]
    public GameObject umbrellaManNear;
    public GameObject umbrellaManFar;

    [Header("FMOD Events")]
    [SerializeField] private EventReference knockSound;
    [SerializeField] private EventReference questCompleteSFX;
    [SerializeField] private EventReference musicEvent;

    private FMOD.Studio.EventInstance musicInstance;
    private FMOD.Studio.PARAMETER_ID musicParamID;

    private Dictionary<string, bool> placedItems = new Dictionary<string, bool>();

    void Awake()
    {
        if (instance == null)
            instance = this;
        else { Destroy(gameObject); return; }

        if (questUI == null)
        {
            questUI = FindObjectOfType<QuestUI>();
            if (questUI == null)
                Debug.LogError("QuestManager не нашёл QuestUI в сцене!");
        }
    }

    void Start()
    {
        if (umbrellaManNear) umbrellaManNear.SetActive(false);
        if (umbrellaManFar) umbrellaManFar.SetActive(false);
        if (gameOverUI) gameOverUI.SetActive(false);

        InitMusic();

        if (firstQuest != null)
            StartQuest(firstQuest);
        else
            Debug.LogError("Первый квест не назначен в QuestManager!");
    }

    private void InitMusic()
    {
        if (!musicEvent.IsNull)
        {
            musicInstance = RuntimeManager.CreateInstance(musicEvent);
            musicInstance.start();

            FMOD.Studio.EventDescription desc;
            musicInstance.getDescription(out desc);
            FMOD.Studio.PARAMETER_DESCRIPTION paramDesc;
            desc.getParameterDescriptionByName("MusicSwitch", out paramDesc);
            musicParamID = paramDesc.id;

            SetMusicSection(0f); // A
            Debug.Log("🎵 Музыка: секция A");
        }
    }

    public void StartQuest(Quest questToStart)
    {
        if (questToStart == null) return;

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
        Debug.Log("Начат квест: " + questToStart.questTitle);
    }

    public void UpdateQuestProgress(string targetID, ObjectiveType type)
    {
        if (currentQuest == null || currentQuest.isComplete) return;
        QuestObjective objective = currentQuest.GetCurrentObjective();
        if (objective == null || objective.isComplete) return;

        // Квест 1: сбор предметов
        if (objective.objectiveType == ObjectiveType.Place && objective.targetID == "PlaceStuff")
        {
            if (!placedItems.ContainsKey(targetID))
            {
                placedItems.Add(targetID, true);
                objective.currentAmount = placedItems.Count;
                questUI.ShowQuestUpdate(currentQuest);

                if (objective.currentAmount >= objective.requiredAmount)
                    CompleteCurrentObjective();
            }
        }
        // Квест 2–3: взаимодействие
        else if (objective.objectiveType == ObjectiveType.Interact && objective.targetID == targetID)
        {
            CompleteCurrentObjective();
        }
    }

    void CompleteCurrentObjective()
    {
        QuestObjective objective = currentQuest.GetCurrentObjective();
        if (objective != null)
        {
            objective.isComplete = true;
            currentQuest.CompleteObjective();
        }

        if (currentQuest.CheckObjectives())
            CompleteQuest(currentQuest);
        else
            questUI.ShowQuestUpdate(currentQuest);
    }

    void CompleteQuest(Quest completedQuest)
    {
        questUI.ShowQuestCompleted(completedQuest);
        if (!questCompleteSFX.IsNull)
            RuntimeManager.PlayOneShot(questCompleteSFX);

        TriggerQuestEvent(completedQuest.questID);

        if (completedQuest.nextQuest != null)
            Invoke(nameof(StartNextQuest), 4f);
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
                    RuntimeManager.PlayOneShot(knockSound);
                SetMusicSection(1f); // B
                break;

            case "Quest2_Door":
                StartCoroutine(HandleLightAndMusicSequence());
                break;

            case "Quest3_Panel":
                if (qteSystem != null)
                    qteSystem.StartQTE(3f, KeyCode.E, OnQTESuccess, OnQTEFailure);
                break;
        }
    }

    // --- Мигание света + музыка ---
    private IEnumerator HandleLightAndMusicSequence()
    {
        SetMusicSection(2f); // C
        Debug.Log("🎵 Музыка: секция C — начало мигания");

        if (lightController)
            StartCoroutine(lightController.FlickerSequence(null));

        // Секция C играет дольше — 8 секунд
        yield return new WaitForSeconds(6f);

        // Переключаемся на секцию D — пик напряжения
        SetMusicSection(3f);
        Debug.Log("🎵 Музыка: секция D — пик напряжения (ожидание)");

        // Ждём 8 секунд на секции D перед появлением монстра
        yield return new WaitForSeconds(12f);

        // После этого появляется монстр и Game Over
        StartCoroutine(TriggerUmbrellaManDeath());
    }


    // --- Скример и моментальный Game Over ---
    public IEnumerator TriggerUmbrellaManDeath()
    {
        Debug.Log("💀 Появляется человек с зонтом");

        if (lightController) lightController.MaxOutLights();
        yield return new WaitForSeconds(0.3f);

        if (umbrellaManNear != null) umbrellaManNear.SetActive(true);

        // Мгновенно включаем Game Over экран
        if (gameOverUI != null)
        {
            gameOverUI.SetActive(true);
            CanvasGroup cg = gameOverUI.GetComponent<CanvasGroup>();
            if (cg == null) cg = gameOverUI.AddComponent<CanvasGroup>();
            cg.alpha = 0f;

            float t = 0f;
            float duration = 1.5f;
            while (t < duration)
            {
                cg.alpha = Mathf.Lerp(0f, 1f, t / duration);
                t += Time.deltaTime;
                yield return null;
            }
            cg.alpha = 1f;
        }

        // Свет полностью гаснет
        if (lightController) lightController.TurnOffAllLights();
        if (playerController) playerController.enabled = false;

        // Через пару секунд — возвращаем музыку в секцию A
        yield return new WaitForSeconds(2f);
        SetMusicSection(0f);
        Debug.Log("🎵 Музыка: секция A — после Game Over");
    }

    // --- QTE успех (на будущее) ---
    public void OnQTESuccess()
    {
        Debug.Log("✅ QTE Успех — игрок спасся");
        if (lightController) lightController.TurnOffAllLights();
        if (umbrellaManFar) umbrellaManFar.SetActive(true);
        if (playerController) playerController.LockMovementButAllowLook();
    }

    public void OnQTEFailure()
    {
        Debug.Log("❌ QTE Провал — скример");
        StartCoroutine(TriggerUmbrellaManDeath());
    }

    private void SetMusicSection(float value)
    {
        if (musicInstance.isValid())
            musicInstance.setParameterByID(musicParamID, value);
    }
}
