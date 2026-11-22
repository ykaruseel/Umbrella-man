
using UnityEngine;

public class PlacementGhostQuestController : MonoBehaviour
{
    [Header("Quest")]
    [SerializeField]
    private string requiredQuestID = "Q5";

    [Header("Placement Spots")]
    [SerializeField]
    private PlacementSpot[] placementSpots;

    private void LateUpdate()
    {
        bool questActive =
            QuestManagerV2.Instance != null &&
            QuestManagerV2.Instance.IsQuestActive(
                requiredQuestID
            );

        if (placementSpots == null)
            return;

        foreach (PlacementSpot spot in placementSpots)
        {
            if (spot == null ||
                spot.highlightEffect == null)
            {
                continue;
            }

            // Show ghosts for unfilled spots
            // only while Q5 is active.
            bool shouldShow =
                questActive &&
                spot.enabled &&
                spot.gameObject.activeInHierarchy;

            if (spot.highlightEffect.activeSelf != shouldShow)
            {
                spot.SetHighlight(shouldShow);
            }
        }
    }

    private void OnDisable()
    {
        if (placementSpots == null)
            return;

        foreach (PlacementSpot spot in placementSpots)
        {
            if (spot != null)
                spot.SetHighlight(false);
        }
    }
}
