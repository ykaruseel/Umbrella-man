using NUnit.Framework.Internal;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class VHSNoise : MonoBehaviour
{
    public RawImage img;
    public float speed = 0.5f;

    private void Update()
    {
        Rect r = img.uvRect;
        r.y += Time.deltaTime * speed;
        img.uvRect = r;
    }

    public IEnumerator FadeRoutine()
    {
        Color startColor = img.color;
        float elapsed = 0f;

        while (elapsed < 1f)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / 1f);
            Color c = startColor;
            c.a = Mathf.Lerp(startColor.a, 0f, t);
            img.color = c;
            yield return null;
        }

        Color finalColor = startColor;
        finalColor.a = 0f;
        img.color = finalColor;
    }
}
