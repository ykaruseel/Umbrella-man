using UnityEngine;

public class PulseHighlight : MonoBehaviour
{
    [SerializeField] private Material pulseMaterial;

    Renderer rend;
    public bool active;

    void Awake()
    {
        rend = GetComponentInChildren<Renderer>();
    }

    //void Start()
    //{
    //    Show();
    //}

   public void Show()
{
    if (rend == null || pulseMaterial == null || active)
        return;

    var mats = new System.Collections.Generic.List<Material>(
        rend.sharedMaterials
    );

    if (!mats.Contains(pulseMaterial))
        mats.Add(pulseMaterial);

    rend.sharedMaterials = mats.ToArray();
    active = true;
}

public void Hide()
{
    if (rend == null || pulseMaterial == null)
        return;

    var mats = new System.Collections.Generic.List<Material>(
        rend.sharedMaterials
    );

    mats.RemoveAll(mat => mat == pulseMaterial);

    rend.sharedMaterials = mats.ToArray();
    active = false;
}
}
