using System.Collections;
using UnityEngine;

public class GameTitleIntro : MonoBehaviour
{
    public CanvasGroup intro;
    public CanvasGroup UI;
    public float fadeDuration = 2f;


    private static bool introPlayed = false;

    void Awake()
    {
        if (introPlayed)
        {
            if (intro != null)
                intro.gameObject.SetActive(false);
            return;
        }

        UI.alpha = 0f;
        UI.blocksRaycasts = false;
        UI.interactable = false;


        introPlayed = true;

        if (intro != null)
            intro.gameObject.SetActive(true);

        intro.alpha = 1f;

        StartCoroutine(FadeOutIntro());
    }

    private IEnumerator FadeOutIntro()
    {
        float elapsed = 0f;
        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            intro.alpha = Mathf.Lerp(1f, 0f, elapsed / fadeDuration);
            yield return null;
        }

        intro.alpha = 0f;
        intro.gameObject.SetActive(false);

        StartCoroutine(FadeInUI());
    }

    private IEnumerator FadeInUI()
    {
        float elapsed = 0f;
        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            UI.alpha = Mathf.Lerp(0f, 1f, elapsed / fadeDuration);
            yield return null;
        }

        UI.alpha = 1f;
        UI.blocksRaycasts = true;
        UI.interactable = true;
    }
}
