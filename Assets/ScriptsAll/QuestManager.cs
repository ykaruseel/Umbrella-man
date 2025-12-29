using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using FMODUnity;
using FMOD.Studio;
using UnityEngine.UI;
using TMPro; // ✅ ДОБАВЛЕНО: Нужно для работы с текстом

public class QuestManager : MonoBehaviour
{
    public static QuestManager instance;

    [Header("Quests")]
    public Quest firstQuest;
    public Quest currentQuest;
    public QuestUI questUI; // Это ссылка на скрипт QuestUI
    public Quest repairPanelQuest;

    [Header("Game Objects")]
    public GameObject umbrellaManNear;
    public GameObject umbrellaManFar;
    public GameObject gameOverUI;
    public GameObject prototypeCompleteUI;
    
    [Header("Components")]
    public FollowLightController followLightController;
    public UmbrellaManChase chase;
    public RepairQTE repairQTE;
    public EnemyLightDistortion enemyLightDistortion;
    public InteractableObject shieldInteractable;
    public LightFlickerController lightController;
    public PlayerController playerController;

    [Header("Audio")]
    public EventReference knockSound;
    public EventReference questCompleteSound;
    public EventReference umbrellaManAppearSound;
    public float musicFadeBeforeKnockDuration = 2f;

    [SerializeField] private Image fadeImage;
    [SerializeField] private float fadeDuration;

    [Header("Final Scene")]
    public GameObject finalSpotlight;

    [Header("Settings")]
    public float prototypeCompleteDelay = 1.5f;

    private Dictionary<string, bool> placedItems = new Dictionary<string, bool>();

