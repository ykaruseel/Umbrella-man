using System.Collections;
using UnityEngine;

public class ObjectInteraction : MonoBehaviour
{
    public Camera playerCamera;
    public Transform holdPoint;
    public float interactionDistance = 3f;
    public LayerMask interactionLayerMask; // Наш "пропуск" для луча

    private GameObject heldObject;
    private Rigidbody heldObjectRb;
    private Vector3 originalScale;
    private CharacterController playerController;
    private bool isInteracting = false;
    private string heldItemID = null;
    private int originalLayer;

    void Start()
    {
        playerController = GetComponent<CharacterController>();
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.E))
        {
            if (heldObject == null && !isInteracting)
            {
                Ray ray = playerCamera.ScreenPointToRay(new Vector3(Screen.width / 2, Screen.height / 2));
                if (Physics.Raycast(ray, out RaycastHit hit, interactionDistance, interactionLayerMask))
                {
                    if (hit.collider.CompareTag("Pickable"))
                    {
                        PickupObject(hit.collider.gameObject);
                    }
                }
            }
            else if (heldObject != null)
            {
                Ray ray = playerCamera.ScreenPointToRay(new Vector3(Screen.width / 2, Screen.height / 2));
                
                // --- ВОТ ИСПРАВЛЕНИЕ: Добавили наш "пропуск" (LayerMask) в этот луч ---
                if (Physics.Raycast(ray, out RaycastHit placeHit, interactionDistance, interactionLayerMask)) 
                {
                    PlacementSpot spot = placeHit.collider.GetComponent<PlacementSpot>();
                    if (spot != null && spot.requiredItemID == heldItemID)
                    {
                        PlaceObject(spot);
                        return;
                    }
                }
                
                DropObject();
            }
        }
    }

    void PlaceObject(PlacementSpot spot)
    {
        UpdateHighlights(false);
        heldItemID = null;
        heldObject.layer = originalLayer;
        heldObject.transform.SetParent(null);
        heldObject.transform.position = spot.placementTransform.position;
        heldObject.transform.rotation = spot.placementTransform.rotation;
        heldObjectRb.isKinematic = true;
        heldObject.tag = "Untagged";
        spot.enabled = false;
        spot.GetComponent<Collider>().enabled = false;
        heldObject = null;
        heldObjectRb = null;
    }

    void PickupObject(GameObject obj)
    {
        heldObject = obj;
        heldObjectRb = heldObject.GetComponent<Rigidbody>();
        originalScale = heldObject.transform.localScale;
        originalLayer = heldObject.layer;
        heldObject.layer = 2; // Слой "Ignore Raycast"

        PlaceableItem placeable = heldObject.GetComponent<PlaceableItem>();
        if (placeable != null)
        {
            heldItemID = placeable.itemID;
            UpdateHighlights(true);
        }

        heldObjectRb.useGravity = false;
        heldObjectRb.isKinematic = true;
        heldObject.transform.SetParent(holdPoint);
        heldObject.transform.localPosition = Vector3.zero;
        heldObject.transform.localRotation = Quaternion.identity;
    }

    void DropObject()
    {
        if (heldItemID != null)
        {
            UpdateHighlights(false);
            heldItemID = null;
        }
        heldObject.layer = originalLayer;
        isInteracting = true;
        Physics.IgnoreCollision(heldObject.GetComponent<Collider>(), playerController, true);
        heldObject.transform.SetParent(null);
        heldObject.transform.localScale = originalScale;
        heldObjectRb.useGravity = true;
        heldObjectRb.isKinematic = false;
        StartCoroutine(ReEnableCollisionAfterDelay(heldObject.GetComponent<Collider>(), 1f));
        heldObject = null;
        heldObjectRb = null;
    }

    void UpdateHighlights(bool show)
    {
        PlacementSpot[] allSpots = FindObjectsOfType<PlacementSpot>();
        foreach (PlacementSpot spot in allSpots)
        {
            if (show && spot.requiredItemID == heldItemID)
            {
                spot.SetHighlight(true);
            }
            else
            {
                spot.SetHighlight(false);
            }
        }
    }

    IEnumerator ReEnableCollisionAfterDelay(Collider objCollider, float delay)
    {
        yield return new WaitForSeconds(delay);
        if (objCollider != null)
        {
            Physics.IgnoreCollision(objCollider, playerController, false);
        }
        isInteracting = false;
    }
}
