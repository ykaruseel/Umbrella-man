
using System.Collections.Generic;
using UnityEngine;

public class OutlineInteractable : MonoBehaviour
{
    public Material outlineMaterial;
    public bool isBlocked = false;

    [Header("Multiple Renderers")]
    [SerializeField]
    private bool outlineAllChildren = false;

    [SerializeField]
    private Transform rendererRoot;

    private readonly Dictionary<Renderer, Material[]>
        originalMaterials = new();

    private bool isOutlined;

    public void Show()
    {
        if (isOutlined || isBlocked || outlineMaterial == null)
            return;

        Transform root = rendererRoot != null
            ? rendererRoot
            : transform;

        Renderer[] renderers;

        if (outlineAllChildren)
        {
            renderers = root.GetComponentsInChildren<Renderer>();
        }
        else
        {
            Renderer first = GetComponentInChildren<Renderer>();
            renderers = first != null
                ? new Renderer[] { first }
                : new Renderer[0];
        }

        foreach (Renderer rend in renderers)
        {
            if (rend == null || !rend.enabled)
                continue;

            // Skip particle and trail renderers.
            if (!(rend is MeshRenderer) &&
                !(rend is SkinnedMeshRenderer))
                continue;

            Material[] originals = rend.sharedMaterials;

            // Avoid adding the same outline twice.
            bool alreadyPresent = false;
            foreach (Material mat in originals)
            {
                if (mat == outlineMaterial)
                {
                    alreadyPresent = true;
                    break;
                }
            }

            if (alreadyPresent)
                continue;

            originalMaterials[rend] = originals;

            Material[] expanded =
                new Material[originals.Length + 1];

            originals.CopyTo(expanded, 0);
            expanded[expanded.Length - 1] = outlineMaterial;

            rend.sharedMaterials = expanded;
        }

        isOutlined = originalMaterials.Count > 0;
    }

    public void Hide()
    {
        foreach (var entry in originalMaterials)
        {
            if (entry.Key != null)
            {
                entry.Key.sharedMaterials = entry.Value;
            }
        }

        originalMaterials.Clear();
        isOutlined = false;
    }

    private void OnDisable()
    {
        Hide();
    }
}
