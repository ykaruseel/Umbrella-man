
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class QTESystem : MonoBehaviour
{
    [Header("UI ��������")]
    public GameObject qtePanel;
    public TextMeshProUGUI keyPromptText;
    public Slider timerSlider;

    private float timeLimit;
    private KeyCode requiredKey;
    private System.Action onSuccessCallback;
    private System.Action onFailureCallback;
    private bool qteActive = false;
    private float currentTime;

    void Start()
    {
        if (qtePanel) qtePanel.SetActive(false);
    }

    public void StartQTE(float duration, KeyCode key, System.Action onSuccess, System.Action onFailure)
    {
        timeLimit = duration;
        requiredKey = key;
        onSuccessCallback = onSuccess;
        onFailureCallback = onFailure;

        currentTime = timeLimit;
        if (keyPromptText) keyPromptText.text = "[" + key.ToString() + "]";
        if (timerSlider) { timerSlider.maxValue = timeLimit; timerSlider.value = timeLimit; }

        qteActive = true;
        if (qtePanel) qtePanel.SetActive(true);
        Debug.Log("QTE �����! ������� " + key.ToString());
    }

    void Update()
    {
        if (!qteActive) return;

        currentTime -= Time.deltaTime;
        if (timerSlider) timerSlider.value = currentTime;

        if (Input.GetKeyDown(requiredKey))
        {
            qteActive = false;
            if (qtePanel) qtePanel.SetActive(false);
            onSuccessCallback?.Invoke();
            return;
        }

        if (currentTime <= 0)
        {
            qteActive = false;
            if (qtePanel) qtePanel.SetActive(false);
            onFailureCallback?.Invoke();
        }
    }
}
