// Файл: FollowLightController.cs (ПОЛНАЯ ВЕРСИЯ)
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class FollowLightController : MonoBehaviour
{
    // Перетащи сюда лампы В ПРАВИЛЬНОМ ПОРЯДКЕ
    public Light[] lightsSequence; 
    
    // Настройки "ленивого" мигания
    public float flickerMinDelay = 0.2f;
    public float flickerMaxDelay = 0.8f;

    private int currentLightIndex = 0;
    private Coroutine currentFlickerCoroutine;
    
    // Ссылка на QuestManager (теперь она глобальная)
    private QuestManager questManager; 

    // 1. Вызывается из QuestManager, чтобы НАЧАТЬ всю сцену
    public void StartSequence(QuestManager qm)
    {
        questManager = qm; // Запоминаем ссылку на "мозг"
        currentLightIndex = 0;
        
        // Выключаем все лампы (на всякий случай)
        foreach (Light light in lightsSequence)
        {
            light.enabled = false;
        }

        ActivateLight(currentLightIndex); // Активируем первую лампу
    }

    // 2. Активирует нужную лампу
    void ActivateLight(int index)
    {
        if (index >= lightsSequence.Length)
        {
            // Цепочка кончилась (этого не должно случиться здесь)
            return; 
        }

        Light light = lightsSequence[index];
        
        // Включаем ее триггер
        light.GetComponent<FollowLightTrigger>().ActivateTrigger();
        
        // Запускаем "ленивое" мигание
        currentFlickerCoroutine = StartCoroutine(FlickerLight(light, flickerMinDelay, flickerMaxDelay));
    }

    // 3. Вызывается из FollowLightTrigger, когда игрок подошел
    public void LightTriggered(int index)
    {
        // Убеждаемся, что игрок подошел к ПРАВИЛЬНОЙ лампе
        if (index != currentLightIndex) return; 

        Light light = lightsSequence[index];

        // 1. Останавливаем мигание
        if (currentFlickerCoroutine != null)
            StopCoroutine(currentFlickerCoroutine);
        
        // 2. Лампа горит ровно (как в PDF)
        light.enabled = true; 
        
        // 3. Выключаем ее триггер, чтобы не сработал 2-й раз
        light.GetComponent<FollowLightTrigger>().DeactivateTrigger();

        // 4. Проверяем, была ли это ПОСЛЕДНЯЯ лампа?
        if (index == lightsSequence.Length - 1)
        {
            // --- ЭТО ПОСЛЕДНЯЯ ЛАМПА (Кульминация) ---
            StartCoroutine(FinalLightSequence(light));
        }
        else
        {
            // --- Это НЕ последняя лампа ---
            // Увеличиваем индекс
            currentLightIndex++;
            
            // Делаем следующее мигание чуть быстрее (как в PDF)
            flickerMinDelay *= 0.9f;
            flickerMaxDelay *= 0.9f;
            
            // Активируем следующую лампу
            ActivateLight(currentLightIndex);
        }
    }

    // Корутина "Ленивого" мигания
    IEnumerator FlickerLight(Light light, float min, float max)
    {
        while (true)
        {
            light.enabled = !light.enabled;
            yield return new WaitForSeconds(Random.Range(min, max));
        }
    }

    // Корутина для ПОСЛЕДНЕЙ лампы (мигает 3 раза, гаснет на 4-й)
    IEnumerator FinalLightSequence(Light light)
    {
        float strobeDelay = 0.1f;
        
        // Быстрое мигание (стробоскоп)
        for (int i = 0; i < 10; i++) 
        {
            light.enabled = !light.enabled;
            yield return new WaitForSeconds(strobeDelay);
        }
        
        // Мигание 1-3 (вкл-выкл)
        light.enabled = true; yield return new WaitForSeconds(0.5f);
        light.enabled = false; yield return new WaitForSeconds(0.2f);
        light.enabled = true; yield return new WaitForSeconds(0.3f);
        light.enabled = false; yield return new WaitForSeconds(0.1f);
        light.enabled = true; yield return new WaitForSeconds(0.2f);
        
        // Гаснет на 4-й раз (как в PDF)
        light.enabled = false; 
        
        // Сообщаем QuestManager'у, что пора запускать Погоню
        if(questManager != null)
            questManager.TriggerChaseScene(); // <-- Теперь эта ссылка ИСПРАВЛЕНА
    }
}
