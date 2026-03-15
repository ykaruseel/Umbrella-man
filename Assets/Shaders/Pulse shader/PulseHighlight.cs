using UnityEngine;

public class PulseHighlight : MonoBehaviour
{
    [SerializeField] private Material pulseMaterial;

    Renderer rend;
    bool active;

    void Awake()
    {
        rend = GetComponentInChildren<Renderer>();
    }

    void Start()
    {
        Show();
    }


    public void Show()
    {
        if (rend == null || pulseMaterial == null || active) return;

        var mats = rend.materials;
        var newMats = new Material[mats.Length + 1];

        for (int i = 0; i < mats.Length; i++)
            newMats[i] = mats[i];

        newMats[newMats.Length - 1] = pulseMaterial;
        rend.materials = newMats;
        active = true;
    }

    public void Hide()
    {
        if (rend == null || !active) return;

        var mats = rend.materials;
        int count = 0;

        for (int i = 0; i < mats.Length; i++)
            if (mats[i].shader != pulseMaterial.shader)
                count++;

        var newMats = new Material[count];
        int idx = 0;

        for (int i = 0; i < mats.Length; i++)
        {
            if (mats[i].shader == pulseMaterial.shader) continue;
            newMats[idx++] = mats[i];
        }

        rend.materials = newMats;
        active = false;
    }
}
