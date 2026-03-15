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

        
        if (TutorialManager.instance != null)
        {
            TutorialManager.instance.CompleteInteractionStep();
        }
        

        
        if (!isShieldReady)
        {
            Debug.Log("Щиток пока заблокирован. Нужно дождаться ключевого события.");
            return;
        }

        
        RepairQTE qteScript = GetComponent<RepairQTE>();
    
        if (qteScript != null)
        {
            Debug.Log("Найден скрипт RepairQTE! Запускаем мини-игру.");
            qteScript.StartRepairQTE();
            return; 
        }

        
        QuestManager qm = QuestManager.instance;
        if (qm != null && qm.currentQuest != null)
        {
            
        }
    }
}
