using UnityEngine;
using TMPro; // Обязательно для TextMeshPro

public class SkipPulse : MonoBehaviour
{
    [Header("Компоненты")]
    public TMP_Text textComponent;

    [Header("Настройки Пульсации")]
    [Tooltip("Как быстро мигает текст")]
    public float speed = 2.0f; 
    
    [Tooltip("Минимальная прозрачность (самая тусклая точка)")]
    [Range(0f, 1f)]
    public float minAlpha = 0.2f; // Ставим мало, чтобы было "еле заметно"

    [Tooltip("Максимальная прозрачность (самая яркая точка)")]
    [Range(0f, 1f)]
    public float maxAlpha = 0.7f; // Не 1.0, чтобы не было слишком ярко

    [Header("Настройки Появления (Fade In)")]
    [Tooltip("За сколько секунд текст появится после открытия диалога")]
    public float fadeInDuration = 1.5f; 
    
    private float currentFadeTime;

    void Start()
    {
        // Если забыл привязать вручную, пробуем найти сами
        if (textComponent == null)
            textComponent = GetComponent<TMP_Text>();
    }

    void OnEnable()
    {
        // Сбрасываем таймер появления каждый раз, когда диалог включается
        currentFadeTime = 0f;
        
        // Сразу делаем текст невидимым, чтобы он начал проявляться
        SetAlpha(0f);
    }

    void Update()
    {
        if (textComponent == null) return;

        // 1. Логика плавного входа (Intro Fade In)
        // Текст постепенно проявляется от 0 до 1 (множитель)
        float introMultiplier = 1f;
        if (currentFadeTime < fadeInDuration)
        {
            currentFadeTime += Time.deltaTime;
            introMultiplier = currentFadeTime / fadeInDuration;
            // Можно добавить плавности (Ease Out)
            introMultiplier = Mathf.SmoothStep(0f, 1f, introMultiplier);
        }

        // 2. Логика пульсации (Sine Wave)
        // Mathf.Sin дает значения от -1 до 1. Превращаем это в диапазон 0..1
        float wave = (Mathf.Sin(Time.time * speed) + 1f) / 2f;
        
        // Интерполируем между min и max прозрачностью
        float pulseAlpha = Mathf.Lerp(minAlpha, maxAlpha, wave);

        // 3. Итоговая прозрачность
        // Умножаем пульсацию на интро (пока интро идет, альфа будет расти от 0 до пульсации)
        float finalAlpha = pulseAlpha * introMultiplier;

        SetAlpha(finalAlpha);
    }

    private void SetAlpha(float alpha)
    {
        if (textComponent != null)
        {
            Color c = textComponent.color;
            c.a = alpha;
            textComponent.color = c;
        }
    }
}
