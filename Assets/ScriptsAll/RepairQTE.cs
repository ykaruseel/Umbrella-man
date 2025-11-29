// Файл: RepairQTE.cs (ФИНАЛЬНАЯ ИНТЕГРИРОВАННАЯ ВЕРСИЯ)
using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class RepairQTE : MonoBehaviour
{
    [Header("UI References")]
    public GameObject qtePanel;
    public List<TrackData> tracks; 

    [Header("Game State")]
    public bool isQTEActive = false;
    private int currentTrackIndex = 0;
    
    // --- ССЫЛКИ НА МОЗГИ ---
    public QuestManager questManager; 
    private PlayerController playerController; 

    [System.Serializable]
    public class TrackData
    {
        public RectTransform arrow;      
        public RectTransform successZone;
        public RectTransform trackBackground;
        public float speed = 300f;       
        [HideInInspector] public float trackHeight;
        public float minWinY = 0f;
    }

    void Start()
    {
        qtePanel.SetActive(false);
        // Находим игрока
        playerController = FindObjectOfType<PlayerController>();
        // Находим QuestManager
        questManager = QuestManager.instance; 

        // Вычисляем высоту треков
        foreach (var track in tracks)
        {
            track.trackHeight = track.trackBackground.rect.height; 
        }
    }

    void Update()
    {
        if (!isQTEActive) return;

        MoveArrow(tracks[currentTrackIndex]);

        // ↓↓↓ ФИКС: СЛУШАЕМ ПРОБЕЛ (Space), чтобы избежать конфликта с E ↓↓↓
        if (Input.GetKeyDown(KeyCode.Space))
        {
            CheckHit();
        }

        // Слушаем выход (Esc)
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            StopQTE(false); // Отмена = Провал
        }
    }

    // --- ЛОГИКА ЗАПУСКА ---
    public void StartRepairQTE()
    {
        isQTEActive = true;
        currentTrackIndex = 0;
        qtePanel.SetActive(true);

        // БЛОКИРУЕМ игрока и камеру
        if (playerController != null)
        {
            playerController.SetCanMove(false);
            playerController.SetDialogueZoom(true); 
            Cursor.lockState = CursorLockMode.Locked; 
            Cursor.visible = false;
        }
        
        // Сброс стрелок
        foreach(var track in tracks)
        {
            track.arrow.anchoredPosition = new Vector2(track.arrow.anchoredPosition.x, -track.trackHeight / 2);
        }
    }

    void MoveArrow(TrackData track)
    {
        float pingPongValue = Mathf.PingPong(Time.time * track.speed, track.trackHeight);
        float newY = pingPongValue - (track.trackHeight / 2);
        track.arrow.anchoredPosition = new Vector2(track.arrow.anchoredPosition.x, newY);
    }

    // --- ЛОГИКА ПРОВЕРКИ ---
    void CheckHit() 
    {
        TrackData currentTrack = tracks[currentTrackIndex];

        float arrowY = currentTrack.arrow.anchoredPosition.y;
        float zoneY = currentTrack.successZone.anchoredPosition.y;
        float zoneHalfHeight = currentTrack.successZone.rect.height / 2;

        // Здесь ты можешь вставить Debug.Log для настройки координат
        // Debug.Log($"ПРОВЕРКА TRACK {currentTrackIndex + 1}: Стрелка Y={arrowY:F1}, Зона Y={zoneY:F1}..."); 

        if (Mathf.Abs(arrowY - zoneY) <= zoneHalfHeight) // УСЛОВИЕ УСПЕХА
        {
            Debug.Log($"ПОПАЛ! (Track {currentTrackIndex + 1})");
            currentTrackIndex++; 

            if (currentTrackIndex >= tracks.Count)
            {
                StopQTE(true); // ПОБЕДА!
            }
        }
        else
        {
            // ПРОВАЛ! (Сброс на первый уровень)
            Debug.Log("МИМО! Сброс.");
            currentTrackIndex = 0; 
        }
    }
    
    // --- ЛОГИКА ОСТАНОВКИ (Запускаем финальный ивент) ---
    void StopQTE(bool success)
    {
        isQTEActive = false;
        qtePanel.SetActive(false);

        // Разблокируем камеру и зум (движение разблокируется в QuestManager)
        if (playerController != null)
        {
            playerController.SetDialogueZoom(false);
        }

        if (questManager != null)
        {
            if (success)
            {
                // Успех QTE -> ЗАПУСК ХОРОШЕЙ КОНЦОВКИ
                questManager.OnQTESuccess(); 
            }
            else
            {
                // Провал QTE -> ЗАПУСК ПЛОХОЙ КОНЦОВКИ
                questManager.OnQTEFailure();
            }
        }
        
        Debug.Log("QTE завершено.");
    }
}
