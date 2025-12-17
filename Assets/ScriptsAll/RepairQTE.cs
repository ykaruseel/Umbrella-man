// Файл: RepairQTE.cs (ФИНАЛЬНАЯ ИНТЕГРИРОВАННАЯ ВЕРСИЯ)
using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using FMODUnity;
using System.Collections; // Добавили для Coroutine

public class RepairQTE : MonoBehaviour
{
    // --- ССЫЛКИ И НАСТРОЙКИ ---
    [Header("UI References")]
    public GameObject qtePanel;
    public List<TrackData> tracks; // Исправлено: один список

    [Header("Game State")]
    public bool isQTEActive = false;
    private int currentTrackIndex = 0;

    [Header("Callbacks (опционально)")]
    public System.Action onSuccess;
    public System.Action onFail;

    [Header("FMOD – звуки QTE")]
    [SerializeField] private EventReference trackSuccessEvent;
    [SerializeField] private EventReference trackFailEvent;
    [SerializeField] private EventReference qteSuccessEvent;

    [Header("Щиток – световой фидбек")]
    public Light shieldLight;
    [Tooltip("Интенсивность света на щитке в спокойном состоянии")]
    public float shieldIdleIntensity = 0.05f;
    [Tooltip("Яркость вспышки при успешной полоске")]
    public float shieldSuccessIntensity = 2f;
    [Tooltip("Яркость хаотичного мигания при фейле")]
    public float shieldFailIntensity = 1.5f;
    [Tooltip("Длительность вспышки при успехе (сек)")]
    public float shieldSuccessDuration = 0.35f;
    [Tooltip("Длительность мигания при фейле (сек)")]
    public float shieldFailDuration = 0.6f;

    private float shieldBaseIntensity;
    private Coroutine shieldRoutine;

    // --- ССЫЛКИ НА МОЗГИ ---
    public QuestManager questManager;
    private PlayerController playerController; // Исправлено: одно объявление

    [System.Serializable]
    public class TrackData
    {
        public RectTransform arrow;
        public RectTransform successZone;
        public RectTransform trackBackground;
        public float speed = 300f;
        [HideInInspector] public float trackHeight;

        [Header("Условие победы")]
        public float minWinY = 45f; // допустимое отклонение по Y
    }

    // ----------------- LIFECYCLE -----------------

    void Start()
    {
        if (qtePanel != null)
            qtePanel.SetActive(false);

        // Поиск контроллеров (первый FindFirstObjectByType более современный)
        playerController = FindFirstObjectByType<PlayerController>();
        
        // Находим QuestManager
        if(QuestManager.instance != null)
            questManager = QuestManager.instance; 
        else
            Debug.LogWarning("RepairQTE: QuestManager.instance не найден!");

        // Вычисляем высоту треков
        foreach (var track in tracks)
        {
            if (track.trackBackground != null)
                track.trackHeight = track.trackBackground.rect.height;
            else if (track.arrow != null && track.arrow.parent is RectTransform rt)
                track.trackHeight = rt.rect.height;
            else
                track.trackHeight = 200f; // Дефолтное значение
        }

        // Настройка света щитка
        if (shieldLight != null)
        {
            shieldBaseIntensity = shieldLight.intensity;
            shieldLight.enabled = false;
        }
    }

    // В файле RepairQTE.cs

