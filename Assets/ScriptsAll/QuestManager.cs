using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using FMODUnity;
using FMOD.Studio;

public class QuestManager : MonoBehaviour
{
    public static QuestManager instance;

    public Quest firstQuest;
    public Quest currentQuest;
    public QuestUI questUI;
    public GameObject umbrellaManNear;
    public GameObject umbrellaManFar;
    public GameObject gameOverUI;
    public EventReference knockSound;
    public EventReference questCompleteSound;
    public FollowLightController followLightController;
    public UmbrellaManChase chase;
    public RepairQTE repairQTE;
    public GameObject prototypeCompleteUI;
    public float prototypeCompleteDelay = 1.5f;
    public float musicFadeBeforeKnockDuration = 2f;
    public EnemyLightDistortion enemyLightDistortion;
    public EventReference umbrellaManAppearSound;
    public InteractableObject shieldInteractable;
    public LightFlickerController lightController;
    public PlayerController playerController;
    public Quest repairPanelQuest;

    private Dictionary<string, bool> placedItems = new Dictionary<string, bool>();

    void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }

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

        if (firstQuest != null)
        {
            StartQuest(firstQuest);
        } 
        else 
        {
            Debug.LogError("Первый квест не назначен в QuestManager!");
        }

        if (MusicManager.Instance != null)
        {
            MusicManager.Instance.SetSection("Value A");
            MusicManager.Instance.SetVolumeImmediate(1f);
        }
    }

    public void StartQuest(Quest questToStart)
    {
        if (questToStart == null) return;
        if (questUI == null)
        {
            Debug.LogError("QuestManager не может запустить квест: ссылка на questUI ПУСТАЯ!");
            return;
        }

        currentQuest = questToStart;
        currentQuest.isComplete = false;
        currentQuest.currentObjectiveIndex = 0;

        foreach(var obj in currentQuest.objectives)
        {
            obj.currentAmount = 0;
            obj.isComplete = false;
        }

        if(questToStart.questID == "Quest1_Placement")
            placedItems.Clear();

        Debug.Log("Начат квест: " + questToStart.questTitle);
        questUI.ShowQuestUpdate(currentQuest);
    }

    public void UpdateQuestProgress(string itemID_or_TargetID, ObjectiveType type)
    {
        if (currentQuest == null || currentQuest.isComplete) return;

        QuestObjective objective = currentQuest.GetCurrentObjective();
        if (objective == null || objective.isComplete) return;

        if (objective.objectiveType == ObjectiveType.Place && objective.targetID == "PlaceStuff")
        {
            if (!placedItems.ContainsKey(itemID_or_TargetID))
            {
                placedItems.Add(itemID_or_TargetID, true);
                objective.currentAmount = placedItems.Count;
                Debug.Log($"Предмет {itemID_or_TargetID} поставлен. Прогресс: {objective.currentAmount}/{objective.requiredAmount}");
                questUI.ShowQuestUpdate(currentQuest);

                if (objective.currentAmount >= objective.requiredAmount)
                {
                    CompleteCurrentObjective();
                }
            }
        }
        else if (objective.objectiveType == ObjectiveType.Interact && objective.targetID == itemID_or_TargetID)
        {
            Debug.Log($"Взаимодействие с {itemID_or_TargetID} засчитано.");
            CompleteCurrentObjective();
        }
    }

    void CompleteCurrentObjective()
    {
        if (currentQuest == null) return;
        QuestObjective objective = currentQuest.GetCurrentObjective();
        if(objective != null)
        {
            Debug.Log("Выполнена цель: " + objective.objectiveDescription);
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

        if (completedQuest.questID == "Quest1_Placement" || completedQuest.questID == "Quest2_Door")
        {
            if (!questCompleteSound.IsNull)
            {
                Debug.Log("[QuestManager] Playing questCompleteSound for quest: " + completedQuest.questID);
                RuntimeManager.PlayOneShot(questCompleteSound);
            }
            else
            {
                Debug.LogWarning("[QuestManager] questCompleteSound is null — assign it in the inspector.");
            }
        }

        TriggerQuestEvent(completedQuest.questID);

        if (completedQuest.nextQuest != null)
        {
            StartQuest(completedQuest.nextQuest);
        }
    }

    IEnumerator PlayKnockAfterFade(float fadeDuration)
    {
        if (MusicManager.Instance != null)
        {
            MusicManager.Instance.FadeToVolume(0f, fadeDuration);
        }
        yield return new WaitForSeconds(fadeDuration);
        if (!knockSound.IsNull)
        {
            RuntimeManager.PlayOneShot(knockSound);
        }
    }

    public void TriggerQuestEvent(string questID)
    {
        switch (questID)
        {
            case "Quest1_Placement":
                if (MusicManager.Instance != null)
                {
                    StartCoroutine(PlayKnockAfterFade(musicFadeBeforeKnockDuration));
                }
                else
                {
                    if(!knockSound.IsNull) RuntimeManager.PlayOneShot(knockSound);
                }
                break;

            case "Quest2_Door":
                if (followLightController != null)
                {
                    followLightController.StartSequence(this);
                    if (MusicManager.Instance != null) MusicManager.Instance.SetSection("Value C");
                    MusicManager.Instance.SetVolumeImmediate(1f);
                }
                break;

            case "Quest_FollowLight":
                break;

            case "Quest_RepairPanel":
                if (MusicManager.Instance != null) MusicManager.Instance.StopMusicImmediate();
            
                // ↓↓↓ ДОБАВЛЕНО: Запускаем квест, чтобы появился ТЕКСТ на экране ↓↓↓
                if (repairPanelQuest != null)
                {
                    StartQuest(repairPanelQuest); 
                }
            
                // ↓↓↓ ВАЖНО: Я отключил это здесь. QTE должно начаться, когда ты подойдешь
                // к щитку и нажмешь E, а не сразу, когда монстр появился.
                // if (repairQTE != null) repairQTE.StartRepairQTE(); 
                break;

            default:
                break;
        }
    }
    
    public void TriggerChaseScene()
    {
        StartChaseScene();
    }

    public void StartChaseScene()
    {
        StartCoroutine(ChaseSceneSequence());
    }

    IEnumerator ChaseSceneSequence()
    {
        yield return new WaitForSeconds(0.1f);

        if (MusicManager.Instance != null)
            MusicManager.Instance.SetSection("Value D");

        MusicManager.Instance.SetVolumeImmediate(1f);

        if (!umbrellaManAppearSound.IsNull)
            RuntimeManager.PlayOneShot(umbrellaManAppearSound);
        
        TriggerQuestEvent("Quest_RepairPanel");

        if (chase != null)
        {
            chase.gameObject.SetActive(true);

            if (enemyLightDistortion != null)
                enemyLightDistortion.SetChaseActive(true);
            if (shieldInteractable != null)
            {
                shieldInteractable.EnableShieldInteraction();
            }
            chase.StartChase();
        }
    }

    public void OnQTESuccess()
{
    Debug.Log("QTE Успех! (Финал 1)");

    // Завершаем квест на починку щитка
    if (repairPanelQuest != null && currentQuest == repairPanelQuest)
    {
        repairPanelQuest.isComplete = true; 
        CompleteQuest(repairPanelQuest); 
    }

    // Глушим музыку
    if (MusicManager.Instance != null)
    {
        MusicManager.Instance.FadeToVolume(0f, 1f);
    }

    // Останавливаем "дыхание" монстра (ближнего)
    if (umbrellaManNear)
    {
        var chase = umbrellaManNear.GetComponent<UmbrellaManChase>();
        if (chase != null)
            chase.StopBreathingLoop();
    }

    // Убираем ближнего монстра
    if (umbrellaManNear)
        umbrellaManNear.SetActive(false);

    // ВЫКЛЮЧАЕМ ВЕСЬ СВЕТ (И мигающий, и статичный)
    if (lightController != null)
        lightController.TurnOffAllLights();

    // Показываем дальнего монстра
    if (umbrellaManFar)
        umbrellaManFar.SetActive(true);

    // Запускаем финальную катсцену (поворот камеры)
    if (playerController)
    {
        playerController.enabled = false;
        playerController.StartCinematicPan(umbrellaManFar.transform, 4.0f);
    }

    // Показываем экран победы
    if (prototypeCompleteUI != null)
    {
        StartCoroutine(ShowCompletionScreenAfterDelay(6f));
    }
    else
    {
        Debug.LogError("Prototype Complete UI не назначен в QuestManager.");
    }
}

    public void OnQTEFailure()
    {
        if (enemyLightDistortion != null)
            enemyLightDistortion.SetChaseActive(false);
        if (MusicManager.Instance != null) MusicManager.Instance.StopMusicImmediate();
        Debug.Log("QTE FAILURE");
        if (repairQTE != null)
        {
            repairQTE.isQTEActive = false;
        }
        StartCoroutine(ShowGameOverAfterDelay(0.5f));
    }

    IEnumerator ShowCompletionScreenAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);

        if (prototypeCompleteUI != null)
        {
            prototypeCompleteUI.SetActive(true);
            Debug.Log("Финальная надпись 'Prototype complete' активирована.");
        }

        if (MusicManager.Instance != null)
        {
            MusicManager.Instance.SetSection("Value A");
            MusicManager.Instance.FadeToVolume(MusicManager.Instance.defaultVolume, 3f);
        }
    }

    IEnumerator ShowGameOverAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        if (gameOverUI != null) gameOverUI.SetActive(true);
        yield return null;
    }
}
