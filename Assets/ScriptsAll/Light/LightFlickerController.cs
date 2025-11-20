// Файл: LightFlickerController.cs
using UnityEngine;
using System.Collections;
using System.Collections.Generic; // Для List

public class LightFlickerController : MonoBehaviour
{
    [Tooltip("Перетащи сюда все лампы, которые должны мигать")]
    public List<Light> lightsToFlicker; 
    
    [Header("Настройки мигания")]
    public float initialDelay = 1f;     // Начальная задержка между миганиями
    public float minDelay = 0.1f;       // Минимальная задержка (макс. скорость)
    public float acceleration = 0.05f;  // Насколько быстрее мигает с каждым циклом
    public int stages = 3;              // Сколько этапов (1 лампа, несколько, все)

    private List<float> initialIntensities = new List<float>(); // Запомним начальную яркость
    private Coroutine pulsingCoroutine; // Для остановки пульсации

    void Start()
    {
         // Запоминаем начальную яркость каждой лампы
        foreach(Light l in lightsToFlicker)
        {
            if(l != null)
            {
                initialIntensities.Add(l.intensity); // Запоминаем ИНТЕНСИВНОСТЬ
            }
            else
            {
                initialIntensities.Add(0); // Добавляем заглушку, если лампа null
            }
        }
    }

    // Главная корутина, управляющая всей последовательностью мигания
    // Вызывается из QuestManager
    public IEnumerator FlickerSequence(System.Action onCompleteCallback)
    {
        if (lightsToFlicker.Count == 0) 
        {
            Debug.LogWarning("Нет ламп для мигания!");
            onCompleteCallback?.Invoke(); // Все равно вызываем коллбэк
            yield break;
        }
        
        // Сначала выключаем все
        foreach (Light l in lightsToFlicker) { if(l) l.enabled = false; }
        yield return new WaitForSeconds(0.5f); // Пауза перед началом

        float currentDelay = initialDelay;
        int lightsPerStage = Mathf.CeilToInt((float)lightsToFlicker.Count / stages);

        Debug.Log("Начинается последовательность мигания...");

        // Этап 1: Мигает одна лампа (первая в списке)
        if (lightsToFlicker.Count > 0 && lightsToFlicker[0] != null)
        {
            Debug.Log("Этап 1: Мигает одна лампа");
            yield return FlickerLights(new List<Light>() { lightsToFlicker[0] }, currentDelay, 5); // Мигнет 5 раз
            currentDelay = Mathf.Max(minDelay, currentDelay - acceleration * 5);
        }

        // Этап 2: Мигает несколько ламп
        if (stages > 1 && lightsToFlicker.Count > 1)
        {
            Debug.Log("Этап 2: Мигает несколько ламп");
            int count = Mathf.Min(lightsToFlicker.Count, lightsPerStage * (stages > 2 ? 1 : 2));
            List<Light> stage2Lights = lightsToFlicker.GetRange(0, count);
            yield return FlickerLights(stage2Lights, currentDelay, 8); // Мигнет 8 раз
            currentDelay = Mathf.Max(minDelay, currentDelay - acceleration * 8);
        }

        // Этап 3: Мигают все лампы, ускоряясь
        Debug.Log("Этап 3: Мигают все лампы, ускоряясь");
        while (currentDelay > minDelay)
        {
            yield return FlickerLights(lightsToFlicker, currentDelay, 1); // Мигнет 1 раз
            currentDelay = Mathf.Max(minDelay, currentDelay - acceleration);
        }

        // Финальное быстрое мигание
        Debug.Log("Финальное мигание");
        yield return FlickerLights(lightsToFlicker, minDelay, 15); // Мигнет 15 раз очень быстро

        Debug.Log("Последовательность мигания завершена.");
        // Восстанавливаем свет
        TurnOnAllLights();

        // Вызываем коллбэк (например, запуск Квеста 3)
        onCompleteCallback?.Invoke();
    }

    // Вспомогательная корутина, которая заставляет мигать указанные лампы
    IEnumerator FlickerLights(List<Light> lights, float delay, int count)
    {
        for (int i = 0; i < count; i++)
        {
            // Включаем
            foreach (Light l in lights) { if(l) l.enabled = true; }
            yield return new WaitForSeconds(delay / 2);
            // Выключаем
            foreach (Light l in lights) { if(l) l.enabled = false; }
            yield return new WaitForSeconds(delay / 2);
        }
    }

     // --- Публичные методы для QuestManager ---

     // Включить весь свет (восстановить)
     public void TurnOnAllLights()
     {
         for (int i = 0; i < lightsToFlicker.Count; i++)
         {
             if(lightsToFlicker[i]) 
             {
                 lightsToFlicker[i].enabled = true;
                 lightsToFlicker[i].intensity = initialIntensities[i]; // Восстанавливаем яркость
             }
         }
         Debug.Log("Весь свет включен.");
     }

     // Выключить весь свет (для успеха QTE)
     public void TurnOffAllLights()
     {
         foreach (Light l in lightsToFlicker)
         {
             if(l) l.enabled = false;
         }
         Debug.Log("Весь свет выключен.");
     }

     // Включить свет на максимум (для провала QTE)
     public void MaxOutLights()
     {
          for (int i = 0; i < lightsToFlicker.Count; i++)
         {
             if(lightsToFlicker[i]) 
             {
                 lightsToFlicker[i].enabled = true;
                 lightsToFlicker[i].intensity = initialIntensities[i] * 3; // Утроить яркость
             }
         }
         Debug.Log("Свет на максимум!");
     }
     // --- НОВЫЕ МЕТОДЫ ДЛЯ ПОГОНИ ---

// 1. Вызывается QuestManager'ом, чтобы запустить пульсацию ВСЕХ ламп
     public void StartPulsingFlicker()
     {
         // Выключаем все лампы (если они горели ровно)
         TurnOffAllLights();
         // Запускаем корутину пульсации
         pulsingCoroutine = StartCoroutine(PulsingFlicker());
     }

// 2. Вызывается QuestManager'ом, чтобы остановить пульсацию
     public void StopPulsingFlicker()
     {
         if (pulsingCoroutine != null)
         {
             StopCoroutine(pulsingCoroutine);
         }
         // Восстанавливаем нормальный свет (перед QTE)
         TurnOnAllLights();
     }

// Корутина "слабой пульсации" (как в PDF)
     IEnumerator PulsingFlicker()
     {
         // Восстанавливаем яркость, но очень низкую
         for (int i = 0; i < lightsToFlicker.Count; i++)
         {
             if(lightsToFlicker[i]) 
             {
                 lightsToFlicker[i].enabled = true;
                 lightsToFlicker[i].intensity = initialIntensities[i] * 0.1f; // 10% яркости
             }
         }

         float pulseSpeed = 2f; // Скорость пульсации

         while(true)
         {
             // Плавно делаем от 10% до 30% яркости
             float pulse = 0.1f + Mathf.PingPong(Time.time * pulseSpeed, 0.2f); // 0.1 -> 0.3 -> 0.1

             for (int i = 0; i < lightsToFlicker.Count; i++)
             {
                 if(lightsToFlicker[i]) 
                     lightsToFlicker[i].intensity = initialIntensities[i] * pulse;
             }
             yield return null;
         }
     }
}
