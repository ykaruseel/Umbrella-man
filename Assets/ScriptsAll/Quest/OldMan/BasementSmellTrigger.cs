
using UnityEngine;

[RequireComponent(typeof(Collider))]
public sealed class BasementSmellTrigger : MonoBehaviour
{
    [Header("Quest")]
    [SerializeField]
    private OldManQuestController oldManQuest;

    private bool wasTriggered = false;

    private void Reset()
    {
        Collider col = GetComponent<Collider>();

        if (col != null)
            col.isTrigger = true;
    }

    private void OnTriggerEnter(Collider other)
    {
        TryActivate(other);
    }

    private void OnTriggerStay(Collider other)
    {
        TryActivate(other);
    }

    private void TryActivate(Collider other)
    {
        // Only once for this trigger.
        if (wasTriggered)
            return;

        // Check that the correct quest is active.
        if (QuestManagerV2.Instance == null ||
            !QuestManagerV2.Instance.IsQuestActive("Q_OLD_MAN"))
        {
            return;
        }

        // Only the player can activate this trigger.
        PlayerController player =
            other.GetComponentInParent<PlayerController>();

        if (player == null)
            return;

        if (oldManQuest == null)
        {
            Debug.LogWarning(
                "[BasementSmell] Old Man Quest is missing!",
                this
            );

            return;
        }

        wasTriggered = true;

        Debug.Log(
            "[BasementSmell] Player detected! " +
            "Triggering kerosene dialogue.",
            this
        );

        oldManQuest.NotifyEnteredBasement();
    }
}
