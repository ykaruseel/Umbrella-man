// Файл: FollowLightTrigger.cs
using UnityEngine;

[RequireComponent(typeof(SphereCollider))]
public class FollowLightTrigger : MonoBehaviour
{
  
    public FollowLightController controller; 
    
   
    public int lightIndex; 
    
    private SphereCollider trigger;

    void Awake()
    {
        trigger = GetComponent<SphereCollider>();
        trigger.isTrigger = true;
        trigger.radius = 1.5f;
        trigger.enabled = false;
    }

   
    public void ActivateTrigger()
    {
        trigger.enabled = true;
    }

 
    public void DeactivateTrigger()
    {
        trigger.enabled = false;
    }

 
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
