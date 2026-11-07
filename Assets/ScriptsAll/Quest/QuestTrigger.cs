
using System.Collections;
using UnityEngine;

public class QuestTrigger : MonoBehaviour
{
    public string targetID;
    public GoalType type;

    private bool triggered;
    private bool q8Logged;

    private void OnTriggerEnter(Collider other)
    {
        TryProcess(other);
    }

    private void OnTriggerStay(Collider other)
    {
        TryProcess(other);
    }

    private void TryProcess(Collider other)
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

        bool required =
            manager.IsGoalRequired(targetID, type);

        if (gameObject.name == "Trigger Q8" &&
            !q8Logged)
        {
            q8Logged = true;

            Debug.Log(
                "[QuestTrigger Q8] " +
                $"Q8 active: {manager.IsQuestActive("Q8")}, " +
                $"Goal required: {required}"
            );
        }

        if (!required)
            return;

        triggered = true;

        StartCoroutine(
            CompleteQuestRoutine(player)
        );
    }

    private IEnumerator CompleteQuestRoutine(
        PlayerController player)
    {
        QuestManagerV2 manager =
            QuestManagerV2.Instance;

        FocusTrigger focus =
            GetComponent<FocusTrigger>();

        if (focus != null &&
            focus.enabled &&
            manager != null &&
            manager.IsQuestActive(
                focus.requiredQuestID))
        {
            // First play the comment and camera focus.
            focus.TriggerFocus(player);

            // FocusTrigger disables itself
            // after its cinematic has finished.
            while (focus != null &&
                   focus.enabled)
            {
                yield return null;
            }
        }

        // Only then complete the quest.
        if (manager != null &&
            manager.IsGoalRequired(
                targetID,
                type))
        {
            Debug.Log(
                $"[QuestTrigger] Completed: {gameObject.name}"
            );

            manager.ProcessAction(
                targetID,
                type
            );
        }

        Collider col = GetComponent<Collider>();

        if (col != null)
            col.enabled = false;
    }
}
