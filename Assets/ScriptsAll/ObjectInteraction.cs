// Assets/Scripts/ObjectInteraction.cs
using System.Collections;
using UnityEngine;
using FMODUnity;

public class ObjectInteraction : MonoBehaviour
{
    public Camera playerCamera;
    public Transform holdPoint;
    public float interactionDistance = 3f;
    public LayerMask interactionLayerMask; // <- Это поле больше не используется PlayerController'ом, но может быть нужно для чего-то еще

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
        playerController = GetComponent<CharacterController>();
    }

    // <<< МЕТОД Update() ОТСЮДА УДАЛЕН. Вся логика в PlayerController.cs >>>

    // Методы сделаны публичными (public), чтобы PlayerController мог их вызывать
    
    public void PlaceObject(PlacementSpot spot)
    {
        UpdateHighlights(false);
        
        // --- ДОБАВЛЕНО (Шаг 7) ---
        // Сообщаем QuestManager ДО того, как очистим heldItemID
        QuestManager.instance.UpdateQuestProgress(spot.requiredItemID, ObjectiveType.Place);
        // -------------------------

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

        RuntimeManager.PlayOneShot(dropEvent, transform.position);
    }

    public void PickupObject(GameObject obj)
    {
        heldObject = obj;
        heldObjectRb = heldObject.GetComponent<Rigidbody>();
        originalScale = heldObject.transform.localScale;
        originalLayer = heldObject.layer;
        heldObject.layer = 2; // Ignore Raycast

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

        RuntimeManager.PlayOneShot(pickupEvent, transform.position);
    }

    public void DropObject()
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

        RuntimeManager.PlayOneShot(dropEvent, transform.position);

        StartCoroutine(ReEnableCollisionAfterDelay(heldObject.GetComponent<Collider>(), 1f));
        heldObject = null;
        heldObjectRb = null;
    }

    // --- ДОБАВЛЕНЫ НОВЫЕ МЕТОДЫ (Шаг 7) ---
    // Проверяет, держим ли мы что-то в руках
    public bool IsHoldingObject()
    {
        return heldObject != null;
    }

    // Позволяет PlayerController узнать ID предмета в руках
    public string GetHeldItemID()
    {
        return heldItemID;
    }
    // -------------------------------------

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

