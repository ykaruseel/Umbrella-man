using System.Collections;
using UnityEngine;

public class GameTitleIntro : MonoBehaviour
{
    public CanvasGroup BackGround;
    public CanvasGroup Text;
    public CanvasGroup UI;

    public GameObject IntroGO;

    public float fadeDuration = 2f;


    private static bool introPlayed = false;

    void Awake()
    {
        if (introPlayed)
        {
            if (BackGround != null)
                IntroGO.SetActive(false);
            return;
        }

        Text.alpha = 0f;
        Text.blocksRaycasts = false;
        Text.interactable = false;

        UI.alpha = 0f;
        UI.blocksRaycasts = false;
        UI.interactable = false;

        introPlayed = true;

        if (BackGround != null)
            BackGround.gameObject.SetActive(true);

        BackGround.alpha = 1f;
        BackGround.blocksRaycasts = true;
        BackGround.interactable = true;

        StartCoroutine(FadeInUI());
    }

    private IEnumerator FadeOutIntro()
    {
        float elapsed = 0f;
        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            BackGround.alpha = Mathf.Lerp(1f, 0f, elapsed / fadeDuration);
            yield return null;
        }

        BackGround.alpha = 0f;
        BackGround.blocksRaycasts = false;
        BackGround.interactable = false;
        BackGround.gameObject.SetActive(false);

        IntroGO.SetActive(false);

        elapsed = 0f;
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

    private IEnumerator FadeInUI()
    {
        float elapsed = 0f;
        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            Text.alpha = Mathf.Lerp(0f, 1f, elapsed / fadeDuration);
            yield return null;
        }

        Text.alpha = 1f;
        Text.blocksRaycasts = true;
        Text.interactable = true;

        yield return new WaitForSeconds(2f);

        elapsed = 0f;
        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            Text.alpha = Mathf.Lerp(1f, 0f, elapsed / fadeDuration);
            yield return null;
        }

        Text.alpha = 0f;
        Text.blocksRaycasts = false;
        Text.interactable = false;

        StartCoroutine(FadeOutIntro());
    }
}
