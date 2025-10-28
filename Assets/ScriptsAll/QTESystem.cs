// Файл: QTESystem.cs
using UnityEngine;
using UnityEngine.UI; // Для Slider
using TMPro; // Для текста (если нужен)
using System.Collections;

public class QTESystem : MonoBehaviour
{
    [Header("UI Элементы")]
    public GameObject qtePanel; // Панель, содержащая UI QTE
    public TextMeshProUGUI keyPromptText; // Текст, показывающий нужную клавишу (напр. "[E]")
    public Slider timerSlider; // Слайдер для отображения времени

    // Приватные переменные для логики
    private float timeLimit;
    private KeyCode requiredKey;
    private System.Action onSuccessCallback; // Функция, которая вызовется при успехе
    private System.Action onFailureCallback; // Функция, которая вызовется при провале
    private bool qteActive = false;
    private float currentTime;

    void Start()
    {
        // Прячем QTE при старте игры
        if(qtePanel) qtePanel.SetActive(false);
    }

    // Эта функция будет вызываться из QuestManager
    public void StartQTE(float duration, KeyCode key, System.Action onSuccess, System.Action onFailure)
    {
        timeLimit = duration;
        requiredKey = key;
        onSuccessCallback = onSuccess;
        onFailureCallback = onFailure;

        // Настраиваем UI
        currentTime = timeLimit;
        if(keyPromptText) keyPromptText.text = "[" + key.ToString() + "]";
        if(timerSlider) timerSlider.maxValue = timeLimit;
        if(timerSlider) timerSlider.value = timeLimit;

        // Активируем
        qteActive = true;
        if(qtePanel) qtePanel.SetActive(true);
        Debug.Log("QTE начат! Нажмите " + key.ToString());
    }

    void Update()
    {
        // Если QTE не активен, ничего не делаем
        if (!qteActive) return;

        // Отсчет времени
        currentTime -= Time.deltaTime;
        if(timerSlider) timerSlider.value = currentTime;

        // 1. Проверка на УСПЕХ (нажата правильная клавиша)
        if (Input.GetKeyDown(requiredKey))
        {
            Debug.Log("QTE - Успех!");
            qteActive = false;
            if(qtePanel) qtePanel.SetActive(false);
            onSuccessCallback?.Invoke(); // Вызываем Success
            return;
        }
        
        // 2. Проверка на ПРОВАЛ (время вышло)
        if (currentTime <= 0)
        {
            Debug.Log("QTE - Провал (Время вышло)!");
            qteActive = false;
            if(qtePanel) qtePanel.SetActive(false);
            onFailureCallback?.Invoke(); // Вызываем Failure
        }
    }
}