using System.Collections;
using UnityEngine;

public class MenuFader : MonoBehaviour
{
    [SerializeField] private float fadeDuration = 0.3f;

    [SerializeField] private CanvasGroup mainMenu;

    [SerializeField] private CanvasGroup settingsMenu;

    [SerializeField] private CanvasGroup creditsMenu;

    private bool isTransitioning = false;

    public void SwitchToSettings()
    {
        if (isTransitioning) return;
        StartCoroutine(FadeRoutine(mainMenu, settingsMenu));
    }

    public void SwitchFromSettings()
    {
        if (isTransitioning) return;
        StartCoroutine(FadeRoutine(settingsMenu, mainMenu));
    }

    public void SwitchToCredits()
    {
        if (isTransitioning) return;
        StartCoroutine(FadeRoutine(mainMenu, creditsMenu));
    }

    public void SwitchFromCredits()
    {
        if (isTransitioning) return;
        StartCoroutine(FadeRoutine(creditsMenu, mainMenu));
    }

    private IEnumerator FadeRoutine(CanvasGroup from, CanvasGroup to)
    {
        isTransitioning = true;
        yield return FadeOut(from);

        yield return FadeIn(to);
        isTransitioning = false;
    }

    private IEnumerator FadeOut(CanvasGroup cg)
    {
        cg.interactable = false;
        cg.blocksRaycasts = false;

        while (cg.alpha > 0)
        {
            cg.alpha -= Time.unscaledDeltaTime / fadeDuration;
            yield return null;
        }

        cg.alpha = 0;
    }

    private IEnumerator FadeIn(CanvasGroup cg)
    {
        cg.alpha = 0;
        cg.interactable = true;
        cg.blocksRaycasts = true;

        while (cg.alpha < 1)
        {
            cg.alpha += Time.unscaledDeltaTime / fadeDuration;
            yield return null;
        }

        cg.alpha = 1;
    }

    public void ResetFade()
    {
        isTransitioning = true;
        StopAllCoroutines();

        mainMenu.alpha = 1;
        mainMenu.interactable = true;
        mainMenu.blocksRaycasts = true;

        settingsMenu.alpha = 0;
        settingsMenu.interactable = false;
        settingsMenu.blocksRaycasts = false;

        isTransitioning = false;
    }
}
