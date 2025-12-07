// FollowLightController.cs — simplified + per-lamp flicker coroutines + per-lamp delays support
using UnityEngine;
using System.Collections;
using FMODUnity;

public class FollowLightController : MonoBehaviour
{
    // Перетащи сюда лампы В ПРАВИЛЬНОМ ПОРЯДКЕ
    public Light[] lightsSequence;

    // Глобальные дефолтные задержки между on/off (используются если per-lamp не заданы)
    public float flickerMinDelay = 0.2f;
    public float flickerMaxDelay = 0.8f;

    // Опционально: задать индивидуальные интервалы для ламп. Длины массивов должны совпадать с lightsSequence.
    // Если они пустые или длина не совпадает — используются глобальные значения.
    public float[] perLampMinDelay;
    public float[] perLampMaxDelay;

    private int currentLightIndex = 0;

    // теперь массив корутин — по одной на лампу
    private Coroutine[] flickerCoroutines;

    // Ссылка на QuestManager
    private QuestManager questManager;

    // Optional FMOD sounds (3D). Можно оставить пустыми.
    [Header("Optional FMOD one-shot sounds (3D)")]
    [SerializeField] public EventReference lightOnEvent;
    [SerializeField] public EventReference lightOffEvent;

    void Awake()
    {
        if (lightsSequence != null)
            flickerCoroutines = new Coroutine[lightsSequence.Length];
    }

    // 1. Вызывается из QuestManager, чтобы НАЧАТЬ всю сцену
    public void StartSequence(QuestManager qm)
    {
        questManager = qm; // Запоминаем ссылку на "мозг"
        currentLightIndex = 0;

        // Выключаем все лампы (как в исходнике)
        if (lightsSequence != null)
        {
            for (int i = 0; i < lightsSequence.Length; i++)
            {
                var l = lightsSequence[i];
                if (l != null)
                {
                    l.enabled = false;
                }

                // очистим возможные корутины
                if (flickerCoroutines != null && i < flickerCoroutines.Length)
                {
                    if (flickerCoroutines[i] != null)
                    {
                        StopCoroutine(flickerCoroutines[i]);
                        flickerCoroutines[i] = null;
                    }
                }
            }
        }

        ActivateLight(currentLightIndex); // Активируем первую лампу
    }

    // 2. Активирует нужную лампу — только она начнёт резко мигать
    void ActivateLight(int index)
    {
        if (lightsSequence == null) return;
        if (index < 0 || index >= lightsSequence.Length) return;

        // не трогаем другие лампы кроме чистки их корутин/звуков
        for (int i = 0; i < lightsSequence.Length; i++)
        {
            if (i == index) continue;

            // стопим корутину других ламп (если они где-то запущены)
            if (flickerCoroutines != null && i < flickerCoroutines.Length && flickerCoroutines[i] != null)
            {
                StopCoroutine(flickerCoroutines[i]);
                flickerCoroutines[i] = null;
            }
        }

        Light light = lightsSequence[index];
        if (light == null) return;

        // Включаем ее триггер (если есть компонент)
        var trig = light.GetComponent<FollowLightTrigger>();
        if (trig != null)
            trig.ActivateTrigger();

        // Чтобы первая итерация корутины гарантированно включила лампу (а не сразу выключила),
        // установим перед стартом состояние в false.
        light.enabled = false;

        // Запускаем "ленивое" мигание (резко on/off)
        float min = flickerMinDelay;
        float max = flickerMaxDelay;
        if (perLampMinDelay != null && perLampMaxDelay != null &&
            perLampMinDelay.Length == lightsSequence.Length && perLampMaxDelay.Length == lightsSequence.Length)
        {
            min = perLampMinDelay[index];
            max = perLampMaxDelay[index];
        }

        // стартуем корутину для этой конкретной лампы (и сохраняем её в массив)
        if (flickerCoroutines == null)
            flickerCoroutines = new Coroutine[lightsSequence.Length];

        // остановим предыдущее, если вдруг
        if (flickerCoroutines[index] != null)
        {
            StopCoroutine(flickerCoroutines[index]);
            flickerCoroutines[index] = null;
        }

        flickerCoroutines[index] = StartCoroutine(FlickerLightCoroutine(index, light, min, max));
    }

