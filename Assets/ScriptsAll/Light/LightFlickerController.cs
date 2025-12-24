using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class LightFlickerController : MonoBehaviour
{
    [Header("Лампы для мигания (Сюжет)")]
    [Tooltip("Перетащи сюда лампы, которые будут мигать во время сцены")]
    public List<Light> lightsToFlicker; 

    [Header("Остальные лампы (Финал)")]
    [Tooltip("Перетащи сюда все остальные лампы, которые просто горят, но должны погаснуть после QTE")]
    public List<Light> staticLightsToTurnOff;

    [Header("Настройки мигания")]
    public float initialDelay = 1f;     
    public float minDelay = 0.1f;       
    public float acceleration = 0.05f;  
    public int stages = 3;              

    private List<float> initialIntensities = new List<float>(); 
    private Coroutine pulsingCoroutine; 

    void Start()
    {
         // Запоминаем начальную яркость мигающих ламп
        foreach(Light l in lightsToFlicker)
        {
            if(l != null) initialIntensities.Add(l.intensity);
            else initialIntensities.Add(0);
        }
    }

    // --- ГЛАВНЫЕ МЕТОДЫ ---

    // 1. Выключить АБСОЛЮТНО ВЕСЬ свет (для успеха QTE)
    public void TurnOffAllLights()
    {
        // Выключаем мигающие лампы
        foreach (Light l in lightsToFlicker)
        {
            if(l) l.enabled = false;
        }

        // Выключаем статичные лампы (коридор и т.д.)
        if (staticLightsToTurnOff != null)
        {
            foreach (Light l in staticLightsToTurnOff)
            {
                if(l) l.enabled = false;
            }
        }

        Debug.Log("ВЕСЬ СВЕТ (мигающий и статичный) ВЫКЛЮЧЕН.");
    }

    // 2. Включить весь свет обратно (если нужно восстановить)
    public void TurnOnAllLights()
    {
        // Включаем мигающие
        for (int i = 0; i < lightsToFlicker.Count; i++)
        {
            if(lightsToFlicker[i]) 
            {
                lightsToFlicker[i].enabled = true;
                if (i < initialIntensities.Count)
                    lightsToFlicker[i].intensity = initialIntensities[i];
            }
        }
        
        // Включаем статичные
        if (staticLightsToTurnOff != null)
        {
            foreach (Light l in staticLightsToTurnOff)
            {
                if(l) l.enabled = true;
            }
        }
    }

    // 3. Последовательность мигания (Сюжет)
    public IEnumerator FlickerSequence(System.Action onCompleteCallback)
    {
        if (lightsToFlicker.Count == 0) 
        {
            onCompleteCallback?.Invoke();
            yield break;
        }
        
        // Выключаем только мигающие перед стартом шоу
        foreach (Light l in lightsToFlicker) { if(l) l.enabled = false; }
        yield return new WaitForSeconds(0.5f);

        float currentDelay = initialDelay;
        int lightsPerStage = Mathf.CeilToInt((float)lightsToFlicker.Count / stages);

        // Этап 1
        if (lightsToFlicker.Count > 0 && lightsToFlicker[0] != null)
        {
            yield return FlickerLights(new List<Light>() { lightsToFlicker[0] }, currentDelay, 5);
            currentDelay = Mathf.Max(minDelay, currentDelay - acceleration * 5);
        }

        // Этап 2
        if (stages > 1 && lightsToFlicker.Count > 1)
        {
            int count = Mathf.Min(lightsToFlicker.Count, lightsPerStage * (stages > 2 ? 1 : 2));
            List<Light> stage2Lights = lightsToFlicker.GetRange(0, count);
            yield return FlickerLights(stage2Lights, currentDelay, 8);
            currentDelay = Mathf.Max(minDelay, currentDelay - acceleration * 8);
        }

        // Этап 3
        while (currentDelay > minDelay)
        {
            yield return FlickerLights(lightsToFlicker, currentDelay, 1);
            currentDelay = Mathf.Max(minDelay, currentDelay - acceleration);
        }

        // Финал мигания
        yield return FlickerLights(lightsToFlicker, minDelay, 15);

        TurnOnAllLights(); // Включаем всё обратно
        onCompleteCallback?.Invoke();
    }

    IEnumerator FlickerLights(List<Light> lights, float delay, int count)
    {
        for (int i = 0; i < count; i++)
        {
            foreach (Light l in lights) { if(l) l.enabled = true; }
            yield return new WaitForSeconds(delay / 2);
            foreach (Light l in lights) { if(l) l.enabled = false; }
            yield return new WaitForSeconds(delay / 2);
        }
    }

     // Включить свет на максимум (для провала QTE)
     public void MaxOutLights()
     {
          for (int i = 0; i < lightsToFlicker.Count; i++)
         {
             if(lightsToFlicker[i]) 
             {
                 lightsToFlicker[i].enabled = true;
                 if (i < initialIntensities.Count)
                    lightsToFlicker[i].intensity = initialIntensities[i] * 3;
             }
         }
         // Статичные тоже можно включить, если они были выключены
         if (staticLightsToTurnOff != null)
         {
            foreach (Light l in staticLightsToTurnOff) { if(l) l.enabled = true; }
         }
     }

     // --- МЕТОДЫ ДЛЯ ПОГОНИ ---

     public void StartPulsingFlicker()
     {
         // Гасим всё перед пульсацией
         foreach (Light l in lightsToFlicker) { if(l) l.enabled = false; }
         // Статичные НЕ трогаем или гасим - по желанию. 
         // Обычно при погоне статичный свет тоже должен пугать, но пока оставим его включенным или выключим:
         // Если хотите полную темноту с пульсацией - раскомментируйте строку ниже:
         // if (staticLightsToTurnOff != null) foreach (Light l in staticLightsToTurnOff) { if(l) l.enabled = false; }

         pulsingCoroutine = StartCoroutine(PulsingFlicker());
     }

     public void StopPulsingFlicker()
     {
         if (pulsingCoroutine != null) StopCoroutine(pulsingCoroutine);
         TurnOnAllLights();
     }

     IEnumerator PulsingFlicker()
     {
         for (int i = 0; i < lightsToFlicker.Count; i++)
         {
             if(lightsToFlicker[i]) 
             {
                 lightsToFlicker[i].enabled = true;
                 if (i < initialIntensities.Count)
                    lightsToFlicker[i].intensity = initialIntensities[i] * 0.1f; 
             }
         }

         float pulseSpeed = 2f; 

         while(true)
         {
             float pulse = 0.1f + Mathf.PingPong(Time.time * pulseSpeed, 0.2f); 
             for (int i = 0; i < lightsToFlicker.Count; i++)
             {
                 if(lightsToFlicker[i] && i < initialIntensities.Count) 
                     lightsToFlicker[i].intensity = initialIntensities[i] * pulse;
             }
             yield return null;
         }
     }
}
