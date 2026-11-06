
using UnityEngine;

public class QuestTrigger : MonoBehaviour
{
    public string targetID;
    public GoalType type;

    private bool triggered = false;

    private void OnTriggerEnter(Collider other)
    {
        TryProcessTrigger(other);
    }

    private void OnTriggerStay(Collider other)
    {
        TryProcessTrigger(other);
    }

    private void TryProcessTrigger(Collider other)
    {
        if (triggered)
            return;

        PlayerController player =
            other.GetComponentInParent<PlayerController>();

        if (player == null)
            return;

        QuestManagerV2 manager =
            QuestManagerV2.Instance;

        if (manager == null)
            return;

        if (!manager.IsGoalRequired(
            targetID,
            type))
        {
            return;
        }

        triggered = true;

        Debug.Log(
            $"[QuestTrigger] Activated: " +
            $"{gameObject.name}, " +
            $"Target: {targetID}, Type: {type}"
        );

        manager.ProcessAction(
            targetID,
            type
        );

        Collider col = GetComponent<Collider>();

        if (col != null)
        {
            col.enabled = false;
        }
    }
}
