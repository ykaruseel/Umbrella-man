using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class MultiUIColorChanger : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField] private List<Graphic> targets;

    [SerializeField] private float duration = 0.2f;

    private Color hoverColor = new Color32(255, 180, 0, 255);
    private Dictionary<Graphic, Color> originalColors = new Dictionary<Graphic, Color>();

    private void Awake()
    {
        foreach (var t in targets)
        {
            if (t != null && !originalColors.ContainsKey(t))
                originalColors[t] = t.color;
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        StopAllCoroutines();
        foreach (var t in targets)
        {
            if (t != null)
                StartCoroutine(LerpColor(t, t.color, hoverColor, duration));
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        StopAllCoroutines();
        foreach (var t in targets)
        {
            if (t != null && originalColors.ContainsKey(t))
                StartCoroutine(LerpColor(t, t.color, originalColors[t], duration));
        }
    }

    private IEnumerator LerpColor(Graphic g, Color from, Color to, float time)
    {
        float t = 0f;
        while (t < time)
        {
            t += Time.unscaledDeltaTime;
            g.color = Color.Lerp(from, to, t / time);
            yield return null;
        }
        g.color = to;
    }
}