    void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        if (questUI == null) questUI = FindObjectOfType<QuestUI>();
    }

    void Start()
    {
        if (umbrellaManNear) umbrellaManNear.SetActive(false);
        if (umbrellaManFar) umbrellaManFar.SetActive(false);
        
        // ✅ ГАРАНТИЯ: Выключаем оба экрана при старте
        if (gameOverUI) gameOverUI.SetActive(false);
        if (prototypeCompleteUI) prototypeCompleteUI.SetActive(false);

        if (firstQuest != null) StartQuest(firstQuest);
        
        if (MusicManager.Instance != null)
        {
            MusicManager.Instance.SetSection("Value A");
            MusicManager.Instance.SetVolumeImmediate(1f);
        }
    }

    // --- ✅ НОВЫЙ МЕТОД ДЛЯ СМЕНЫ ТЕКСТА (ВЫЗЫВАЕТСЯ ИЗ ДИАЛОГА) ---
    public void ForceUpdateQuestText(string text)
    {
        if (questUI != null)
        {
            // Включаем сам объект UI, если он был выключен
            questUI.gameObject.SetActive(true);

            // Ищем TextMeshPro внутри
            var textComp = questUI.GetComponentInChildren<TextMeshProUGUI>();
            if (textComp != null)
            {
                textComp.text = text;
            }
            else
            {
                // Запасной вариант для обычного Text
                var oldText = questUI.GetComponentInChildren<Text>();
                if (oldText != null) oldText.text = text;
            }
        }
    }
    // -------------------------------------------------------------

    public void StartQuest(Quest questToStart)
    {
        if (questToStart == null || questUI == null) return;

        currentQuest = questToStart;
        currentQuest.isComplete = false;
        currentQuest.currentObjectiveIndex = 0;

        foreach(var obj in currentQuest.objectives)
        {
            obj.currentAmount = 0;
            obj.isComplete = false;
        }

        if(questToStart.questID == "Quest1_Placement") placedItems.Clear();

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
                questUI.ShowQuestUpdate(currentQuest);

                if (objective.currentAmount >= objective.requiredAmount) CompleteCurrentObjective();
            }
        }
        else if (objective.objectiveType == ObjectiveType.Interact && objective.targetID == itemID_or_TargetID)
        {
            CompleteCurrentObjective();
        }
    }

    void CompleteCurrentObjective()
    {
        if (currentQuest == null) return;
        currentQuest.CompleteObjective();

        if (currentQuest.CheckObjectives()) CompleteQuest(currentQuest);
        else questUI.ShowQuestUpdate(currentQuest);
    }

    void CompleteQuest(Quest completedQuest)
    {
        Debug.Log("КВЕСТ ВЫПОЛНЕН: " + completedQuest.questTitle);
        questUI.ShowQuestCompleted(completedQuest);

        if (completedQuest.questID == "Quest1_Placement" || completedQuest.questID == "Quest2_Door")
        {
            if (!questCompleteSound.IsNull) RuntimeManager.PlayOneShot(questCompleteSound);
        }

        TriggerQuestEvent(completedQuest.questID);

        if (completedQuest.nextQuest != null) StartQuest(completedQuest.nextQuest);
    }

    IEnumerator PlayKnockAfterFade(float fadeDuration)
    {
        if (MusicManager.Instance != null) MusicManager.Instance.FadeToVolume(0f, fadeDuration);
        yield return new WaitForSeconds(fadeDuration);
        if (!knockSound.IsNull) RuntimeManager.PlayOneShot(knockSound);
    }

    public void TriggerQuestEvent(string questID)
    {
        switch (questID)
        {
            case "Quest1_Placement":
                if (MusicManager.Instance != null) StartCoroutine(PlayKnockAfterFade(musicFadeBeforeKnockDuration));
                else if(!knockSound.IsNull) RuntimeManager.PlayOneShot(knockSound);
                break;

            case "Quest2_Door":
                if (followLightController != null)
                {
                    followLightController.StartSequence(this);
                    if (MusicManager.Instance != null) MusicManager.Instance.SetSection("Value C");
                    MusicManager.Instance.SetVolumeImmediate(1f);
                }
                break;

            case "Quest_FollowLight": break;

            case "Quest_RepairPanel":
                if (repairPanelQuest != null) StartQuest(repairPanelQuest);
                break;
        }
    }
    
    public void TriggerChaseScene()
    {
        StartCoroutine(ChaseSceneSequence());
    }

    IEnumerator ChaseSceneSequence()
    {
        yield return new WaitForSeconds(0.1f);

        if (MusicManager.Instance != null)
        {
            MusicManager.Instance.EnsureMusicPlaying();
            MusicManager.Instance.SetVolumeImmediate(1f);
            MusicManager.Instance.SetSection("Value D");
        }

        if (!umbrellaManAppearSound.IsNull) RuntimeManager.PlayOneShot(umbrellaManAppearSound);

        TriggerQuestEvent("Quest_RepairPanel");

        if (chase != null)
        {
            chase.gameObject.SetActive(true);
            if (enemyLightDistortion != null) enemyLightDistortion.SetChaseActive(true);
            if (shieldInteractable != null) shieldInteractable.EnableShieldInteraction();
            chase.StartChase();
        }
    }

    public void OnQTESuccess()
    {
        // ПОБЕДА
        if (MusicManager.Instance != null)
        {
            MusicManager.Instance.FadeToVolume(0f, 1f);
            MusicManager.Instance.SetSection("Value A");
        }
        
        if (umbrellaManNear)
        {
            var chase = umbrellaManNear.GetComponent<UmbrellaManChase>();
            if (chase != null) chase.StopBreathingLoop();
            umbrellaManNear.SetActive(false);
        }

        if (lightController != null) lightController.TurnOffAllLights();
        if (umbrellaManFar) umbrellaManFar.SetActive(true);
        if (finalSpotlight != null) finalSpotlight.SetActive(true);
        
        if (playerController)
        {
            playerController.StartCinematicPan(umbrellaManFar.transform, 4.0f);
        }

        StartCoroutine(FinalSequenceAfterLook(6f));
    }

    public void OnQTEFailure()
    {
        // ПРОИГРЫШ
        if (enemyLightDistortion != null) enemyLightDistortion.SetChaseActive(false);
        if (MusicManager.Instance != null) MusicManager.Instance.FadeToVolume(0f, 0.5f);
        if (repairQTE != null) repairQTE.isQTEActive = false;

        StartCoroutine(ShowGameOverAfterDelay(0.5f));
    }

    // --- Метод для вызова Game Over извне (например, если монстр поймал) ---
    public void TriggerGameOver()
    {
        StartCoroutine(ShowGameOverAfterDelay(0f));
    }

    IEnumerator FinalSequenceAfterLook(float delay)
    {
        yield return new WaitForSeconds(delay);

        // ✅ ГАРАНТИЯ: Выключаем Game Over перед показом победы
        if (gameOverUI != null) gameOverUI.SetActive(false);

        if (prototypeCompleteUI != null)
        {
            prototypeCompleteUI.SetActive(true);
            
            CanvasGroup cg = prototypeCompleteUI.GetComponent<CanvasGroup>();
            if (cg == null) cg = prototypeCompleteUI.AddComponent<CanvasGroup>();
            
            float fadeTime = 2.0f;
            float t = 0;
            cg.alpha = 0;
            
            while (t < fadeTime)
            {
                t += Time.deltaTime;
                cg.alpha = Mathf.Lerp(0, 1, t / fadeTime);
                yield return null;
            }
            cg.alpha = 1;
        }

        if (MusicManager.Instance != null)
        {
            MusicManager.Instance.FadeToVolume(MusicManager.Instance.defaultVolume, 3f);
        }
        
        // Останавливаем время и показываем курсор для финала
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    IEnumerator ShowGameOverAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);

        // ✅ ГАРАНТИЯ: Выключаем экран победы перед показом смерти
        if (prototypeCompleteUI != null) prototypeCompleteUI.SetActive(false);

        if (gameOverUI != null) gameOverUI.SetActive(true);
        
        if (playerController) playerController.SetCanMove(false);
        
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    private void Update()
    {
        if (gameOverUI != null && gameOverUI.activeSelf)
        {
            if (Input.GetKeyDown(KeyCode.Space))
            {
                RespawnAfterDeath();
            }
        }
    }

    private void RespawnAfterDeath()
    {
        if (chase)
        {
            chase.ResetChase();
            chase.gameObject.SetActive(false);
        }

        if (playerController)
        {
            CharacterController cc = playerController.GetComponent<CharacterController>();
            if (cc) cc.enabled = false;

            playerController.transform.position = new Vector3(-3.645f, 1.133824f, -35.114f);
            playerController.transform.rotation = Quaternion.Euler(0f, 90f, 0f);
            playerController.SetRotation(90f, 0f);
            
            // Скрываем UI при респауне
            if (gameOverUI) gameOverUI.SetActive(false);
            if (prototypeCompleteUI) prototypeCompleteUI.SetActive(false);

            FadeOut(cc);
        }

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        if (enemyLightDistortion != null)
            enemyLightDistortion.SetChaseActive(false);

        if (shieldInteractable != null)
        {
            shieldInteractable.DisableShieldInteraction();
        }

        if (repairQTE != null)
        {
            repairQTE.ResetQTEState();
        }

        TriggerQuestEvent("Quest2_Door");
    }

    public void FadeOut(CharacterController cc)
    {
        if (fadeImage == null) return;
        fadeImage.gameObject.SetActive(true);
        StartCoroutine(FadeRoutine(cc));
    }

    private IEnumerator FadeRoutine(CharacterController cc)
    {
        Color color = fadeImage.color;
        float time = 0f;

        while (time < fadeDuration)
        {
            time += Time.deltaTime;
            float t = time / fadeDuration;
            color.a = Mathf.Lerp(1f, 0f, t);
            fadeImage.color = color;
            yield return null;
        }

        fadeImage.gameObject.SetActive(false);
        color.a = 1f;
        fadeImage.color = color;

        playerController.SetCanMove(true);
        if (cc) cc.enabled = true;
        playerController.enabled = true;
    }
}
