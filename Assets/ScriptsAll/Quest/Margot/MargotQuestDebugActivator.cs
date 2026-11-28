using UnityEngine;

// Temporary helper for testing the Margot quest without replaying all prior quests.
// Remove this component from MainScene before shipping.
public sealed class MargotQuestDebugActivator : MonoBehaviour
{
    [ContextMenu("DEBUG: Activate Q_MARGOT (Play Mode)")]
    private void ActivateMargotQuest()
    {
        if (!Application.isPlaying)
        {
            Debug.LogWarning("[Margot Debug] Start Play Mode first.");
            return;
        }

        QuestManagerV2 manager = QuestManagerV2.Instance;
        if (manager == null)
        {
            Debug.LogError("[Margot Debug] QuestManagerV2.Instance is missing.");
            return;
        }

        bool activated = manager.ActivateQuestByID("Q_MARGOT", true);
        if (activated)
            Debug.Log("[Margot Debug] Q_MARGOT activated. Approach apartment 206 and press E.");
    }
}
