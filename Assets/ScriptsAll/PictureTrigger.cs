using UnityEngine;
using UnityEngine.Rendering;
using System.Collections;

public class PictureTrigger : MonoBehaviour
{
    [Header("What should fall?")]
    public FallingPicture picture;

    [Header("Fright Effect (PostProcess Volume)")]
    public Volume scareVolume; 

    private bool triggered = false;

    void OnTriggerEnter(Collider other)
    {
        
        if (triggered) return; 
        
        if (other.CompareTag("Player"))
        {
            
            triggered = true;
            
            
            Collider myCollider = GetComponent<Collider>();
            if (myCollider != null)
            {
                myCollider.enabled = false;
            }
            
            
            if (picture != null) 
            {
                picture.Drop();
            }
            else 
            {
                Debug.LogError("ТЫ ЗАБЫЛ НАЗНАЧИТЬ КАРТИНУ В ИНСПЕКТОРЕ ТРИГГЕРА!");
            }
            
            
            if (scareVolume != null) 
            {
                StartCoroutine(FlashScreen());
            }
        }
    }

    
    IEnumerator FlashScreen()
    {
        scareVolume.weight = 1f;
        yield return new WaitForSeconds(0.1f);
        
        
        float w = 1f;
        while (w > 0)
        {
            w -= Time.deltaTime * 3f;
            scareVolume.weight = w;
            yield return null;
        }
        scareVolume.weight = 0f;
    }
}
