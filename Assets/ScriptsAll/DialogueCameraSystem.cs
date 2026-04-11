using UnityEngine;
using Unity.Cinemachine; // Используем новую систему Cinemachine

public class DialogueCameraSystem : MonoBehaviour
{
    [Header("Камера игрока (Standard Perspective)")]
    [Tooltip("Основная камера игрока, к которой мы вернемся после диалога")]
    public CinemachineCamera playerCamera;

    [Header("Камеры диалога (от 1 до 3)")]
    [Tooltip("Перетащи сюда заранее расставленные виртуальные камеры")]
    public CinemachineCamera[] dialogueCameras;

    [Header("Настройки переключения (Częstotliwość)")]
    [Tooltip("1 = менять при каждой фразе, 2 = каждые две фразы и т.д.")]
    public int switchFrequency = 1;

    private int currentCameraIndex = 0;
    private int linesPlayed = 0;
    private bool isDialogueActive = false;

    private void Start()
    {
        // Инициализация: жестко выключаем все диалоговые камеры при старте сцены
        foreach (var cam in dialogueCameras)
        {
            if (cam != null) cam.Priority = 0;
        }
    }

    // 1. ВЫЗЫВАТЬ ПРИ СТАРТЕ ДИАЛОГА
    public void StartDialogue()
    {
        if (dialogueCameras.Length == 0)
        {
            Debug.LogWarning("Brak kamer dialogowych! (Нет камер для диалога)");
            return;
        }

        isDialogueActive = true;
        linesPlayed = 0;
        currentCameraIndex = 0;

        // Опускаем приоритет камеры игрока
        if (playerCamera != null) playerCamera.Priority = 0;

        // Включаем первую камеру диалога
        UpdateCameras();
    }

    // 2. ВЫЗЫВАТЬ ПРИ КАЖДОМ ПЕРЕКЛЮЧЕНИИ ФРАЗЫ В UI
    public void NextLine()
    {
        if (!isDialogueActive || dialogueCameras.Length == 0) return;

        linesPlayed++;

        // Проверяем интервал переключения (switchFrequency)
        if (linesPlayed % switchFrequency == 0)
        {
            // Переходим к следующей камере по кругу
            currentCameraIndex = (currentCameraIndex + 1) % dialogueCameras.Length;
            UpdateCameras();
        }
    }

    // 3. ВЫЗЫВАТЬ ПРИ ЗАВЕРШЕНИИ ДИАЛОГА
    public void EndDialogue()
    {
        isDialogueActive = false;

        // Выключаем все кинокамеры
        foreach (var cam in dialogueCameras)
        {
            if (cam != null) cam.Priority = 0;
        }

        // Возвращаем приоритет камере игрока
        if (playerCamera != null) playerCamera.Priority = 100;
    }

    // Внутренняя функция переключения приоритетов
    private void UpdateCameras()
    {
        for (int i = 0; i < dialogueCameras.Length; i++)
        {
            if (dialogueCameras[i] != null)
            {
                // Главной становится только текущая камера (Priority = 100), остальные гаснут (0)
                dialogueCameras[i].Priority = (i == currentCameraIndex) ? 100 : 0;
            }
        }
    }
}
