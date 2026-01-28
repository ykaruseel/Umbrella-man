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

    private void Start()
    {
        StartCoroutine(SequenceRoutine());
    }

    public IEnumerator SequenceRoutine()
    {
        yield return new WaitForSeconds(2f);

        IntroTextGO.SetActive(true);

        foreach (string msg in technicalMessages)
        {
            yield return TypeText(technicalText, msg);
            yield return new WaitForSeconds(messagePause);
            yield return DeleteText(technicalText);
        }

        IntroTextGO.SetActive(false);
    }

    IEnumerator TypeText(TMP_Text text, string content)
    {
        text.text = "";
        foreach (char c in content)
        {
            text.text += c;
            yield return new WaitForSeconds(typeSpeed);
        }
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
