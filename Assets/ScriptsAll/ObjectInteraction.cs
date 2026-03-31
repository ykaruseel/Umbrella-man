using System.Collections;
using UnityEngine;
using FMODUnity;

public class ObjectInteraction : MonoBehaviour
{
    public Camera playerCamera;
    public Transform holdPoint;
    public float interactionDistance = 3f;
    public LayerMask interactionLayerMask;

    [Header("FMOD Events")]
    [SerializeField] private EventReference pickupEvent;
    [SerializeField] private EventReference dropEvent;

    private GameObject heldObject;
    private Rigidbody heldObjectRb;
    private Vector3 originalScale;
    private CharacterController playerController;
    private bool isInteracting = false;
    private string heldItemID = null;
    private int originalLayer;

    void Start()
    {
        heldObject = null;
        playerController = GetComponent<CharacterController>();
        if (playerCamera == null) playerCamera = Camera.main;
    }


    public void PlaceObject(PlacementSpot spot)
    {
        UpdateHighlights(false);

        if (QuestManager.instance != null)
        {
            QuestManager.instance.UpdateQuestProgress(
                spot.requiredItemID,
                ObjectiveType.Place
            );
        }

        StartCoroutine(SmoothPlaceObject(spot));
    }

    private IEnumerator SmoothPlaceObject(PlacementSpot spot)
    {
        float duration = 0.35f;
        float elapsed = 0f;

        Transform objTransform = heldObject.transform;

        Vector3 startPos = objTransform.position;
        Quaternion startRot = objTransform.rotation;

        Vector3 targetPos = spot.placementTransform.position;
        Quaternion targetRot = spot.placementTransform.rotation;

        heldObjectRb.isKinematic = true;
        heldObject.layer = originalLayer;
        heldObject.tag = "Untagged";
        spot.enabled = false;
        spot.GetComponent<Collider>().enabled = false;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;

            t = Mathf.SmoothStep(0f, 1f, t);

            objTransform.position = Vector3.Lerp(startPos, targetPos, t);
            objTransform.rotation = Quaternion.Slerp(startRot, targetRot, t);

            yield return null;
        }

        objTransform.position = targetPos;
        objTransform.rotation = targetRot;
        objTransform.SetParent(null);

        heldItemID = null;
        PlaceableItem placeable = objTransform.GetComponent<PlaceableItem>();
        if (placeable != null)
            placeable.isPlaced = true;

        if (QuestManagerV2.Instance.IsGoalRequired(placeable.itemID, GoalType.ReturnItem))
        {
            QuestManagerV2.Instance.ProcessAction(placeable.itemID, GoalType.ReturnItem);
        }

        OutlineInteractable outline = objTransform.GetComponent<OutlineInteractable>();
        if (outline != null)
            outline.Hide();

        heldObject = null;
        heldObjectRb = null;

        RuntimeManager.PlayOneShot(dropEvent, transform.position);
    }

    public void PickupObject(GameObject obj)
    {
        var pulse = obj.GetComponent<PulseHighlight>();
        if (pulse != null)
            pulse.Hide();

        OutlineInteractable outline = obj.GetComponent<OutlineInteractable>();
        if (outline != null)
            outline.Hide();

        heldObject = obj;
        heldObjectRb = heldObject.GetComponent<Rigidbody>();
        originalScale = heldObject.transform.localScale;
        originalLayer = heldObject.layer;

        heldObject.layer = 2;

        PlaceableItem item = heldObject.GetComponent<PlaceableItem>();
        if (item != null)
            item.SetState(PlaceableItem.ItemState.Held);

        PlaceableItem placeable = heldObject.GetComponent<PlaceableItem>();
        if (placeable != null)
        {
            placeable.isPlaced = false;
            heldItemID = placeable.itemID;
            UpdateHighlights(true);
        }

        heldObjectRb.useGravity = false;
        heldObjectRb.isKinematic = true;
        heldObject.transform.SetParent(holdPoint);
        heldObject.transform.localPosition = Vector3.zero;
        heldObject.transform.localRotation = Quaternion.identity;

        RuntimeManager.PlayOneShot(pickupEvent, transform.position);
    }

    public void DropObject()
    {
        Debug.Log("Бросил объект: " + heldObject.name);
        if (heldItemID != null)
        {
            UpdateHighlights(false);
            heldItemID = null;
        }

        PlaceableItem item = heldObject.GetComponent<PlaceableItem>();
        if (item != null)
            item.SetState(PlaceableItem.ItemState.OnGround);

        heldObject.layer = originalLayer;
        isInteracting = true;
        Physics.IgnoreCollision(heldObject.GetComponent<Collider>(), playerController, true);

        heldObject.transform.SetParent(null);

        Ray ray = new Ray(playerCamera.transform.position, playerCamera.transform.forward);
        float distanceToHoldPoint = Vector3.Distance(playerCamera.transform.position, holdPoint.position);

        if (Physics.Raycast(ray, out RaycastHit hit, distanceToHoldPoint))
        {
            if (!hit.collider.isTrigger && hit.transform != transform)
            {
                heldObject.transform.position = hit.point - (playerCamera.transform.forward * 0.2f);
            }
        }

        heldObject.transform.localScale = originalScale;
        heldObjectRb.useGravity = true;
        heldObjectRb.isKinematic = false;

        RuntimeManager.PlayOneShot(dropEvent, transform.position);

        StartCoroutine(ReEnableCollisionAfterDelay(heldObject.GetComponent<Collider>(), 1f));

        heldObject = null;
        heldObjectRb = null;
    }


    
    public bool IsHoldingObject()
    {
        return heldObject != null;
    }

    public string GetHeldItemID()
    {
        return heldItemID;
    }

    void UpdateHighlights(bool show)
    {
        PlacementSpot[] allSpots = FindObjectsOfType<PlacementSpot>();
        foreach (PlacementSpot spot in allSpots)
        {
            if (show && spot.requiredItemID == heldItemID)
                spot.SetHighlight(true);
            else
                spot.SetHighlight(false);
        }
    }

    IEnumerator ReEnableCollisionAfterDelay(Collider objCollider, float delay)
    {
        yield return new WaitForSeconds(delay);
        if (objCollider != null)
            Physics.IgnoreCollision(objCollider, playerController, false);
        isInteracting = false;
    }
}




