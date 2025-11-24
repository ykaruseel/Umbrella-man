// RepairQTE.cs
using UnityEngine;
using System.Collections.Generic;
using FMODUnity;

public class RepairQTE : MonoBehaviour
{
    [Header("UI References")]
    public GameObject qtePanel;
    public List<TrackData> tracks;

    [Header("Game State")]
    public bool isQTEActive = false;
    private int currentTrackIndex = 0;
    private PlayerController playerController;

    [Header("Callbacks (опционально)")]
    public System.Action onSuccess;
    public System.Action onFail;

    [Header("FMOD – звуки QTE")]
    [SerializeField] private EventReference trackSuccessEvent;  // успех полоски (кроме финальной)
    [SerializeField] private EventReference trackFailEvent;     // промах / сброс
    [SerializeField] private EventReference qteSuccessEvent;    // полный успех QTE

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

    [System.Serializable]
    public class TrackData
    {
        public RectTransform arrow;           // белая полоска (стрелка)
        public RectTransform successZone;     // серая зона
        public RectTransform trackBackground; // фон трека (Track_1/2/3)
        public float speed = 300f;            // скорость движения стрелки

        [HideInInspector] public float trackHeight;

        [Header("Условие победы")]
        public float minWinY = 45f;           // допустимое отклонение по Y (локально в треке)
    }

    // ----------------- LIFECYCLE -----------------

    void Start()
    {
        if (qtePanel != null)
            qtePanel.SetActive(false);

        playerController = FindFirstObjectByType<PlayerController>();

        foreach (var track in tracks)
        {
            if (track.trackBackground != null)
                track.trackHeight = track.trackBackground.rect.height;
            else if (track.arrow != null && track.arrow.parent is RectTransform rt)
                track.trackHeight = rt.rect.height;
            else
                track.trackHeight = 200f;
        }

        // Щиток по умолчанию ВЫКЛЮЧЕН
        if (shieldLight != null)
        {
            shieldBaseIntensity = shieldLight.intensity;
            shieldLight.enabled = false;          // <- главное изменение
        }
    }

