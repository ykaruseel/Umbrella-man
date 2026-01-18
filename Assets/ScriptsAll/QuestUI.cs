using UnityEngine;
using TMPro;
using System.Collections;

[RequireComponent(typeof(CanvasGroup))]
public class QuestUI : MonoBehaviour
{
    public GameObject questPanel; 
    public TextMeshProUGUI questTitleText;
    public TextMeshProUGUI questObjectiveText;
    public Color completedColor = Color.green;
    private Color originalColor;

    [Header("Animation Settings")]
    public float fadeInTime = 0.5f;
    public float visibleTime = 3.0f;
    public float fadeOutTime = 0.5f;

    [Header("Settings")]
    public GameObject controlsPanel; 

    // 🔥 УДАЛЕНО: Переменные для старой подсказки (tutorialPromptText и таймеры)

    private CanvasGroup canvasGroup;
    private Coroutine displayCoroutine;

    void Awake()
    {
        if (questPanel != null)
        {
            canvasGroup = questPanel.GetComponent<CanvasGroup>();
            canvasGroup.alpha = 0;
        }

        if (questTitleText != null) originalColor = questTitleText.color;
        
        if (controlsPanel != null) controlsPanel.SetActive(false);
        
        // 🔥 УДАЛЕНО: Включение старого текста при старте
    }

    void Update()
    {
        // 🔥 УДАЛЕНО: Логика пульсации старой подсказки

        // --- Обработка нажатия Q (ОСТАВЛЕНО) ---
        if (Input.GetKeyDown(KeyCode.Q))
        {
             // Просто показываем квест
             ShowQuestTemporarily();
             
             // Включаем панель управления (если она нужна)
             if (controlsPanel != null) controlsPanel.SetActive(true);
        }
    }

    // 🔥 УДАЛЕНО: Coroutine FadeOutPrompt

    public void ShowQuestUpdate(Quest quest)
    {
        if (quest == null) return;
        QuestObjective objective = quest.GetCurrentObjective();
        if (objective == null) return;

        questTitleText.color = originalColor;
        questTitleText.text = quest.questTitle;
        
        string objectiveText = objective.objectiveDescription;
        if(quest.questID == "Quest1_Placement" && objective.objectiveType == ObjectiveType.Place)
        {
            objectiveText += $" {objective.currentAmount}/{objective.requiredAmount}";
        }
        questObjectiveText.text = objectiveText;

        StartDisplayCoroutine();
    }

    public void ShowQuestCompleted(Quest quest)
    {
         if (quest == null) return;
         questTitleText.color = completedColor;
         questTitleText.text = quest.questTitle + " (Completed)";
         
         QuestObjective lastObjective = quest.objectives[quest.objectives.Length-1];
         string objectiveText = lastObjective.objectiveDescription;
         if(quest.questID == "Quest1_Placement" && lastObjective.objectiveType == ObjectiveType.Place)
         {
             objectiveText += $" {lastObjective.currentAmount}/{lastObjective.requiredAmount}";
         }
         questObjectiveText.text = objectiveText;

         StartDisplayCoroutine();
    }

    public void ShowQuestTemporarily()
    {
        QuestManager qm = QuestManager.instance;
        if (qm != null && qm.currentQuest != null && !qm.currentQuest.isComplete)
        {
             ShowQuestUpdate(qm.currentQuest);
        }
    }

    private void StartDisplayCoroutine()
    {
        if (displayCoroutine != null) StopCoroutine(displayCoroutine);
        displayCoroutine = StartCoroutine(DisplaySequence());
    }

    IEnumerator DisplaySequence()
    {
        // Fade In
        float timer = 0;
        float startAlpha = canvasGroup.alpha; 
        while (timer < fadeInTime)
        {
            canvasGroup.alpha = Mathf.Lerp(startAlpha, 1, timer / fadeInTime); 
            timer += Time.deltaTime;
            yield return null;
        }
        canvasGroup.alpha = 1;

        // Visible
        yield return new WaitForSeconds(visibleTime);

        // Fade Out
        timer = 0;
        while (timer < fadeOutTime)
        {
            canvasGroup.alpha = Mathf.Lerp(1, 0, timer / fadeOutTime);
            timer += Time.deltaTime;
            yield return null;
        }
        canvasGroup.alpha = 0;
        
        if (controlsPanel != null) controlsPanel.SetActive(false);
        
        displayCoroutine = null;
    }
}
