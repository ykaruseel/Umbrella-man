
using System.Collections;
using UnityEngine;
using TMPro;
using FMODUnity;

public class FlashbackTitleSequence : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private CanvasGroup canvasGroup;
    [SerializeField] private TMP_Text titleLine1;
    [SerializeField] private TMP_Text titleLine2;

    [Header("Title")]
    [SerializeField] private string firstLine =
        "Do you know";

    [SerializeField] private string secondLine =
        "THE MAN WITH THE\nUMBRELLA?";

    [Header("Timing")]
    [SerializeField] private float typeSpeed = 0.06f;
    [SerializeField] private float betweenLinesDelay = 0.5f;
    [SerializeField] private float titleHoldDuration = 2f;
    [SerializeField] private float deleteSpeed = 0.04f;

    [Header("FMOD")]
    [SerializeField] private EventReference typewriterSound;

    private bool played = false;

    private void Awake()
    {
        if (canvasGroup != null)
        {
            canvasGroup.alpha = 0f;
            canvasGroup.interactable = false;
            canvasGroup.blocksRaycasts = false;
        }

        if (titleLine1 != null)
            titleLine1.text = "";

        if (titleLine2 != null)
            titleLine2.text = "";
    }

    public IEnumerator PlaySequence()
    {
        if (played)
            yield break;

        if (canvasGroup == null ||
            titleLine1 == null ||
            titleLine2 == null)
        {
            Debug.LogError(
                "[FlashbackTitle] UI references are missing!"
            );

            yield break;
        }

        played = true;

        Debug.Log("[FlashbackTitle] Sequence started");

        titleLine1.text = "";
        titleLine2.text = "";
        titleLine1.maxVisibleCharacters = 0;
        titleLine2.maxVisibleCharacters = 0;

        // Show black background.
        canvasGroup.alpha = 1f;
        canvasGroup.blocksRaycasts = true;

        // Type the first line.
        yield return StartCoroutine(
            TypeText(titleLine1, firstLine)
        );

        yield return new WaitForSeconds(
            betweenLinesDelay
        );

        // Type the second line.
        yield return StartCoroutine(
            TypeText(titleLine2, secondLine)
        );

        // Keep the complete title visible.
        yield return new WaitForSeconds(
            titleHoldDuration
        );

        // Delete in reverse order.
        yield return StartCoroutine(
            DeleteText(titleLine2)
        );

        yield return StartCoroutine(
            DeleteText(titleLine1)
        );

        // Hide the title.
        canvasGroup.alpha = 0f;
        canvasGroup.blocksRaycasts = false;

        Debug.Log("[FlashbackTitle] Sequence finished");
    }

   private IEnumerator TypeText(
    TMP_Text target,
    string content)
{
    target.text = content;
    target.maxVisibleCharacters = 0;
    target.ForceMeshUpdate();

    int count = target.textInfo.characterCount;

    for (int i = 1; i <= count; i++)
    {
        target.maxVisibleCharacters = i;

        char c = content[i - 1];

        if (c != ' ' && c != '\n' &&
            !typewriterSound.IsNull)
        {
            RuntimeManager.PlayOneShot(typewriterSound);
        }

        yield return new WaitForSeconds(typeSpeed);
    }

    target.maxVisibleCharacters = int.MaxValue;
}

    private IEnumerator DeleteText(TMP_Text target)
    {
        while (target.text.Length > 0)
        {
            target.text = target.text.Substring(
                0,
                target.text.Length - 1
            );

            yield return new WaitForSeconds(deleteSpeed);
        }
    }

    [ContextMenu("TEST: Play Flashback Title")]
    private void TestSequence()
    {
        if (Application.isPlaying)
            StartCoroutine(PlaySequence());
    }
}
