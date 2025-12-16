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
                // ❌ НЕ глушим музыку здесь!
                // Музыка Value D должна играть во время погони

                if (repairPanelQuest != null)
                    StartQuest(repairPanelQuest);
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
        {
            MusicManager.Instance.EnsureMusicPlaying();

            // ВАЖНО: сначала гарантируем, что музыка СЛЫШНА
            MusicManager.Instance.SetVolumeImmediate(1f);

            // ВАЖНО: потом переключаем секцию
            MusicManager.Instance.SetSection("Value D");
        }

        if (!umbrellaManAppearSound.IsNull)
            RuntimeManager.PlayOneShot(umbrellaManAppearSound);

        // ⚠️ Quest_RepairPanel больше НЕ глушит музыку
        TriggerQuestEvent("Quest_RepairPanel");

        if (chase != null)
        {
            chase.gameObject.SetActive(true);

            if (enemyLightDistortion != null)
                enemyLightDistortion.SetChaseActive(true);

            if (shieldInteractable != null)
                shieldInteractable.EnableShieldInteraction();

            chase.StartChase();
        }
    }

    public void OnQTESuccess()
    {
        Debug.Log("QTE Успех! (Финал)");

        if (MusicManager.Instance != null)
        {
            // 1️⃣ Уходим в тишину
            MusicManager.Instance.FadeToVolume(0f, 1f);

            // 2️⃣ ВАЖНО: СРАЗУ ЖЕ переключаемся на A
            // пока игрок НИЧЕГО не слышит
            MusicManager.Instance.SetSection("Value A");
        }

        // Останавливаем дыхание ближнего монстра
        if (umbrellaManNear)
        {
            var chase = umbrellaManNear.GetComponent<UmbrellaManChase>();
            if (chase != null)
                chase.StopBreathingLoop();
        }

        if (umbrellaManNear)
            umbrellaManNear.SetActive(false);

        // Гасим свет
        if (lightController != null)
            lightController.TurnOffAllLights();

        // Показываем дальнего человека с зонтом
        if (umbrellaManFar)
            umbrellaManFar.SetActive(true);

        // Камера — время посмотреть
        if (playerController)
        {
            playerController.enabled = false;
            playerController.StartCinematicPan(
                umbrellaManFar.transform,
                4.0f
            );
        }

        // Запускаем финальную режиссуру
        StartCoroutine(FinalSequenceAfterLook(6f));
    }

    public void OnQTEFailure()
    {
        if (enemyLightDistortion != null)
            enemyLightDistortion.SetChaseActive(false);

        // ❌ НЕ убиваем event
        if (MusicManager.Instance != null)
            MusicManager.Instance.FadeToVolume(0f, 0.5f);

        Debug.Log("QTE FAILURE");

        if (repairQTE != null)
            repairQTE.isQTEActive = false;

        StartCoroutine(ShowGameOverAfterDelay(0.5f));
    }
    IEnumerator FinalSequenceAfterLook(float delay)
    {
        yield return new WaitForSeconds(delay);

        if (prototypeCompleteUI != null)
            prototypeCompleteUI.SetActive(true);

        if (MusicManager.Instance != null)
        {
            // Просто поднимаем громкость
            MusicManager.Instance.FadeToVolume(
                MusicManager.Instance.defaultVolume,
                3f
            );
        }
    }

    IEnumerator ShowGameOverAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        if (gameOverUI != null) gameOverUI.SetActive(true);
        yield return null;
    }
}
