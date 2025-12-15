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

    // ↓↓↓ 1. ДОБАВИЛИ ПЕРЕМЕННУЮ ДЛЯ ПАНЕЛИ УПРАВЛЕНИЯ ↓↓↓
    [Header("Settings")]
    public GameObject controlsPanel; 
    // ↑↑↑

    private CanvasGroup canvasGroup;
    private Coroutine displayCoroutine;

    void Awake()
    {
        canvasGroup = questPanel.GetComponent<CanvasGroup>(); 
        if (questTitleText != null) originalColor = questTitleText.color;
        
        canvasGroup.alpha = 0; 
        
        // Гарантированно скрываем панель при старте
        if (controlsPanel != null) controlsPanel.SetActive(false);
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Q))
        {
             ShowQuestTemporarily();
             
             // ↓↓↓ 2. ПОКАЗЫВАЕМ ПАНЕЛЬ ПРИ НАЖАТИИ Q ↓↓↓
             if (controlsPanel != null) controlsPanel.SetActive(true);
        }
    }

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
        if (displayCoroutine != null)
        {
            StopCoroutine(displayCoroutine);
        }
        
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

        // Visible (ждем 3 секунды)
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
        
        // ↓↓↓ 3. СКРЫВАЕМ ПАНЕЛЬ, КОГДА ЗАКОНЧИЛСЯ ТАЙМЕР ↓↓↓
        if (controlsPanel != null) controlsPanel.SetActive(false);
        // ↑↑↑
        
        displayCoroutine = null;
    }
}
