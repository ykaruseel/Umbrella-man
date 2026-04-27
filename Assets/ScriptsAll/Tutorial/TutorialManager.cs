using UnityEngine;
using TMPro;
using System.Collections;
using System.Collections.Generic;

public class TutorialManager : MonoBehaviour
{
    public static TutorialManager Instance;

    public CanvasGroup hintCanvasGroup;

    public TextMeshProUGUI hintText;

    public float fadeDuration = 1f;

    public float displayDuration = 7f; 

    private HashSet<HintType> shownHints = new HashSet<HintType>();
    private Coroutine activeCoroutine;

    private void Awake()
    {
        Instance = this;
        hintCanvasGroup.alpha = 0;
    }

    public void ShowHint(HintType type)
    {
        if (shownHints.Contains(type)) return;

        string message = GetMessage(type);
        if (string.IsNullOrEmpty(message)) return;

        shownHints.Add(type);

        if (activeCoroutine != null) StopCoroutine(activeCoroutine);
        activeCoroutine = StartCoroutine(FadeRoutine(message));
    }

    private IEnumerator FadeRoutine(string msg)
    {
        if (hintCanvasGroup.alpha > 0.1f)
        {
            yield return StartCoroutine(FadeCanvas(hintCanvasGroup.alpha, 0.5f));
        }

        hintText.text = msg;

        yield return StartCoroutine(FadeCanvas(hintCanvasGroup.alpha, 1));

        yield return new WaitForSeconds(displayDuration);

        yield return StartCoroutine(FadeCanvas(1, 0));

        activeCoroutine = null;
    }

    private IEnumerator FadeOutOnly()
    {
        yield return StartCoroutine(FadeCanvas(hintCanvasGroup.alpha, 0));
        activeCoroutine = null;
    }

    private IEnumerator FadeCanvas(float start, float end)
    {
        float elapsed = 0;
        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            hintCanvasGroup.alpha = Mathf.Lerp(start, end, elapsed / fadeDuration);
            yield return null;
        }
        hintCanvasGroup.alpha = end;
    }

    private string GetMessage(HintType type)
    {
        return type switch
        {
            HintType.Move => "Use [WASD] to move",
            HintType.ViewQuest => "Press [Q] to view current task",
            HintType.Interact => "Press [E] to open the door, pick up or place the item",
            HintType.Sprint => "Hold [Shift] to sprint",
            HintType.Flashlight => "Click [LMB] to charge the flashlight",
            _ => ""
        };
    }

    public List<HintType> GetShownHints() => new List<HintType>(shownHints);
    public void LoadShownHints(List<HintType> saved) => shownHints = new HashSet<HintType>(saved);
}
