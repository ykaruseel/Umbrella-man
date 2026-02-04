using UnityEngine;
using UnityEngine.UI;

public class CrosshairController : MonoBehaviour
{
    [SerializeField] private Camera cam;
    [SerializeField] private Image crosshairImage;
    [SerializeField] private float rayDistance = 3f;
    [SerializeField] private LayerMask interactMask;
    [SerializeField] private float animSpeed = 6f;

    private Material mat;
    private float progress;
    private float target;

    void Start()
    {
        mat = crosshairImage.material;
    }

    void Update()
    {
        CheckInteractable();
        Animate();
    }

    void CheckInteractable()
    {
        Ray ray = new Ray(cam.transform.position, cam.transform.forward);

        if (Physics.Raycast(ray, out RaycastHit hit, rayDistance, interactMask))
        {
            switch (hit.collider.tag)
            {
                case "Door":
                    target = 1f;
                    break;

                case "Pickable":
                    target = 1f;
                    break;

                case "Spot":
                    if (hit.collider.GetComponent<PlacementSpot>().highlightEffect.activeSelf)
                        target = 1f;
                    break;

                default:
                    target = 0f;
                    break;
            }
        }
        else
        {
            target = 0f;
        }
    }

    void Animate()
    {
        progress = Mathf.Lerp(progress, target, Time.deltaTime * animSpeed);
        mat.SetFloat("_Progress", progress);
    }
}
