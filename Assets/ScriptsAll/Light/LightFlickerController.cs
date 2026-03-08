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
     
        foreach(Light l in lightsToFlicker)
        {
            if(l != null) initialIntensities.Add(l.intensity);
            else initialIntensities.Add(0);
        }
    }

   
    public void TurnOffAllLights()
    {
        
        foreach (Light l in lightsToFlicker)
        {
            if(l) l.enabled = false;
        }

 
        if (staticLightsToTurnOff != null)
        {
            foreach (Light l in staticLightsToTurnOff)
            {
                if(l) l.enabled = false;
            }
        }

        Debug.Log("ВЕСЬ СВЕТ (мигающий и статичный) ВЫКЛЮЧЕН.");
    }

   
    public void TurnOnAllLights()
    {
      
        for (int i = 0; i < lightsToFlicker.Count; i++)
        {
            if(lightsToFlicker[i]) 
            {
                lightsToFlicker[i].enabled = true;
                if (i < initialIntensities.Count)
                    lightsToFlicker[i].intensity = initialIntensities[i];
            }
        }
        
   
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
        
   
        foreach (Light l in lightsToFlicker) { if(l) l.enabled = false; }
        yield return new WaitForSeconds(0.5f);

        float currentDelay = initialDelay;
        int lightsPerStage = Mathf.CeilToInt((float)lightsToFlicker.Count / stages);

        
        if (lightsToFlicker.Count > 0 && lightsToFlicker[0] != null)
        {
            yield return FlickerLights(new List<Light>() { lightsToFlicker[0] }, currentDelay, 5);
            currentDelay = Mathf.Max(minDelay, currentDelay - acceleration * 5);
        }

        
        if (stages > 1 && lightsToFlicker.Count > 1)
        {
            int count = Mathf.Min(lightsToFlicker.Count, lightsPerStage * (stages > 2 ? 1 : 2));
            List<Light> stage2Lights = lightsToFlicker.GetRange(0, count);
            yield return FlickerLights(stage2Lights, currentDelay, 8);
            currentDelay = Mathf.Max(minDelay, currentDelay - acceleration * 8);
        }

        
        while (currentDelay > minDelay)
        {
            yield return FlickerLights(lightsToFlicker, currentDelay, 1);
            currentDelay = Mathf.Max(minDelay, currentDelay - acceleration);
        }

      
        yield return FlickerLights(lightsToFlicker, minDelay, 15);

        TurnOnAllLights(); 
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
     
         if (staticLightsToTurnOff != null)
         {
            foreach (Light l in staticLightsToTurnOff) { if(l) l.enabled = true; }
         }
     }

   

     public void StartPulsingFlicker()
     {
        
         foreach (Light l in lightsToFlicker) { if(l) l.enabled = false; }
        

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