    // 3. Вызывается из FollowLightTrigger, когда игрок подошел
    public void LightTriggered(int index)
    {
        // Убеждаемся, что игрок подошел к ПРАВИЛЬНОЙ лампе
        if (index != currentLightIndex) return;
        if (lightsSequence == null) return;
        if (index < 0 || index >= lightsSequence.Length) return;

        Light light = lightsSequence[index];

        // 1. Останавливаем мигание этой лампы
        if (flickerCoroutines != null && index < flickerCoroutines.Length && flickerCoroutines[index] != null)
        {
            StopCoroutine(flickerCoroutines[index]);
            flickerCoroutines[index] = null;
        }

        // 2. Лампа горит ровно (как в PDF)
        if (light != null)
            light.enabled = true;

        // play on sound (optional)
        if (light != null && !lightOnEvent.IsNull)
            RuntimeManager.PlayOneShotAttached(lightOnEvent, light.gameObject);

        // 3. Выключаем ее триггер, чтобы не сработал 2-й раз
        var trig = light.GetComponent<FollowLightTrigger>();
        if (trig != null)
            trig.DeactivateTrigger();

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

            // Если используются per-lamp delays, применить те же коэффициенты к ним, чтобы сохранить поведение:
            if (perLampMinDelay != null && perLampMaxDelay != null &&
                perLampMinDelay.Length == lightsSequence.Length && perLampMaxDelay.Length == lightsSequence.Length)
            {
                perLampMinDelay[index] *= 0.9f;
                perLampMaxDelay[index] *= 0.9f;
            }

            // Активируем следующую лампу
            ActivateLight(currentLightIndex);
        }
    }

    // Корутина "Ленивого" мигания (резко включ/выкл) — теперь принимает index и хранится в массиве
    IEnumerator FlickerLightCoroutine(int index, Light light, float min, float max)
    {
        // safety: если light == null — сразу выход
        if (light == null) yield break;

        while (true)
        {
            // переключаем состояние
            light.enabled = !light.enabled;

            // play corresponding on/off sound (optional)
            if (light.enabled)
            {
                if (!lightOnEvent.IsNull)
                    RuntimeManager.PlayOneShotAttached(lightOnEvent, light.gameObject);
            }
            else
            {
                if (!lightOffEvent.IsNull)
                    RuntimeManager.PlayOneShotAttached(lightOffEvent, light.gameObject);
            }

            // ждём между переключениями
            float wait = Random.Range(min, max);
            yield return new WaitForSeconds(wait);
        }
    }

    // Корутина для ПОСЛЕДНЕЙ лампы (мигает, затем гаснет)
    IEnumerator FinalLightSequence(Light light)
    {
        float strobeDelay = 0.1f;

        // Быстрое мигание (стробоскоп)
        for (int i = 0; i < 10; i++)
        {
            if (light != null)
            {
                light.enabled = !light.enabled;
                // звуки
                if (light.enabled)
                {
                    if (!lightOnEvent.IsNull)
                        RuntimeManager.PlayOneShotAttached(lightOnEvent, light.gameObject);
                }
                else
                {
                    if (!lightOffEvent.IsNull)
                        RuntimeManager.PlayOneShotAttached(lightOffEvent, light.gameObject);
                }
            }
            yield return new WaitForSeconds(strobeDelay);
        }

        // Мигание 1-3 (вкл-выкл)
        if (light != null) { light.enabled = true; if (!lightOnEvent.IsNull) RuntimeManager.PlayOneShotAttached(lightOnEvent, light.gameObject); }
        yield return new WaitForSeconds(0.5f);
        if (light != null) { light.enabled = false; if (!lightOffEvent.IsNull) RuntimeManager.PlayOneShotAttached(lightOffEvent, light.gameObject); }
        yield return new WaitForSeconds(0.2f);
        if (light != null) { light.enabled = true; if (!lightOnEvent.IsNull) RuntimeManager.PlayOneShotAttached(lightOnEvent, light.gameObject); }
        yield return new WaitForSeconds(0.3f);
        if (light != null) { light.enabled = false; if (!lightOffEvent.IsNull) RuntimeManager.PlayOneShotAttached(lightOffEvent, light.gameObject); }
        yield return new WaitForSeconds(0.1f);
        if (light != null) { light.enabled = true; if (!lightOnEvent.IsNull) RuntimeManager.PlayOneShotAttached(lightOnEvent, light.gameObject); }
        yield return new WaitForSeconds(0.2f);

        // Гаснет на 4-й раз
        if (light != null)
            light.enabled = false;

        // Сообщаем QuestManager'у, что пора запускать Погоню
        if (questManager != null)
            questManager.TriggerChaseScene();
    }

    private void OnDestroy()
    {
        // cleanup coroutines
        if (flickerCoroutines != null)
        {
            for (int i = 0; i < flickerCoroutines.Length; i++)
            {
                if (flickerCoroutines[i] != null)
                {
                    StopCoroutine(flickerCoroutines[i]);
                    flickerCoroutines[i] = null;
                }
            }
        }
    }
}

