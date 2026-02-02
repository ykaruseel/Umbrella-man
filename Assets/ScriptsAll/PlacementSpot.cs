using UnityEngine;

public class PlacementSpot : MonoBehaviour
{
    public string requiredItemID;
    public GameObject highlightEffect;
    public Transform placementTransform;

    void Start()
    {
        if (highlightEffect != null)
            highlightEffect.SetActive(false);
    }

    public bool TryPlace(GameObject item)
    {
        PlaceableItem placeable = item.GetComponent<PlaceableItem>();
        if (placeable == null) return false;
        if (placeable.itemID != requiredItemID) return false;

        item.transform.position = placementTransform.position;
        item.transform.rotation = placementTransform.rotation;

        placeable.isPlaced = true;

        PulseHighlight pulse = item.GetComponent<PulseHighlight>();
        if (pulse != null)
            pulse.Hide();

        if (highlightEffect != null)
            highlightEffect.SetActive(false);

        return true;
    }

    public void SetHighlight(bool isActive)
    {
        if (highlightEffect != null)
            highlightEffect.SetActive(isActive);
    }
}