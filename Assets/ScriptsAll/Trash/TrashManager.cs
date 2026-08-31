using System.Collections.Generic;
using UnityEngine;

public class TrashManager : MonoBehaviour
{
    public static TrashManager Instance;

    [SerializeField] private List<BoxCollider> trash;//zamenit na shader

    private void Awake()
    {
        Instance = this;
    }

    public void SetTrashCollidersEnabled()
    {
        foreach (var collider in trash)
        {
            if (collider != null)
            {
                collider.enabled = enabled;
                collider.gameObject.tag = "Trash";
            }
        }
    }
}
