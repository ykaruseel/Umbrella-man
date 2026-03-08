// Assets/Scripts/UI/IntroNarrativeController.cs

using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using TMPro;


public sealed class IntroNarrativeController : MonoBehaviour
{
    [Header("Durations (seconds)")]
    [SerializeField] private float fadeInSeconds = 0.6f;
    [SerializeField] private float showSeconds = 60f;
    [SerializeField] private float fadeOutSeconds = 0.8f;

    [Header("Narrative UI")]
    [SerializeField] private CanvasGroup canvasGroup;
    [SerializeField] private Image backgroundImage;
    [SerializeField] private TextMeshProUGUI narrativeText;

    [Header("Gameplay")]
    [SerializeField] private PlayerController playerController;

    [TextArea(6, 12)]
    [SerializeField]
    private string storyText =
        "����� ����� ���������� �� ����. ����� �� ������������, � ���� ������� ���� �� ������.\n" +
        "�������, �� ��������� ������ ������� � ������. ��, ��� �������� ��� ������, �� ������������...\n\n" +
        "������� ����� �� ��������� �� �����. �������, �� ��� �����.";

    private bool createdUI;

    void Awake()
    {
        if (canvasGroup == null || narrativeText == null)
        {
            CreateOverlayUIIfMissing();
            createdUI = true;
        }

        if (narrativeText != null) narrativeText.text = storyText;

        if (canvasGroup != null)
        {
            canvasGroup.alpha = 0f;
            canvasGroup.gameObject.SetActive(true);
            canvasGroup.interactable = false;
            canvasGroup.blocksRaycasts = true;
        }
    }

    void Start()
    {
        
        if (playerController != null)
        {
            playerController.LockMovementButAllowLook();
        }

        StartCoroutine(RunIntroSequence());
    }

    private IEnumerator RunIntroSequence()
    {
        if (canvasGroup == null)
            yield break;

        
        yield return FadeCanvasGroup(canvasGroup, 0f, 1f, fadeInSeconds);

        
        yield return new WaitForSeconds(showSeconds);

        
        yield return FadeCanvasGroup(canvasGroup, 1f, 0f, fadeOutSeconds);

        
        canvasGroup.gameObject.SetActive(false);

        if (playerController != null)
        {
            playerController.SetCanMove(true);
        }

        if (createdUI && canvasGroup != null)
        {
            Destroy(canvasGroup.gameObject);
        }
    }

    private static IEnumerator FadeCanvasGroup(CanvasGroup cg, float from, float to, float duration)
    {
        if (duration <= 0f) { cg.alpha = to; yield break; }

        float t = 0f;
        cg.alpha = from;
        while (t < duration)
        {
            t += Time.deltaTime;
            cg.alpha = Mathf.Lerp(from, to, t / duration);
            yield return null;
        }
        cg.alpha = to;
    }

    private void CreateOverlayUIIfMissing()
    {
        
        var canvasGO = new GameObject("IntroNarrativeCanvas", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster), typeof(CanvasGroup));
        var canvas = canvasGO.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 10000;

        var scaler = canvasGO.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);

        canvasGroup = canvasGO.GetComponent<CanvasGroup>();

        
        var panelGO = new GameObject("Background", typeof(RectTransform), typeof(Image));
        panelGO.transform.SetParent(canvasGO.transform, false);
        var rect = panelGO.GetComponent<RectTransform>();
        rect.anchorMin = Vector2.zero; rect.anchorMax = Vector2.one;
        rect.offsetMin = Vector2.zero; rect.offsetMax = Vector2.zero;

        backgroundImage = panelGO.GetComponent<Image>();
        backgroundImage.color = new Color(0f, 0f, 0f, 0.96f);

        
        var textGO = new GameObject("NarrativeText", typeof(RectTransform), typeof(TextMeshProUGUI));
        textGO.transform.SetParent(panelGO.transform, false);
        var textRect = textGO.GetComponent<RectTransform>();
        textRect.anchorMin = new Vector2(0.1f, 0.15f);
        textRect.anchorMax = new Vector2(0.9f, 0.85f);
        textRect.offsetMin = Vector2.zero; textRect.offsetMax = Vector2.zero;

        narrativeText = textGO.GetComponent<TextMeshProUGUI>();
        narrativeText.enableWordWrapping = true;
        narrativeText.alignment = TextAlignmentOptions.Center;
        narrativeText.fontSize = 36f;
        narrativeText.color = new Color(1f, 1f, 1f, 0.95f);
        narrativeText.text = storyText;
    }
}
