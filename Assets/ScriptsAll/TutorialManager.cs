using UnityEngine;
using TMPro;
using System.Collections;
using System.Collections.Generic;

public class TutorialManager : MonoBehaviour
{
    public static TutorialManager Instance;

    [Header("UI Components")]
    public CanvasGroup uiGroup; 
    public TMP_Text uiText;     

    [Header("Settings")]
    public float fadeDuration = 0.5f; 
    public float pulseSpeed = 2.0f;
    public float delayBetweenHints = 0.7f;

    public enum HintType
    {
        None,
        Movement_WASD,
        Task_Q,
        Interact_E,
        Pickup_E,
        Drop_E,
        Run_Shift,
        Flashlight_LMB
    }

    private Queue<HintType> hintQueue = new Queue<HintType>();
    private HintType currentHint = HintType.None;
    private bool isProcessing = false;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    void Start()
    {
        if (uiGroup != null) uiGroup.alpha = 0f;
    }

    
    public void ShowHint(HintType hint)
    {
        
        if (PlayerPrefs.GetInt("Tutorial_" + hint.ToString(), 0) == 1) return;
        
        
        if (currentHint == hint || hintQueue.Contains(hint)) return;

        hintQueue.Enqueue(hint);

        
        if (!isProcessing)
        {
            StartCoroutine(ProcessQueueRoutine());
        }
    }

    private IEnumerator ProcessQueueRoutine()
    {
        isProcessing = true;

        while (hintQueue.Count > 0)
        {
            currentHint = hintQueue.Dequeue();
            
            
            SetHintText(currentHint);

            
            yield return StartCoroutine(FadeAlpha(1f));

            
            bool actionDone = false;
            while (!actionDone)
            {
                
                float alpha = Mathf.PingPong(Time.time * pulseSpeed, 0.3f) + 0.7f; 
                uiGroup.alpha = alpha;

                switch (currentHint)
                {
                    case HintType.Movement_WASD:
                        if (Mathf.Abs(Input.GetAxis("Horizontal")) > 0.1f || Mathf.Abs(Input.GetAxis("Vertical")) > 0.1f) 
                            actionDone = true;
                        break;
                    case HintType.Task_Q:
                        if (Input.GetKeyDown(KeyCode.Q)) actionDone = true;
                        break;
                    case HintType.Interact_E:
                    case HintType.Pickup_E:
                    case HintType.Drop_E:
                        if (Input.GetKeyDown(KeyCode.E)) actionDone = true;
                        break;
                    case HintType.Run_Shift:
                        if (Input.GetKey(KeyCode.LeftShift)) actionDone = true;
                        break;
                    case HintType.Flashlight_LMB:
                        if (Input.GetMouseButtonDown(0)) actionDone = true;
                        break;
                }
                yield return null;
            }

            
            PlayerPrefs.SetInt("Tutorial_" + currentHint.ToString(), 1);
            PlayerPrefs.Save();

            
            yield return StartCoroutine(FadeAlpha(0f));
            
            currentHint = HintType.None;

            
            yield return new WaitForSeconds(delayBetweenHints);
        }

        isProcessing = false;
    }

    private IEnumerator FadeAlpha(float target)
    {
        float start = uiGroup.alpha;
        float t = 0;
        while (t < fadeDuration)
        {
            t += Time.deltaTime;
            uiGroup.alpha = Mathf.Lerp(start, target, t / fadeDuration);
            yield return null;
        }
        uiGroup.alpha = target;
    }

    private void SetHintText(HintType hint)
    {
        switch (hint)
        {
            case HintType.Movement_WASD: uiText.text = "Movement – WASD"; break;
            case HintType.Task_Q: uiText.text = "Press [Q] to see current task"; break;
            case HintType.Interact_E: uiText.text = "Press [E] to interact"; break;
            case HintType.Pickup_E: uiText.text = "Press [E] to pick up"; break;
            case HintType.Drop_E: uiText.text = "Press [E] to drop/place"; break;
            case HintType.Run_Shift: uiText.text = "Hold [Shift] to run"; break;
            case HintType.Flashlight_LMB: uiText.text = "Click the left mouse button to charge the flashlight"; break;
        }
    }

    [ContextMenu("Reset Progress")]
    public void ResetProgress()
    {
        PlayerPrefs.DeleteAll();
        Debug.Log("Tutorial Progress Reset!");
    }
}
