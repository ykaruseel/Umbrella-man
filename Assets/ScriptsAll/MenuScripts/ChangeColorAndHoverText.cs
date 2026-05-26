using System.Collections;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;

public class ChangeColorAndHoverText : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public bool needHover;

    [SerializeField] private float moveOffset = 20f;
    [SerializeField] private float animationTime = 0.15f;

    private Color hoverColor = new Color32(255, 180, 0, 255);
    private TMP_Text text;
    private RectTransform rectTransform;
    private Vector3 startPos;
    private Color startColor;
    private Coroutine animationCoroutine;

    private void Awake()
    {
        text = GetComponent<TMP_Text>();
        rectTransform = GetComponent<RectTransform>();

        startPos = rectTransform.anchoredPosition;
        startColor = text.color;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        StartAnimation(startPos + Vector3.right * moveOffset, hoverColor);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        StartAnimation(startPos, startColor);
    }

    private void StartAnimation(Vector3 targetPos, Color targetColor)
    {
        if (animationCoroutine != null)
            StopCoroutine(animationCoroutine);

        animationCoroutine = StartCoroutine(Animate(targetPos, targetColor));
    }

    private IEnumerator Animate(Vector3 targetPos, Color targetColor)
    {
        float t = 0f;
        Vector3 fromPos = rectTransform.anchoredPosition;
        Color fromColor = text.color;

        while (t < animationTime)
        {
            t += Time.unscaledDeltaTime;
            float lerp = t / animationTime;

            if (needHover)
                rectTransform.anchoredPosition = Vector2.Lerp(fromPos, targetPos, lerp);

            text.color = Color.Lerp(fromColor, targetColor, lerp);

            yield return null;
        }

        if (needHover)
            rectTransform.anchoredPosition = targetPos;

        text.color = targetColor;
    }

    private void OnDisable()
    {
        rectTransform.anchoredPosition = startPos;
        text.color = startColor;
    }
}
