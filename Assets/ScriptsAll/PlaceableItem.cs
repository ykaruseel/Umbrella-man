using UnityEngine;


public class PlaceableItem : MonoBehaviour
{
    
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

        bool onGround = CurrentState == ItemState.OnGround;

        foreach (PulseHighlight pulse in
                GetComponentsInChildren<PulseHighlight>(true))
        {
            if (onGround)
                pulse.Show();
            else
                pulse.Hide();
        }

        foreach (OutlineInteractable outline in
                GetComponentsInChildren<OutlineInteractable>(true))
        {
            outline.Hide();
            outline.isBlocked = !onGround;
        }
    }

}
