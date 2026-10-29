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

    [Header("Outline Distance")]
    [SerializeField]
    private bool limitOutlineDistance = false;

    [SerializeField]
    private Camera playerCamera;

    [SerializeField]
    private float outlineDistance = 2f;

    private readonly Dictionary<Renderer, Material[]>
        originalMaterials = new();

    private bool isOutlined;

    private void Update()
    {
        if (!limitOutlineDistance)
            return;

        if (playerCamera == null)
        {
            Hide();
            return;
        }

        if (isBlocked || !IsWithinOutlineDistance())
        {
            Hide();
            return;
        }

        Show();
    }

    private bool IsWithinOutlineDistance()
    {
        Ray ray = new Ray(
            playerCamera.transform.position,
            playerCamera.transform.forward
        );

        if (!Physics.Raycast(
            ray,
            out RaycastHit hit,
            outlineDistance,
            Physics.DefaultRaycastLayers,
            QueryTriggerInteraction.Ignore))
        {
            return false;
        }

        OutlineInteractable hitOutline =
            hit.transform.GetComponentInParent<OutlineInteractable>();

        return hitOutline == this;
    }

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

            if (!(rend is MeshRenderer) &&
                !(rend is SkinnedMeshRenderer))
                continue;

            Material[] originals = rend.sharedMaterials;

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