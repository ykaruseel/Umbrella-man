// Файл: RepairQTE.cs (ПОЛНАЯ ЧИСТАЯ ВЕРСИЯ С ЛОГАМИ)
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
    private PlayerController playerController; 

    [System.Serializable]
    public class TrackData
    {
        public RectTransform arrow;      
        public RectTransform successZone;
        public RectTransform trackBackground;
        public float speed = 300f;       
        [HideInInspector] public float trackHeight;
        
        [Header("Условие Победы (новая логика)")]
        public float minWinY = 45f;
    }

    void Start()
    {
        qtePanel.SetActive(false);
        playerController = FindObjectOfType<PlayerController>();

        // Вычисляем высоту треков заранее
        foreach (var track in tracks)
        {
            track.trackHeight = track.trackBackground.rect.height; 
        }
    }

    void Update()
    {
        if (!isQTEActive) return;

        MoveArrow(tracks[currentTrackIndex]);

        if (Input.GetKeyDown(KeyCode.E))
        {
            CheckHit();
        }

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            StopQTE(false);
        }
    }

    public void StartRepairQTE()
    {
        isQTEActive = true;
        currentTrackIndex = 0;
        qtePanel.SetActive(true);

        if (playerController != null)
        {
            playerController.SetCanMove(false);
            playerController.SetDialogueZoom(true); 
            Cursor.lockState = CursorLockMode.Locked; 
            Cursor.visible = false;
        }
        
        // Сбрасываем стрелки в начало
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

        // Получаем позиции по Y
        float arrowY = currentTrack.arrow.anchoredPosition.y;
        float zoneY = currentTrack.successZone.anchoredPosition.y;
        float zoneHalfHeight = currentTrack.successZone.rect.height / 2;

        // --- ШПИОНСКИЙ ЛОГ (Покажет, что сравнивается) ↓↓↓ ---
        float diff = Mathf.Abs(arrowY - zoneY);
        Debug.Log($"ПРОВЕРКА TRACK {currentTrackIndex + 1}: Стрелка Y={arrowY:F1}, Зона Y={zoneY:F1}. Разница={diff:F1}. Допустимо={zoneHalfHeight:F1}");
        // -----------------------------------------------------

        if (diff <= zoneHalfHeight)
        {
            // УСПЕХ!
            Debug.Log($"ПОПАЛ! (Track {currentTrackIndex + 1})");
            currentTrackIndex++; // Переходим к следующему

            if (currentTrackIndex >= tracks.Count)
            {
                StopQTE(true); // Победа!
            }
        }
        else
        {
            // ПРОВАЛ! (Сброс на первый уровень)
            Debug.Log("МИМО! Сброс.");
            currentTrackIndex = 0;
        }
    }
    
    // --- ЛОГИКА ОСТАНОВКИ ---
    void StopQTE(bool success)
    {
        isQTEActive = false;
        qtePanel.SetActive(false);

        // Разблокируем игрока
        if (playerController != null)
        {
            playerController.SetCanMove(true);
            playerController.SetDialogueZoom(false);
        }

        if (success)
        {
            Debug.Log("ЩИТОК ПОЧИНЕН!");
            // Тут можно вызвать QuestManager.instance.UpdateQuestProgress("Panel", ObjectiveType.Interact);
        }
        else
        {
            Debug.Log("QTE отменено или провалено.");
        }
    }
}
