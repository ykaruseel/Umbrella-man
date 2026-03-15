using UnityEngine;
using TMPro;
using System.Collections;

public class TutorialManager : MonoBehaviour
{
    public static TutorialManager instance;

    [Header("UI Components")]
    public CanvasGroup uiGroup; 
    public TMP_Text uiText;     

    [Header("Settings")]
    public float fadeDuration = 1.0f; 
    public float pulseSpeed = 2.0f;   
    public float delayBetweenSteps = 1.5f; 

    public enum TutorialStep
    {
        Movement_WASD = 0,
        Interaction_E = 1,
        Task_Q = 2,
        Completed = 99
    }

    public TutorialStep currentStep;
    
    private bool isVisible = false;
    private float moveTimer = 0f;

    void Awake()
    {
        if (instance == null) instance = this;
        else Destroy(gameObject);
    }

    void Start()
    {
        uiGroup.alpha = 0f;
    }

    public void StartTutorial()
    {
        gameObject.SetActive(true);

        if (uiGroup != null)
            uiGroup.gameObject.SetActive(true);

        if (uiText != null)
            uiText.gameObject.SetActive(true);

        int savedStep = PlayerPrefs.GetInt("TutorialProgress", 0);
        currentStep = (TutorialStep)savedStep;

        if (currentStep == TutorialStep.Completed)
        {
            gameObject.SetActive(false);
            return;
        }

        StartCoroutine(ProcessStep());
    }


    void Update()
    {
        if (isVisible)
        {
            
            float alpha = Mathf.PingPong(Time.time * pulseSpeed, 0.6f) + 0.4f; 
            uiGroup.alpha = alpha;
        }

        
        if (currentStep == TutorialStep.Movement_WASD && isVisible)
        {
            float x = Input.GetAxis("Horizontal");
            float z = Input.GetAxis("Vertical");

            if (Mathf.Abs(x) > 0.1f || Mathf.Abs(z) > 0.1f)
            {
                moveTimer += Time.deltaTime;
                if (moveTimer > 1.0f)
                {
                    AdvanceTutorial();
                }
            }
        }

        
        if (currentStep == TutorialStep.Interaction_E && isVisible)
        {
            moveTimer += Time.deltaTime;
            
            if (moveTimer > 7.0f)
            {
                AdvanceTutorial();
            }
        }

        
        if (currentStep == TutorialStep.Task_Q && isVisible)
        {
            
            moveTimer += Time.deltaTime;

            
            if (moveTimer > 3.0f || Input.GetKeyDown(KeyCode.Q))
            {
                AdvanceTutorial();
            }
        }
    }

    public void CompleteInteractionStep()
    {
        if (currentStep == TutorialStep.Interaction_E && isVisible)
        {
            AdvanceTutorial();
        }
    }

    private void AdvanceTutorial()
    {
        isVisible = false; 
        moveTimer = 0f;
        
        switch (currentStep)
        {
            case TutorialStep.Movement_WASD:
                currentStep = TutorialStep.Interaction_E;
                break;
            case TutorialStep.Interaction_E:
                currentStep = TutorialStep.Task_Q;
                break;
            case TutorialStep.Task_Q:
                currentStep = TutorialStep.Completed;
                break;
        }

        PlayerPrefs.SetInt("TutorialProgress", (int)currentStep);
        PlayerPrefs.Save();

        StartCoroutine(ProcessStep());
    }

    IEnumerator ProcessStep()
    {
        yield return FadeAlpha(0f);

        if (currentStep == TutorialStep.Completed)
        {
            gameObject.SetActive(false);
            yield break;
        }

        yield return new WaitForSeconds(delayBetweenSteps);

        switch (currentStep)
        {
            case TutorialStep.Movement_WASD:
                uiText.text = "Movement – WASD";
                break;
            case TutorialStep.Interaction_E:
                uiText.text = "Item interaction [E]";
                break;
            case TutorialStep.Task_Q:
                uiText.text = "Press [Q] to see current task";
                break;
        }

        yield return FadeAlpha(1f);
        isVisible = true; 
    }

    IEnumerator FadeAlpha(float targetAlpha)
    {
        float startAlpha = uiGroup.alpha;
        float time = 0;

        while (time < fadeDuration)
        {
            time += Time.deltaTime;
            uiGroup.alpha = Mathf.Lerp(startAlpha, targetAlpha, time / fadeDuration);
            yield return null;
        }
        uiGroup.alpha = targetAlpha;
    }
    
    [ContextMenu("Reset Progress")]
    public void ResetProgress()
    {
        PlayerPrefs.DeleteKey("TutorialProgress");
        Debug.Log("Tutorial Reset!");
    }

}
