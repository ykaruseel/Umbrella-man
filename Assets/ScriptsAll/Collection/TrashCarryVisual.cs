using UnityEngine;

public class TrashCarryVisual : MonoBehaviour
{
    [SerializeField]
    private GameObject fullTrashBagVisual;


    public bool HasBag =>
        fullTrashBagVisual != null &&
        fullTrashBagVisual.activeSelf;


    public void ShowFullBag()
    {
        if (fullTrashBagVisual != null)
        {
            fullTrashBagVisual.SetActive(true);
        }
    }


    public void HideFullBag()
    {
        if (fullTrashBagVisual != null)
        {
            fullTrashBagVisual.SetActive(false);
        }
    }
}