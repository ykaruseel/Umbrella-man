// Файл: InteractableObject.cs
using UnityEngine;

public class InteractableObject : MonoBehaviour
{
    // Уникальный ID объекта (должен совпадать с targetID в квесте)
    // Для Двери: "Door"
    // Для Щитка: "Panel"
    public string objectID;
    public ObjectiveType interactionType = ObjectiveType.Interact; // Тип взаимодействия

    // Этот метод будет вызывать твой PlayerController, когда игрок нажимает 'E'
    public void Interact()
    {
        Debug.Log("Взаимодействие с: " + objectID);

        // Проверяем, есть ли QuestManager и активный квест
        QuestManager qm = QuestManager.instance;
        if(qm != null && qm.currentQuest != null)
        {
            QuestObjective currentObjective = qm.currentQuest.GetCurrentObjective();
             
            // Разрешаем взаимодействие только если это ТЕКУЩАЯ цель квеста
            if (currentObjective != null && 
                currentObjective.targetID == objectID && 
                currentObjective.objectiveType == interactionType &&
                !currentObjective.isComplete)
            {
                // Сообщаем менеджеру.
                qm.UpdateQuestProgress(objectID, interactionType);
            } 
            else 
            {
                Debug.Log("Сейчас нельзя взаимодействовать с этим объектом.");
                // Сюда можно добавить звук "заблокировано" или "хм, не то"
            }
        }
    }
}
