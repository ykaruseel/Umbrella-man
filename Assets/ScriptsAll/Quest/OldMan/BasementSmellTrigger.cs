using UnityEngine;

[RequireComponent(typeof(Collider))]
public sealed class BasementSmellTrigger : MonoBehaviour
{
    [SerializeField] private OldManQuestController oldManQuest;

    private void Reset()
    {
        Collider col = GetComponent<Collider>();
        if (col != null) col.isTrigger = true;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (oldManQuest == null) return;
        if (other.GetComponentInParent<PlayerController>() == null) return;
        oldManQuest.NotifyEnteredBasement();
    }
}