    void Update()
    {
        if (!isQTEActive) return;

        // двигаем стрелку активного трека
        if (currentTrackIndex >= 0 && currentTrackIndex < tracks.Count)
        {
            MoveArrow(tracks[currentTrackIndex]);
        }

        // ввод QTE: ПРОБЕЛ
        if (Input.GetKeyDown(KeyCode.Space))
        {
            CheckHit();
        }

        // отмена QTE
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            StopQTE(false);
        }
    }

    // ----------------- ПУБЛИЧНЫЙ СТАРТ / СТОП -----------------

    public void StartRepairQTE()
    {
        if (qtePanel != null)
            qtePanel.SetActive(true);

        isQTEActive = true;
        currentTrackIndex = 0;

        if (playerController != null)
        {
            playerController.SetCanMove(false);
            playerController.SetDialogueZoom(true);
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }

        foreach (var track in tracks)
            ResetArrowOnTrack(track);

        // Включаем лампу щитка ТОЛЬКО на время QTE
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

        if (playerController != null)
        {
            playerController.SetCanMove(true);
            playerController.SetDialogueZoom(false);
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }

        // Вместо SetShieldIdle() – полностью гасим лампу
        TurnOffShieldLight();

        if (success)
        {
            Debug.Log("RepairQTE: QTE успешно завершён");
            onSuccess?.Invoke();
        }
        else
        {
            Debug.Log("RepairQTE: QTE провален / прерван");
            onFail?.Invoke();
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

        float h = track.trackHeight;
        float pingPongValue = Mathf.PingPong(Time.time * track.speed, h);
        float newY = pingPongValue - (h / 2f);

        var pos = track.arrow.anchoredPosition;
        pos.y = newY;
        track.arrow.anchoredPosition = pos;
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
            Debug.LogWarning("RepairQTE: не заданы arrow / successZone на треке " + (currentTrackIndex + 1));
            return;
        }

        RectTransform trackRoot = currentTrack.trackBackground;
        if (trackRoot == null)
        {
            trackRoot = currentTrack.arrow.parent as RectTransform;
        }

        if (trackRoot == null)
        {
            Debug.LogWarning("RepairQTE: у трека нет общего RectTransform-родителя, сравнение по Y может быть некорректным.");
        }

        // Переводим обе точки в локальные координаты трека
        Vector3 arrowLocal = trackRoot.InverseTransformPoint(currentTrack.arrow.position);
        Vector3 zoneLocal = trackRoot.InverseTransformPoint(currentTrack.successZone.position);

        float diff = Mathf.Abs(arrowLocal.y - zoneLocal.y);

        Debug.Log(
            $"ПРОВЕРКА TRACK {currentTrackIndex + 1}: " +
            $"ArrowLocalY={arrowLocal.y:F1}, ZoneLocalY={zoneLocal.y:F1}, " +
            $"Diff={diff:F1}, Allowed={currentTrack.minWinY:F1}"
        );

        if (diff <= currentTrack.minWinY)
        {
            // УСПЕХ ПОЛОСЫ
            Debug.Log($"TRACK {currentTrackIndex + 1}: УСПЕХ");

            // 👉 сразу играем звук успеха полоски
            if (!trackSuccessEvent.IsNull)
            {
                Debug.Log("RepairQTE: Play trackSuccessEvent (any success)");
                FMODUnity.RuntimeManager.PlayOneShot(trackSuccessEvent);
            }
            else
            {
                Debug.LogWarning("RepairQTE: trackSuccessEvent не назначен");
            }

            currentTrackIndex++;

            // Финальный успех QTE
            if (currentTrackIndex >= tracks.Count)
            {
                // звук полного успеха поверх (если нужно)
                if (!qteSuccessEvent.IsNull)
                {
                    Debug.Log("RepairQTE: Play qteSuccessEvent");
                    FMODUnity.RuntimeManager.PlayOneShot(qteSuccessEvent);
                }

                StartShieldSuccessFlash();
                StopQTE(true);
            }
            else
            {
                StartShieldSuccessFlash();
                Debug.Log($"Переход на трек {currentTrackIndex + 1}");
                ResetArrowOnTrack(tracks[currentTrackIndex]);
            }
        }
        else
        {
            // ПРОМАХ / СБРОС
            Debug.Log($"TRACK {currentTrackIndex + 1}: ПРОМАХ, возврат на первый трек");

            if (!trackFailEvent.IsNull)
            {
                Debug.Log("RepairQTE: Play trackFailEvent");
                RuntimeManager.PlayOneShot(trackFailEvent);
            }
            else
            {
                Debug.LogWarning("RepairQTE: trackFailEvent не назначен");
            }

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

        if (shieldRoutine != null)
            StopCoroutine(shieldRoutine);

        shieldLight.intensity = shieldIdleIntensity;
    }

    void StartShieldSuccessFlash()
    {
        if (shieldLight == null) return;

        if (shieldRoutine != null)
            StopCoroutine(shieldRoutine);

        shieldRoutine = StartCoroutine(ShieldSuccessFlashCoroutine());
    }

    void StartShieldFailFlash()
    {
        if (shieldLight == null) return;

        if (shieldRoutine != null)
            StopCoroutine(shieldRoutine);

        shieldRoutine = StartCoroutine(ShieldFailFlashCoroutine());
    }

    System.Collections.IEnumerator ShieldSuccessFlashCoroutine()
    {
        float t = 0f;

        while (t < shieldSuccessDuration)
        {
            // плавное, но быстрое пульсирование по синусу
            float phase = Mathf.Sin(t * Mathf.PI * 4f); // несколько пульсов
            float k = Mathf.InverseLerp(-1f, 1f, phase); // 0..1
            shieldLight.intensity = Mathf.Lerp(shieldIdleIntensity, shieldSuccessIntensity, k);

            t += Time.deltaTime;
            yield return null;
        }

        shieldLight.intensity = shieldIdleIntensity;
        shieldRoutine = null;
    }

    System.Collections.IEnumerator ShieldFailFlashCoroutine()
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

        shieldLight.intensity = shieldIdleIntensity;
        shieldRoutine = null;
    }
}
