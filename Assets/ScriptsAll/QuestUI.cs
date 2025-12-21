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

    // ↓↓↓ НОВОЕ: Переменные для подсказки "Tap Q" ↓↓↓
    [Header("Tutorial Prompt")]
    public TextMeshProUGUI tutorialPromptText; // Ссылка на текст подсказки
    private bool hasPressedQ = false;          // Флаг: нажимали ли уже Q?
    // ↑↑↑

    private CanvasGroup canvasGroup;
    private Coroutine displayCoroutine;

    void Awake()
    {
        canvasGroup = questPanel.GetComponent<CanvasGroup>(); 
        if (questTitleText != null) originalColor = questTitleText.color;
        
        canvasGroup.alpha = 0; 
        
        // Гарантированно скрываем панель управления при старте
        if (controlsPanel != null) controlsPanel.SetActive(false);

        // Гарантированно ВКЛЮЧАЕМ подсказку при старте
        if (tutorialPromptText != null) tutorialPromptText.gameObject.SetActive(true);
    }

    void Update()
    {
        // ↓↓↓ НОВОЕ: Логика пульсации текста (мигание) ↓↓↓
        if (!hasPressedQ && tutorialPromptText != null)
        {
            // Плавно меняем прозрачность от 0.2 до 1.0
            float alpha = 0.2f + Mathf.PingPong(Time.time * 2f, 0.8f);
            Color c = tutorialPromptText.color;
            tutorialPromptText.color = new Color(c.r, c.g, c.b, alpha);
        }
        // ↑↑↑

        if (Input.GetKeyDown(KeyCode.Q))
        {
             // ↓↓↓ НОВОЕ: Если это первое нажатие - убираем подсказку навсегда ↓↓↓
             if (!hasPressedQ)
             {
                 hasPressedQ = true;
                 if (tutorialPromptText != null) 
                 {
                     tutorialPromptText.gameObject.SetActive(false);
                 }
             }
             // ↑↑↑

             ShowQuestTemporarily();
             
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
