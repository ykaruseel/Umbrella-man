using UnityEngine;

public class InteractableObject : MonoBehaviour
{
    public string objectID;
    public ObjectiveType interactionType = ObjectiveType.Interact;

    // 👇 НОВОЕ: Переменная для блокировки щитка
    [Header("Gating")]
    public bool isShieldReady = false; 

    // 👇 НОВОЕ: Публичный метод для разблокировки
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

        // 👇 [СЮДА] Вставляем это в самое начало!
        // Чтобы обучение засчиталось, даже если щиток пока закрыт.
        if (TutorialManager.instance != null)
        {
            TutorialManager.instance.CompleteInteractionStep();
        }
        // ----------------------------------------------------

        // 👇 А уже ПОТОМ проверяем условия
        if (!isShieldReady)
        {
            Debug.Log("Щиток пока заблокирован. Нужно дождаться ключевого события.");
            return;
        }

        // --- ЛОГИКА QTE ---
        RepairQTE qteScript = GetComponent<RepairQTE>();
    
        if (qteScript != null)
        {
            Debug.Log("Найден скрипт RepairQTE! Запускаем мини-игру.");
            qteScript.StartRepairQTE();
            return; 
        }

        // --- ЛОГИКА КВЕСТОВ ---
        QuestManager qm = QuestManager.instance;
        if (qm != null && qm.currentQuest != null)
        {
            // ... (Старая логика квестов) ...
        }
    }
}
