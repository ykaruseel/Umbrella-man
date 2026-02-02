using UnityEngine;

public class OutlineInteractable : MonoBehaviour
{
    public Material outlineMaterial;

    Renderer rend;
    bool isOutlined;

    void Awake()
    {
        rend = GetComponentInChildren<Renderer>();
    }

    public void Show()
    {
        if (rend == null || outlineMaterial == null || isOutlined) return;

        var mats = rend.materials;
        var newMats = new Material[mats.Length + 1];

        for (int i = 0; i < mats.Length; i++)
            newMats[i] = mats[i];

        newMats[newMats.Length - 1] = outlineMaterial;
        rend.materials = newMats;
        isOutlined = true;
    }

    public void Hide()
    {
        if (rend == null || !isOutlined) return;

        var mats = rend.materials;
        int count = 0;

        for (int i = 0; i < mats.Length; i++)
            if (mats[i].shader != outlineMaterial.shader)
                count++;

        var newMats = new Material[count];
        int idx = 0;

        for (int i = 0; i < mats.Length; i++)
        {
            if (mats[i].shader == outlineMaterial.shader) continue;
            newMats[idx++] = mats[i];
        }

        rend.materials = newMats;
        isOutlined = false;
    }
}
