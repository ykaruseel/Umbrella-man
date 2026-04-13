using System.Collections;
using TMPro;
using UnityEngine;

public class QuestUIV2 : MonoBehaviour
{
    public TextMeshProUGUI questText;
    public CanvasGroup canvasGroup;
    private bool isVisible = false;

    void Start() => canvasGroup.alpha = 0;

    public void ShowNewQuest(QuestData quest)
    {
        questText.text = quest.GetTitleWithProgress();
        questText.color = Color.white;
        StopAllCoroutines();
        StartCoroutine(FadeSequence());
    }

    private IEnumerator FadeSequence()
    {
        yield return StartCoroutine(Fade(1, 0.5f));
        yield return new WaitForSeconds(2f);
        yield return StartCoroutine(Fade(0, 0.5f));
        isVisible = false;
    }

    public void ToggleQuest()
    {
        isVisible = !isVisible;
        StopAllCoroutines();
        float targetAlpha = isVisible ? 1 : 0;
        StartCoroutine(Fade(targetAlpha, 0.3f));
    }

    private IEnumerator Fade(float targetAlpha, float duration)
    {
        float startAlpha = canvasGroup.alpha;
        float time = 0;
        while (time < duration)
        {
            time += Time.deltaTime;
            canvasGroup.alpha = Mathf.Lerp(startAlpha, targetAlpha, time / duration);
            yield return null;
        }
        canvasGroup.alpha = targetAlpha;
    }

    public IEnumerator CompleteAndSwitchRoutine(QuestData completedQuest, QuestData nextQuest = null)
    {
        if (canvasGroup.alpha < 0.1f)
        {
            questText.text = completedQuest.GetTitleWithProgress();
            questText.color = Color.white;
            yield return StartCoroutine(Fade(1, 0.5f));
        }

        questText.text = completedQuest.GetTitleWithProgress();

        questText.color = Color.green;

        transform.localScale = Vector3.one * 1.1f;

        yield return new WaitForSeconds(2f);

        transform.localScale = Vector3.one;

        yield return StartCoroutine(Fade(0, 0.5f));

        if (nextQuest != null)
        {
            yield return new WaitForSeconds(0.5f);
            ShowNewQuest(nextQuest);
        }
    }

    //torze nado perenesti v drugoe mesto
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Q))
        {
            ToggleQuest();
        }
    }

    public void UpdateProgressUI(QuestData quest)
    {
        questText.text = quest.GetTitleWithProgress();

        if (canvasGroup.alpha < 0.1f)
        {
            StopAllCoroutines();
            StartCoroutine(Fade(1, 0.3f));
            isVisible = true;
        }
    }
}
