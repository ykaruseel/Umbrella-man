using System.Collections;
using UnityEngine;

public class ObjectInteraction : MonoBehaviour
{
    public Camera playerCamera;
    public Transform holdPoint;
    public float pickupDistance = 3f;
    public LayerMask pickupLayerMask;

    private GameObject heldObject;
    private Rigidbody heldObjectRb;
    private Vector3 originalScale;
    private CharacterController playerController;

    // --- ИСПРАВЛЕНИЕ БАГА: Наш "светофор", чтобы избежать состояния гонки ---
    private bool isInteracting = false;

    void Start()
    {
        playerController = GetComponent<CharacterController>();
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.E))
        {
            // --- ИСПРАВЛЕНИЕ БАГА: Проверяем "светофор". Поднимать можно только если горит "зеленый". ---
            if (heldObject == null && !isInteracting)
            {
                Ray ray = playerCamera.ScreenPointToRay(new Vector3(Screen.width / 2, Screen.height / 2));
                
                if (Physics.Raycast(ray, out RaycastHit hit, pickupDistance, pickupLayerMask))
                {
                    if (hit.collider.CompareTag("Pickable"))
                    {
                        PickupObject(hit.collider.gameObject);
                    }
                }
            }
            else if (heldObject != null) // Убедимся, что объект есть, прежде чем бросить
            {
                DropObject();
            }
        }
    }

    void PickupObject(GameObject obj)
    {
        heldObject = obj;
        heldObjectRb = heldObject.GetComponent<Rigidbody>();
        
        originalScale = heldObject.transform.localScale;

        heldObjectRb.useGravity = false;
        heldObjectRb.isKinematic = true;

        heldObject.transform.SetParent(holdPoint);
        heldObject.transform.localPosition = Vector3.zero;
        heldObject.transform.localRotation = Quaternion.identity;
    }

    void DropObject()
    {
        // --- ИСПРАВЛЕНИЕ БАГА: Включаем "красный свет", пока идет процесс броска. ---
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

    IEnumerator ReEnableCollisionAfterDelay(Collider objCollider, float delay)
    {
        yield return new WaitForSeconds(delay);
        if (objCollider != null)
        {
            Physics.IgnoreCollision(objCollider, playerController, false);
        }
        
        // --- ИСПРАВЛЕНИЕ БАГА: Процесс броска завершен, включаем "зеленый свет". ---
        isInteracting = false;
    }
}
