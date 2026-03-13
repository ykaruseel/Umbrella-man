using UnityEngine;
using System.Collections;
using FMODUnity;
using UnityEngine.Rendering;

public class FallingPictureEvent : MonoBehaviour
{
    [Header("Настройки Картины")]
    [Tooltip("Картина, которая должна упасть (должен быть Rigidbody и Collider)")]
    public GameObject pictureToFall;

    [Header("Звук FMOD")]
    [Tooltip("Звук падения, сыграет при касании пола")]
    public EventReference impactSound;

    [Header("Визуальный Эффект (Опционально)")]
    [Tooltip("Закинь сюда Volume (например, с виньеткой), скрипт мигнет его Weight")]
    public Volume postProcessVolume;

    [Header("Система Квестов")]
    [Tooltip("Требуется ли активный квест? Выключи для тестов.")]
    public bool requiresQuest = false;
    public string requiredQuestTargetID = "collect_items"; //ID квеста

    private bool hasTriggered = false;
    private Rigidbody pictureRb;

    void Start()
    {
        if (pictureToFall != null)
        {
            pictureRb = pictureToFall.GetComponent<Rigidbody>();
            
            
            if (pictureRb != null) 
                pictureRb.isKinematic = true;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        
        if (hasTriggered || !other.CompareTag("Player")) return;

        
        if (requiresQuest && QuestManager.instance != null)
        {
            var currentObj = QuestManager.instance.currentQuest?.GetCurrentObjective();
            if (currentObj == null || currentObj.targetID != requiredQuestTargetID)
                return;
        }

        TriggerFall();
    }

    private void TriggerFall()
    {
        hasTriggered = true;

        if (pictureRb != null)
        {
            
            pictureRb.isKinematic = false;
            
            
            pictureRb.AddForce(pictureToFall.transform.forward * 1.5f, ForceMode.Impulse);
            
            pictureRb.AddTorque(new Vector3(Random.Range(-1f, 1f), 0, 0), ForceMode.Impulse);

            
            var impactDetector = pictureToFall.AddComponent<PictureImpactDetector>();
            impactDetector.impactSound = impactSound;
            impactDetector.onImpact = TriggerVisualEffect;
        }
    }

    public void TriggerVisualEffect()
    {
        if (postProcessVolume != null)
            StartCoroutine(PostProcessPulse());
    }

    
    private IEnumerator PostProcessPulse()
    {
        float duration = 0.35f;
        float time = 0;
        
        while (time < duration)
        {
            time += Time.deltaTime;
            
            float t = time / duration;
            postProcessVolume.weight = Mathf.Sin(t * Mathf.PI); 
            yield return null;
        }
        postProcessVolume.weight = 0f;
    }
}


public class PictureImpactDetector : MonoBehaviour
{
    public EventReference impactSound;
    public System.Action onImpact;
    private bool hasPlayed = false;

    private void OnCollisionEnter(Collision collision)
    {
        
        if (!hasPlayed && collision.relativeVelocity.magnitude > 0.5f) 
        {
            hasPlayed = true;
            
            if (!impactSound.IsNull)
                RuntimeManager.PlayOneShot(impactSound, transform.position);
            
            onImpact?.Invoke();
            
            Destroy(this);
        }
    }
}