    void Update()
    {
        if (!isQTEActive) return;

        // ↓↓↓ Твоя логика: двигаем стрелку активного трека ↓↓↓
        if (currentTrackIndex >= 0 && currentTrackIndex < tracks.Count)
        {
            MoveArrow(tracks[currentTrackIndex]);
        }

        // ввод QTE: ПРОБЕЛ
        if (Input.GetKeyDown(KeyCode.Space))
        {
            CheckHit();
        }

        // ↓↓↓ ИСПРАВЛЕНИЕ ОШИБКИ ESC: Теперь вызываем CancelQTE() ↓↓↓
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            CancelQTE(); // FIX: Выходим без вызова финала поражения
        }
    }

    // ----------------- ПУБЛИЧНЫЙ СТАРТ / СТОП -----------------

    // --- ЛОГИКА ЗАПУСКА ---
    public void StartRepairQTE()
    {
        if (isQTEActive) return; // Не запускаем, если уже активно

        isQTEActive = true;
        currentTrackIndex = 0;
        
        if (qtePanel != null)
            qtePanel.SetActive(true);

        // БЛОКИРУЕМ игрока и камеру
        if (playerController != null)
        {
            playerController.SetCanMove(false);
            playerController.SetDialogueZoom(true);
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }

        foreach (var track in tracks)
            ResetArrowOnTrack(track);

        // Включаем лампу щитка
        if (shieldLight != null)
            shieldLight.enabled = true;

        SetShieldIdle();

        Debug.Log("RepairQTE: старт QTE, текущий трек = 1");
    }

    public void StopQTE(bool success)
    {
        isQTEActive = false;

        if (qtePanel != null)
            qtePanel.SetActive(false);

        // Разблокируем игрока и камеру
        if (playerController != null)
        {
            playerController.SetCanMove(true);
            playerController.SetDialogueZoom(false);
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }

        // Сброс стрелок
        foreach (var track in tracks)
            ResetArrowOnTrack(track);

        // Полностью гасим лампу щитка
        TurnOffShieldLight();

        // Останавливаем погоню и сердцебиение
        UmbrellaManChase chase = FindFirstObjectByType<UmbrellaManChase>();
        if (chase != null)
        {
            chase.StopChase();
            chase.StopHeartbeat();
            Debug.Log("[RepairQTE] Остановили погоню и сердцебиение после QTE");
        }

        // Сообщаем QuestManager результат QTE
        if (questManager != null)
        {
            if (success)
                questManager.OnQTESuccess();
            else
                questManager.OnQTEFailure();
        }
    }

    void TurnOffShieldLight()
    {
        if (shieldLight == null) return;

        if (shieldRoutine != null)
        {
            StopCoroutine(shieldRoutine);
            shieldRoutine = null;
        }

        shieldLight.intensity = 0f;
        shieldLight.enabled = false;
    }

    // ----------------- ДВИЖЕНИЕ СТРЕЛКИ -----------------

    void MoveArrow(TrackData track)
    {
        if (track.arrow == null || track.trackBackground == null) return;

        // Используем один расчет для PingPong
        float h = track.trackHeight;
        float pingPongValue = Mathf.PingPong(Time.time * track.speed, h);
        float newY = pingPongValue - (h / 2f); // Сдвигаем к центру

        track.arrow.anchoredPosition = new Vector2(track.arrow.anchoredPosition.x, newY);
    }

    void ResetArrowOnTrack(TrackData track)
    {
        if (track.arrow == null) return;

        float h = track.trackHeight;
        var pos = track.arrow.anchoredPosition;
        pos.y = -h / 2f; // Ставим в самый низ
        track.arrow.anchoredPosition = pos;
    }

    // ----------------- ПРОВЕРКА ПОПАДАНИЯ -----------------

    void CheckHit()
    {
        if (currentTrackIndex < 0 || currentTrackIndex >= tracks.Count) return;

        TrackData currentTrack = tracks[currentTrackIndex];

        if (currentTrack.arrow == null || currentTrack.successZone == null)
        {
            Debug.LogWarning("RepairQTE: не заданы arrow / successZone на треке " + (currentTrackIndex + 1));
            return;
        }
        
        // Получаем позицию стрелки относительно родительского трека
        float arrowY = currentTrack.arrow.anchoredPosition.y;
        
        // Получаем границы зоны успеха
        float zoneY = currentTrack.successZone.anchoredPosition.y;
        float zoneHalfHeight = currentTrack.successZone.rect.height / 2;

        // Проверка: находится ли стрелка в пределах зоны (Zone Y - half height) до (Zone Y + half height)
        if (Mathf.Abs(arrowY - zoneY) <= zoneHalfHeight) // УСЛОВИЕ УСПЕХА
        {
            // УСПЕХ ПОЛОСЫ
            Debug.Log($"TRACK {currentTrackIndex + 1}: УСПЕХ");

            // 👉 сразу играем звук успеха полоски
            if (!trackSuccessEvent.IsNull)
                FMODUnity.RuntimeManager.PlayOneShot(trackSuccessEvent);

            StartShieldSuccessFlash();
            currentTrackIndex++;

            // Финальный успех QTE
            if (currentTrackIndex >= tracks.Count)
            {
                // звук полного успеха
                if (!qteSuccessEvent.IsNull)
                    FMODUnity.RuntimeManager.PlayOneShot(qteSuccessEvent);

                StopQTE(true); // ПОЛНАЯ ПОБЕДА!
            }
            else
            {
                Debug.Log($"Переход на трек {currentTrackIndex + 1}");
                ResetArrowOnTrack(tracks[currentTrackIndex]);
            }
        }
        else
        {
            // ПРОМАХ / СБРОС
            Debug.Log($"TRACK {currentTrackIndex + 1}: ПРОМАХ, возврат на первый трек");

            if (!trackFailEvent.IsNull)
                FMODUnity.RuntimeManager.PlayOneShot(trackFailEvent);

            StartShieldFailFlash();

            // Сброс на первый трек
            currentTrackIndex = 0;
            foreach (var track in tracks)
                ResetArrowOnTrack(track);
        }
    }

    // ----------------- СВЕТ НА ЩИТКЕ -----------------
    
    // ... (Методы для света SetShieldIdle, StartShieldSuccessFlash, StartShieldFailFlash)
    // ... (Методы-Coroutine ShieldSuccessFlashCoroutine, ShieldFailFlashCoroutine)
    // ... (Оставил их без изменений, предполагая, что они не содержат ошибок синтаксиса в теле, кроме конфликта вызова StopQTE в Coroutine ShieldFailFlashCoroutine - см. ниже)

    void SetShieldIdle()
    {
        if (shieldLight == null) return;
        if (shieldRoutine != null) StopCoroutine(shieldRoutine);
        shieldLight.intensity = shieldIdleIntensity;
    }

    void StartShieldSuccessFlash()
    {
        if (shieldLight == null) return;
        if (shieldRoutine != null) StopCoroutine(shieldRoutine);
        shieldRoutine = StartCoroutine(ShieldSuccessFlashCoroutine());
    }

    void StartShieldFailFlash()
    {
        if (shieldLight == null) return;
        if (shieldRoutine != null) StopCoroutine(shieldRoutine);
        shieldRoutine = StartCoroutine(ShieldFailFlashCoroutine());
    }

    IEnumerator ShieldSuccessFlashCoroutine()
    {
        float t = 0f;
        while (t < shieldSuccessDuration)
        {
            if (playerController != null)
            {
                // плавное, но быстрое пульсирование по синусу
                float phase = Mathf.Sin(t * Mathf.PI * 4f); // несколько пульсов
                float k = Mathf.InverseLerp(-1f, 1f, phase); // 0..1
                shieldLight.intensity = Mathf.Lerp(shieldIdleIntensity, shieldSuccessIntensity, k);

                t += Time.deltaTime;
                yield return null;
                //playerController.SetDialogueZoom(false); // Убрано: SetDialogueZoom должен быть в StopQTE
            }
        }

        shieldLight.intensity = shieldIdleIntensity;
        shieldRoutine = null;
    }

    IEnumerator ShieldFailFlashCoroutine()
    {
        float t = 0f;
        bool state = false;

        while (t < shieldFailDuration)
        {
            // более "рваное" мигание при фейле
            state = !state;

            shieldLight.intensity = state ? shieldFailIntensity : 0f;

            float wait = Random.Range(0.05f, 0.12f);
            t += wait;
            yield return new WaitForSeconds(wait);
        }

        if (questManager != null)
        {
            // Эта логика должна быть в StopQTE, а не в Coroutine
            // Но чтобы починить, я оставлю здесь вызовы.
        }

        shieldLight.intensity = shieldIdleIntensity;
        shieldRoutine = null;
    }
    // В файле RepairQTE.cs (НОВАЯ ФУНКЦИЯ)

    void CancelQTE()
    {
        isQTEActive = false;
        qtePanel.SetActive(false); // Скрываем QTE панель

        // Присваиваем игроку обратно контроль движения и камеры
        if (playerController != null)
        {
            playerController.SetCanMove(true); // Разблокируем движение!
            playerController.SetDialogueZoom(false); // Выходим из зума
            // Курсор остается заблокирован, так как мы вернулись к игре
        }

        // Мы НЕ вызываем OnQTEFailure, чтобы не появлялась надпись "He got you".
        Debug.Log("QTE: Активирована Отмена. Игрок восстанавливает управление.");
    }

    public void ResetQTEState()
    {
        isQTEActive = false;
        currentTrackIndex = 0;

        if (qtePanel != null)
            qtePanel.SetActive(false);

        foreach (var track in tracks)
            ResetArrowOnTrack(track);

        TurnOffShieldLight();

        if (shieldRoutine != null)
        {
            StopCoroutine(shieldRoutine);
            shieldRoutine = null;
        }

        playerController.SetCanMove(true);
        playerController.SetDialogueZoom(false);
    }
}
