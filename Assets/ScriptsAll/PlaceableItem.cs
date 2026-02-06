using UnityEngine;


public class PlaceableItem : MonoBehaviour
{
    // ”никальный ID дл€ этого предмета, например "VaseKey" или "BookOfRituals"
    public string itemID;
    public bool isPlaced = false;
    public enum ItemState
    {
        OnGround,
        Held,
        Placed
    }

    public ItemState CurrentState = ItemState.OnGround;
    public void SetState(ItemState newState)
    {
        CurrentState = newState;

        PulseHighlight pulse = GetComponent<PulseHighlight>();
        OutlineInteractable outline = GetComponent<OutlineInteractable>();

        if (pulse != null)
        {
            if (CurrentState == ItemState.OnGround)
                pulse.Show();
            else
                pulse.Hide();
        }

        if (outline != null)
        {
            outline.Hide();
        }
    }

}
