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

    [Header("Tutorial Prompt")]
    public TextMeshProUGUI tutorialPromptText; 
    public float promptAutoCloseTime = 10f;
    
    private bool hasPressedQ = false;          
    private bool isPromptFadingOut = false; 
    private float promptTimer = 0f; 

    private CanvasGroup canvasGroup;
    private Coroutine displayCoroutine;

    void Awake()
    {
        canvasGroup = questPanel.GetComponent<CanvasGroup>(); 
        if (questTitleText != null) originalColor = questTitleText.color;
        
        canvasGroup.alpha = 0; 
        
        if (controlsPanel != null) controlsPanel.SetActive(false);
        if (tutorialPromptText != null) tutorialPromptText.gameObject.SetActive(true);
    }

    void Update()
    {
        
        if (!hasPressedQ && tutorialPromptText != null && !isPromptFadingOut)
        {
            
            float alpha = 0.3f + Mathf.PingPong(Time.time * 2f, 0.7f);
            tutorialPromptText.color = new Color(tutorialPromptText.color.r, tutorialPromptText.color.g, tutorialPromptText.color.b, alpha);

            
            promptTimer += Time.deltaTime;
            if (promptTimer >= promptAutoCloseTime)
            {
                StartCoroutine(FadeOutPrompt());
            }
        }

        
        if (Input.GetKeyDown(KeyCode.Q))
        {
             
             if (!hasPressedQ)
             {
                 if (!isPromptFadingOut) StartCoroutine(FadeOutPrompt());
             }

             ShowQuestTemporarily();
             
             
             if (controlsPanel != null) controlsPanel.SetActive(true);
        }
    }

    
    IEnumerator FadeOutPrompt()
    {
        isPromptFadingOut = true;
        hasPressedQ = true;

        float duration = 1.0f;
        float timer = 0f;
        Color startColor = tutorialPromptText.color;

        while (timer < duration)
        {
            timer += Time.deltaTime;
            float newAlpha = Mathf.Lerp(startColor.a, 0f, timer / duration);
            tutorialPromptText.color = new Color(startColor.r, startColor.g, startColor.b, newAlpha);
            yield return null;
        }

        tutorialPromptText.color = new Color(startColor.r, startColor.g, startColor.b, 0f);
        tutorialPromptText.gameObject.SetActive(false);
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

        
        yield return new WaitForSeconds(visibleTime);

        
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
