// Файл: QuestUI.cs (ЧИСТАЯ ВЕРСИЯ)
using UnityEngine;
using TMPro;
using System.Collections;

[RequireComponent(typeof(CanvasGroup))]
public class QuestUI : MonoBehaviour
{
    public GameObject questPanel; // Поле для "Себя"
    public TextMeshProUGUI questTitleText;
    public TextMeshProUGUI questObjectiveText;
    public Color completedColor = Color.green;
    private Color originalColor;

    [Header("Animation Settings")]
    public float fadeInTime = 0.5f;
    public float visibleTime = 3.0f;
    public float fadeOutTime = 0.5f;

    private CanvasGroup canvasGroup;
    private Coroutine displayCoroutine;

    void Awake()
    {
        // Твой скриншот ДОКАЗЫВАЕТ, что questPanel НАЗНАЧЕН в инспекторе.
        // Поэтому "костыль" (if questPanel == null) не нужен.
        
        canvasGroup = questPanel.GetComponent<CanvasGroup>(); 
        if (questTitleText != null) originalColor = questTitleText.color;
        
        // Убедись, что QuestPanel ВКЛЮЧЕН в иерархии, но Alpha = 0
        canvasGroup.alpha = 0; 
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Q))
        {
             ShowQuestTemporarily();
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
        
        // Мы НЕ используем questPanel.SetActive(true)
        displayCoroutine = StartCoroutine(DisplaySequence());
    }

    IEnumerator DisplaySequence()
    {
        // Мы НЕ используем questPanel.SetActive(true)
        
        // Fade In (Агрессивная версия)
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
        
        // Мы НЕ используем questPanel.SetActive(false)
        displayCoroutine = null;
    }
}
