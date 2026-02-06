using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using FMODUnity;
using System.Collections; 

public class RepairQTE : MonoBehaviour
{
    // --- ССЫЛКИ И НАСТРОЙКИ ---
    [Header("UI References")]
    public GameObject qtePanel;
    public List<TrackData> tracks; 

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

    [Header("UI – лампочка")]
    [SerializeField] private Animator lampAnimator;

    private float shieldBaseIntensity;
    private Coroutine shieldRoutine;

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

        [Header("Условие победы")]
        public float minWinY = 45f; 
    }

    // ----------------- LIFECYCLE -----------------

    void Start()
    {
        if (qtePanel != null)
            qtePanel.SetActive(false);

        playerController = FindFirstObjectByType<PlayerController>();

        if (QuestManager.instance != null)
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
                track.trackHeight = 200f; 
        }

        // Настройка света щитка
        if (shieldLight != null)
        {
            shieldBaseIntensity = shieldLight.intensity;
            shieldLight.enabled = false;
        }
    }

    void Update()
    {
        // [FIX 1] Если игра закончилась (смерть) во время QTE — вырубаем всё
        if (PlayerController.isGameEnded && isQTEActive)
        {
            ForceStopQTE();
            return;
        }

        if (!isQTEActive) return;

        // Двигаем стрелку
        if (currentTrackIndex >= 0 && currentTrackIndex < tracks.Count)
        {
            MoveArrow(tracks[currentTrackIndex]);
        }

        // Ввод QTE
        if (Input.GetKeyDown(KeyCode.Space))
        {
            CheckHit();
        }

        // Отмена на ESC
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            CancelQTE(); 
        }
    }

    // ----------------- ПУБЛИЧНЫЙ СТАРТ / СТОП -----------------

    public void StartRepairQTE()
    {
        if (isQTEActive) return; 
        
        // [FIX 2] Не запускаем, если игра уже кончилась
        if (PlayerController.isGameEnded) return; 

        isQTEActive = true;
        currentTrackIndex = 0;

        if (qtePanel != null)
            qtePanel.SetActive(true);

        if (playerController != null)
        {
            playerController.SetCanMove(false);
            playerController.SetDialogueZoom(true);
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }

        foreach (var track in tracks)
            ResetArrowOnTrack(track);

        if (shieldLight != null)
            shieldLight.enabled = true;

        SetShieldIdle();

        Debug.Log("RepairQTE: старт QTE");
    }

    public void StopQTE(bool success)
    {
        // [FIX 3] ГЛАВНАЯ ПРОВЕРКА
        // Если враг уже убил нас (флаг true), то запрещаем выигрывать.
        if (PlayerController.isGameEnded) return;

        // Если мы выиграли, ставим флаг, чтобы враг теперь НЕ мог убить нас
        if (success)
        {
            PlayerController.isGameEnded = true;
        }
        // -----------------------

        isQTEActive = false;

        if (qtePanel != null)
            qtePanel.SetActive(false);

        // Разблокировка управления (только если не победа, т.к. при победе обычно катсцена/титры)
        // Но если хочешь, чтобы при победе игрок мог ходить — раскомментируй блок ниже для success тоже.
        if (!success && playerController != null)
        {
            playerController.SetCanMove(true);
            playerController.SetDialogueZoom(false);
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
        // Для победы (Prototype Complete) обычно управление блокируют, поэтому оставим игрока стоять.

        foreach (var track in tracks)
            ResetArrowOnTrack(track);

        TurnOffShieldLight();

        UmbrellaManChase chase = FindFirstObjectByType<UmbrellaManChase>();
        if (chase != null)
        {
            chase.StopChase();
            chase.StopHeartbeat();
        }

        if (questManager != null)
        {
            if (success)
                questManager.OnQTESuccess();
            else
                questManager.OnQTEFailure();
        }
    }

    // Метод для экстренного закрытия при смерти
    void ForceStopQTE()
    {
        isQTEActive = false;
        if (qtePanel != null) qtePanel.SetActive(false);
        TurnOffShieldLight();
        // Управление игроку НЕ возвращаем, так как он умер
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

        float h = track.trackHeight;
        float pingPongValue = Mathf.PingPong(Time.time * track.speed, h);
        float newY = pingPongValue - (h / 2f); 

        track.arrow.anchoredPosition = new Vector2(track.arrow.anchoredPosition.x, newY);
    }

    void ResetArrowOnTrack(TrackData track)
    {
        if (track.arrow == null) return;

        float h = track.trackHeight;
        var pos = track.arrow.anchoredPosition;
        pos.y = -h / 2f; 
        track.arrow.anchoredPosition = pos;
    }

    // ----------------- ПРОВЕРКА ПОПАДАНИЯ -----------------

    void CheckHit()
    {
        if (currentTrackIndex < 0 || currentTrackIndex >= tracks.Count) return;

        TrackData currentTrack = tracks[currentTrackIndex];

        if (currentTrack.arrow == null || currentTrack.successZone == null)
        {
            Debug.LogWarning("RepairQTE: не заданы настройки трека!");
            return;
        }

        float arrowY = currentTrack.arrow.anchoredPosition.y;
        float zoneY = currentTrack.successZone.anchoredPosition.y;
        float zoneHalfHeight = currentTrack.successZone.rect.height / 2;

        // Проверка попадания
        if (Mathf.Abs(arrowY - zoneY) <= zoneHalfHeight) 
        {
            // УСПЕХ
            if (!trackSuccessEvent.IsNull)
                FMODUnity.RuntimeManager.PlayOneShot(trackSuccessEvent);

            if (lampAnimator != null)
            {
                lampAnimator.ResetTrigger("Success");
                lampAnimator.SetTrigger("Success");
            }

            StartShieldSuccessFlash();
            currentTrackIndex++;

            if (currentTrackIndex >= tracks.Count)
            {
                // ПОЛНАЯ ПОБЕДА
                if (!qteSuccessEvent.IsNull)
                    FMODUnity.RuntimeManager.PlayOneShot(qteSuccessEvent);

                StopQTE(true); 
            }
            else
            {
                ResetArrowOnTrack(tracks[currentTrackIndex]);
            }
        }
        else
        {
            // ПРОМАХ
            if (!trackFailEvent.IsNull)
                FMODUnity.RuntimeManager.PlayOneShot(trackFailEvent);

            StartShieldFailFlash();

            currentTrackIndex = 0;
            foreach (var track in tracks)
                ResetArrowOnTrack(track);
        }
    }

    // ----------------- СВЕТ НА ЩИТКЕ -----------------

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
            // Плавное пульсирование
             float phase = Mathf.Sin(t * Mathf.PI * 4f); 
             float k = Mathf.InverseLerp(-1f, 1f, phase); 
             shieldLight.intensity = Mathf.Lerp(shieldIdleIntensity, shieldSuccessIntensity, k);
             t += Time.deltaTime;
             yield return null;
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
            // Резкое мигание
            state = !state;
            shieldLight.intensity = state ? shieldFailIntensity : 0f;
            float wait = Random.Range(0.05f, 0.12f);
            t += wait;
            yield return new WaitForSeconds(wait);
        }
        shieldLight.intensity = shieldIdleIntensity;
        shieldRoutine = null;
    }

    // Отмена QTE на ESC
    void CancelQTE()
    {
        isQTEActive = false;
        qtePanel.SetActive(false); 

        if (playerController != null)
        {
            playerController.SetCanMove(true); 
            playerController.SetDialogueZoom(false); 
        }
        // Не вызываем Fail, просто закрываем
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

        if (playerController != null)
        {
            playerController.SetCanMove(true);
            playerController.SetDialogueZoom(false);
        }
    }
}
