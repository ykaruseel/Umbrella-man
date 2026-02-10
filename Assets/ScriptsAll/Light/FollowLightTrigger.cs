// Файл: FollowLightTrigger.cs
using UnityEngine;

[RequireComponent(typeof(SphereCollider))]
public class FollowLightTrigger : MonoBehaviour
{
    // Ссылка на главный контроллер (перетащим в инспекторе)
    public FollowLightController controller; 
    
    // Номер этой лампы в цепочке (0, 1, 2, 3...)
    public int lightIndex; 
    
    private SphereCollider trigger;

    void Awake()
    {
        trigger = GetComponent<SphereCollider>();
        trigger.isTrigger = true;
        trigger.radius = 1.5f;
        trigger.enabled = false;
    }

    // Включает триггер
    public void ActivateTrigger()
    {
        trigger.enabled = true;
    }

    // Выключает триггер
    public void DeactivateTrigger()
    {
        trigger.enabled = false;
    }

    // Когда игрок входит в зону
    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            if (controller != null)
            {
                controller.LightTriggered(lightIndex);
            }
        }
    }
}
