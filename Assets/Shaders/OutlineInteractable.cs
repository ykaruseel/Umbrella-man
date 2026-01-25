using UnityEngine;

public class OutlineInteractable : MonoBehaviour
{
    public Material outlineMaterial;

    private Renderer rend;
    private Material[] originalMaterials;
    private bool isOutlined;

    void Awake()
    {
        rend = GetComponentInChildren<Renderer>();
        if (rend != null)
            originalMaterials = rend.materials;
    }

    public void Show()
    {
        if (rend == null || outlineMaterial == null || isOutlined) return;

        Material[] mats = new Material[originalMaterials.Length + 1];
        for (int i = 0; i < originalMaterials.Length; i++)
            mats[i] = originalMaterials[i];

        mats[mats.Length - 1] = outlineMaterial;
        rend.materials = mats;
        isOutlined = true;
    }

    public void Hide()
    {
        if (rend == null || !isOutlined) return;

        rend.materials = originalMaterials;
        isOutlined = false;
    }
}
