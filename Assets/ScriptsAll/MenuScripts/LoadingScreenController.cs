using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class LoadingScreenController : MonoBehaviour
{
    public static bool CanSwitchScenes = false;

    [Header("UI")]
    public TMP_Text technicalText;
    public TMP_Text titleLine1;
    public TMP_Text titleLine2;

    [Header("Typewriter")]
    [SerializeField] private float typeSpeed = 0.05f;
    [SerializeField] private float deleteSpeed = 0.03f;

    [Header("Messages")]
    public List<string> technicalMessages;

    [Header("Timings")]
    public float messagePause = 1.5f;
    public float titleDelay = 2f;

    [SerializeField] private PlayAndQuit loader;

    private void Start()
    {
        CanSwitchScenes = false;
    }

    public void StartSequence()
    {
        gameObject.SetActive(true);
        StartCoroutine(SequenceRoutine());
    }

    public IEnumerator SequenceRoutine()
    {
        foreach (string msg in technicalMessages)
        {
            yield return TypeText(technicalText, msg);
            yield return new WaitForSeconds(messagePause);
            yield return DeleteText(technicalText);
        }
    }

    public IEnumerator TitleWrite()
    {
        yield return new WaitForSeconds(titleDelay);
        yield return TypeText(titleLine1, "Do you know");
        yield return new WaitForSeconds(1.5f);
        yield return TypeText(titleLine2, "The man with the umbrella?");

        yield return new WaitForSeconds(2f);

        yield return DeleteText(titleLine2);
        yield return DeleteText(titleLine1);

        yield return new WaitForSeconds(2f);

        CanSwitchScenes = true;
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
