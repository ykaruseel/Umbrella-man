using System.Collections;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class CameraFade : MonoBehaviour
{
    [SerializeField] private Image fadeImage;
    [SerializeField] private float fadeDuration = 1f;

    private bool active = false;

    public IEnumerator FadeOut()
    {
        active = !active;
        SetFadeImageActive(active);
        yield return Fade(0f, 1f);
    }

    public IEnumerator FadeIn()
    {
        yield return Fade(1f, 0f);
        active = !active;
        SetFadeImageActive(active);
    }

    IEnumerator Fade(float from, float to)
    {
        float t = 0f;
        Color c = fadeImage.color;

        while (t < 1f)
        {
            t += Time.deltaTime / fadeDuration;
            c.a = Mathf.Lerp(from, to, t);
            fadeImage.color = c;
            yield return null;
        }

        c.a = to;
        fadeImage.color = c;
    }

    public void SetFadeImageActive(bool value)
    {
        StopAllCoroutines();
        fadeImage.gameObject.SetActive(value);
    }

    public void SetFadeAlpha(float alpha)
    {
        Color c = fadeImage.color;
        c.a = Mathf.Clamp01(alpha);
        fadeImage.color = c;
    }
}

