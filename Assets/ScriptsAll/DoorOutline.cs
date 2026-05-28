using UnityEngine;

public class DoorOutline : MonoBehaviour
{
    public OutlineInteractable outlineInteractable;
    public void Show()
    {
        if (outlineInteractable != null)
            outlineInteractable.Show();
    }

    public void Hide()
    {
        if (outlineInteractable != null)
            outlineInteractable.Hide();
    }
}
