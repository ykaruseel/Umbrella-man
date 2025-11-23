// Assets/ScriptsAll/InteractableObject.cs (ОБНОВЛЕННЫЙ)
using UnityEngine;

public class InteractableObject : MonoBehaviour
{
    public string objectID; // "Door", "Panel", ...
    public ObjectiveType interactionType = ObjectiveType.Interact;

    public void Interact()
    {
        Debug.Log("Взаимодействие с: " + objectID);

        // --- НОВОЕ: Проверка на QTE ---
        // Проверяем, есть ли на этом же объекте скрипт RepairQTE
        RepairQTE qteScript = GetComponent<RepairQTE>();
        
        if (qteScript != null)
        {
            Debug.Log("Найден скрипт RepairQTE! Запускаем мини-игру.");
            qteScript.StartRepairQTE();
            return; // Выходим, чтобы не мешать логике квестов (пока что)
        }
        // -----------------------------

        // --- СТАРОЕ: Логика Квестов ---
        QuestManager qm = QuestManager.instance;
        if (qm != null && qm.currentQuest != null)
        {
            QuestObjective currentObjective = qm.currentQuest.GetCurrentObjective();

            if (currentObjective != null &&
                currentObjective.targetID == objectID &&
                currentObjective.objectiveType == interactionType &&
                !currentObjective.isComplete)
            {
                qm.UpdateQuestProgress(objectID, interactionType);
            }
            else
            {
                Debug.Log("Сейчас нельзя взаимодействовать с этим объектом (нет активного квеста).");
            }
        }
    }
}
