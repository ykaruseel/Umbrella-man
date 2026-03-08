using UnityEngine;
using TMPro;

public class SkipPulse : MonoBehaviour
{
    [Header("Компоненты")]
    public TMP_Text textComponent;

    [Header("Настройки Пульсации")]
    [Tooltip("Как быстро мигает текст")]
    public float speed = 2.0f; 
    
    [Tooltip("Минимальная прозрачность (самая тусклая точка)")]
    [Range(0f, 1f)]
    public float minAlpha = 0.2f;

    [Tooltip("Максимальная прозрачность (самая яркая точка)")]
    [Range(0f, 1f)]
    public float maxAlpha = 0.7f;

    [Header("Настройки Появления (Fade In)")]
    [Tooltip("За сколько секунд текст появится после открытия диалога")]
    public float fadeInDuration = 1.5f; 
    
    private float currentFadeTime;

    void Start()
    {
        
        if (textComponent == null)
            textComponent = GetComponent<TMP_Text>();
    }

    void OnEnable()
    {
        
        currentFadeTime = 0f;
        
        
        SetAlpha(0f);
    }

    void Update()
    {
        if (textComponent == null) return;

        
        float introMultiplier = 1f;
        if (currentFadeTime < fadeInDuration)
        {
            currentFadeTime += Time.deltaTime;
            introMultiplier = currentFadeTime / fadeInDuration;
            
            introMultiplier = Mathf.SmoothStep(0f, 1f, introMultiplier);
        }

        
        float wave = (Mathf.Sin(Time.time * speed) + 1f) / 2f;
        
        
        float pulseAlpha = Mathf.Lerp(minAlpha, maxAlpha, wave);

        
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
