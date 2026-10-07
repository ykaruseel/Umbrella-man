using System.Collections;
using TMPro;
using UnityEngine;

public class InteractionMessageUI : MonoBehaviour
{
    [SerializeField]
    private TextMeshProUGUI messageText;

    [SerializeField]
    private CanvasGroup canvasGroup;

    [SerializeField]
    private float fadeDuration = 0.25f;

    [SerializeField]
    private float visibleDuration = 1.5f;


    private Coroutine currentRoutine;


    private void Awake()
    {
        if (canvasGroup != null)
        {
            canvasGroup.alpha = 0f;
            canvasGroup.interactable = false;
            canvasGroup.blocksRaycasts = false;
        }

        if (messageText != null)
        {
            messageText.text = "";
        }
    }


    public void ShowMessage(string message)
    {
        if (currentRoutine != null)
        {
            StopCoroutine(currentRoutine);
        }

        currentRoutine =
            StartCoroutine(
                ShowRoutine(message)
            );
    }


    private IEnumerator ShowRoutine(string message)
    {
        if (messageText != null)
        {
            messageText.text = message;
        }


        yield return StartCoroutine(
            FadeTo(1f)
        );


        yield return new WaitForSeconds(
            visibleDuration
        );


        yield return StartCoroutine(
            FadeTo(0f)
        );


        currentRoutine = null;
    }


    private IEnumerator FadeTo(float target)
    {
        if (canvasGroup == null)
            yield break;


        float startAlpha = canvasGroup.alpha;
        float elapsed = 0f;


        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;

            float t =
                Mathf.Clamp01(
                    elapsed / fadeDuration
                );

            canvasGroup.alpha =
                Mathf.Lerp(
                    startAlpha,
                    target,
                    t
                );

            yield return null;
        }


        canvasGroup.alpha = target;
    }
}