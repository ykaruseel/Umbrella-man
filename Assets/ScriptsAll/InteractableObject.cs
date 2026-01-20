using UnityEngine;

public class InteractableObject : MonoBehaviour
{
    public string objectID;
    public ObjectiveType interactionType = ObjectiveType.Interact;

    [Header("Gating")]
    public bool isShieldReady = false; 

    public void EnableShieldInteraction()
    {
        isShieldReady = true;
        Debug.Log("Щиток разблокирован человеком с зонтом.");
    }

    public void DisableShieldInteraction()
    {
        isShieldReady = false;
    }

    public void Interact()
    {
        Debug.Log("Взаимодействие с: " + objectID);

        // Чтобы обучение засчиталось, даже если щиток пока закрыт.
        if (TutorialManager.instance != null)
        {
            TutorialManager.instance.CompleteInteractionStep();
        }

        // 🔥 [ИСПРАВЛЕНИЕ] 🔥
        // Я закомментировал этот блок. Теперь щиток НЕ проверяет условия
        // и открывается всегда, когда ты нажмешь E.
        /* if (!isShieldReady)
        {
            Debug.Log("Щиток пока заблокирован. Нужно дождаться ключевого события.");
            return;
        }
        */

        // --- ЛОГИКА QTE ---
        // Пытаемся найти скрипт QTE на этом же объекте
        RepairQTE qteScript = GetComponent<RepairQTE>();
    
        if (qteScript != null)
        {
            Debug.Log("Найден скрипт RepairQTE! Запускаем мини-игру.");
            qteScript.StartRepairQTE();
            return; 
        }

        // --- ЛОГИКА КВЕСТОВ (Обычные предметы) ---
        QuestManager qm = QuestManager.instance;
        if (qm != null && qm.currentQuest != null)
        {
            // Здесь старая логика, если нужна
            // qm.UpdateQuestProgress(objectID, interactionType);
        }
    }
}
