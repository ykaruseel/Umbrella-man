using FMODUnity;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class IntroText : MonoBehaviour
{
    [SerializeField] private TMP_Text technicalText;

    [Header("Typewriter")]
    [SerializeField] private float typeSpeed = 0.05f;
    [SerializeField] private float deleteSpeed = 0.03f;
    [SerializeField] private float messagePause = 1.5f;

    [Header("Messages")]
    [SerializeField] private List<string> technicalMessages;

    [SerializeField] private GameObject IntroTextGO;
    [SerializeField] private EventReference typewriterEvent;


    public void StartIntroText()
    {
        StartCoroutine(SequenceRoutine(2f));
        IntroTextGO.GetComponent<CanvasGroup>().alpha = 1f;
    }
    public void EndIntroText()
    {
        IntroTextGO.GetComponent<CanvasGroup>().alpha = 0f;
    }
    public IEnumerator SequenceRoutine(float t)
    {
        yield return new WaitForSeconds(t);

        IntroTextGO.SetActive(true);

        foreach (string msg in technicalMessages)
        {
            yield return TypeText(technicalText, msg);
            yield return new WaitForSeconds(messagePause);
            StartCoroutine(FadeCanvasGroup(IntroTextGO.GetComponent<CanvasGroup>(), 1f, 0f));
            yield return new WaitForSeconds(2f);
        }

        IntroTextGO.SetActive(false);
    }

    IEnumerator TypeText(TMP_Text text, string content)
    {
        text.text = "";

        IntroTextGO.GetComponent<CanvasGroup>().alpha = 1f;

        foreach (char c in content)
        {
            text.text += c;

            if (!typewriterEvent.IsNull && c != ' ')
                RuntimeManager.PlayOneShot(typewriterEvent);

            yield return new WaitForSeconds(typeSpeed);
        }
    }

    private IEnumerator FadeCanvasGroup(CanvasGroup cg, float start, float end)
    {
        float timer = 0f;
        cg.alpha = start;

        while (timer < 2f)
        {
            timer += Time.deltaTime;
            cg.alpha = Mathf.Lerp(start, end, timer / 2f);
            yield return null;
        }

        cg.alpha = end;
    }

    IEnumerator DeleteText(TMP_Text text)
    {
        while (text.text.Length > 0)
        {
            text.text = text.text.Substring(0, text.text.Length - 1);
            yield return new WaitForSeconds(deleteSpeed);
        }
    }

}
