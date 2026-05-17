using FMODUnity;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class EndGame : MonoBehaviour
{
    [SerializeField] private TMP_Text technicalText;

    [Header("Typewriter")]
    [SerializeField] private float typeSpeed = 0.05f;
    [SerializeField] private float deleteSpeed = 0.03f;
    [SerializeField] private float messagePause = 1.5f;

    [Header("Messages")]
    [SerializeField] private string technicalMessages;

    [SerializeField] private GameObject Buttons;
    [SerializeField] private EventReference typewriterEvent;

    private void Awake()
    {
        StartCoroutine(SequenceRoutine());
    }

    public IEnumerator SequenceRoutine()
    {
        yield return TypeText(technicalText, technicalMessages);
        yield return new WaitForSeconds(messagePause);

        StartCoroutine(FadeCanvasGroup(Buttons.GetComponent<CanvasGroup>(), 0f, 1f));

        yield return new WaitForSeconds(2f);

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    IEnumerator TypeText(TMP_Text text, string content)
    {
        text.text = "";

        foreach (char c in content)
        {
            text.text += c;

            if (!typewriterEvent.IsNull && c != ' ')
            {
                RuntimeManager.PlayOneShot(typewriterEvent, Camera.main.transform.position);
            }

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
}
