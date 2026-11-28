using UnityEngine;

// OPTIONAL test helper: attach to an active scene GameObject, use component menu
// during Play Mode, then remove the component before committing/releasing.
public sealed class MargotDebugActivator : MonoBehaviour
{
    [ContextMenu("DEBUG: Activate Q_MARGOT (Play Mode)")]
    private void ActivateMargotQuest()
    {
        if (!Application.isPlaying)
        {
            Debug.LogWarning("[MargotDebug] Enter Play Mode first.");
            return;
        }
        if (QuestManagerV2.Instance == null)
        {
            Debug.LogError("[MargotDebug] QuestManagerV2 not found.");
            return;
        }
        QuestManagerV2.Instance.ActivateQuestByID("Q_MARGOT", true);
    }
}
