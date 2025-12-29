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
        playerController = GetComponent<CharacterController>();
        if (playerCamera == null) playerCamera = Camera.main;
    }

    // Методы публичные для PlayerController
    
    public void PlaceObject(PlacementSpot spot)
    {
        UpdateHighlights(false);
        
        // Квест засчитывается
        if (QuestManager.instance != null)
        {
            QuestManager.instance.UpdateQuestProgress(spot.requiredItemID, ObjectiveType.Place);
        }

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
        
        // Меняем слой на IgnoreRaycast (обычно 2), чтобы сам предмет не мешал лучам
        heldObject.layer = 2; 

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
        
        // Отцепляем от игрока
        heldObject.transform.SetParent(null);

        // --- ИСПРАВЛЕНИЕ: ПРОВЕРКА СТЕН (ANTI-CLIP) ---
        // Пускаем луч от камеры до точки, где должен быть предмет
        Ray ray = new Ray(playerCamera.transform.position, playerCamera.transform.forward);
        float distanceToHoldPoint = Vector3.Distance(playerCamera.transform.position, holdPoint.position);

        // Маска для стен (обычно Default). Используем ~0 (Все слои), но исключаем слой игрока если нужно.
        // Здесь мы просто проверяем, ударились ли мы обо что-то твердое (не триггер)
        if (Physics.Raycast(ray, out RaycastHit hit, distanceToHoldPoint))
        {
            // Проверяем, что это не сам игрок и не триггер
            if (!hit.collider.isTrigger && hit.transform != transform)
            {
                // Если луч попал в стену — ставим предмет ПЕРЕД стеной (с отступом 20 см)
                heldObject.transform.position = hit.point - (playerCamera.transform.forward * 0.2f);
            }
        }
        // Если препятствий нет — предмет остается там, где и был (на holdPoint), ничего менять не надо
        // ------------------------------------------------

        heldObject.transform.localScale = originalScale;
        heldObjectRb.useGravity = true;
        heldObjectRb.isKinematic = false;

        RuntimeManager.PlayOneShot(dropEvent, transform.position);

        StartCoroutine(ReEnableCollisionAfterDelay(heldObject.GetComponent<Collider>(), 1f));
        heldObject = null;
        heldObjectRb = null;
    }

    // Вспомогательные методы
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

