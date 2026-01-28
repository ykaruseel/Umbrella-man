using System.Collections;
using UnityEngine;
using FMODUnity;
using Unity.Cinemachine;

[RequireComponent(typeof(CharacterController))]
public class PlayerController : MonoBehaviour
{
    [Header("Movement Settings")]
    public float walkSpeed = 3f;
    public float runSpeed = 5f;
    public float jumpPower = 0f;
    public float gravity = 10f;

    [Header("Camera Settings")]
    public float lookSpeed = 2f;
    public float lookXLimit = 75f;
    public float cameraRotationSmooth = 5f;
    public bool isCinematic = false;

    [Header("Interaction Settings")]
    public Camera playerCam;
    public float interactionDistance = 3f;
    public LayerMask interactionLayerMask;


    [Header("FMOD Footsteps")]
    [SerializeField] private EventReference footstepEvent;
    [SerializeField] private float walkStepInterval = 0.5f;
    [SerializeField] private float runStepInterval = 0.3f;

    [Header("FMOD Ambient")]
    [SerializeField] private EventReference ambientEvent;
    private FMOD.Studio.EventInstance ambientInstance;

    private bool isWalking = false;
    private bool isFootstepCoroutineRunning = false;
    private CharacterController characterController;
    private Vector3 moveDirection = Vector3.zero;
    private float rotationX = 0f;
    private float rotationY = 180f;
    private Coroutine footstepCoroutine;
    private OutlineInteractable currentOutline;


    [Header("Dialogue Zoom Settings")]
    public CinemachineCamera virtualCam;   // виртуальная камера для зума
    [SerializeField] private float dialogueZoomFOV;
    [SerializeField] private float dialogueZoomSpeed;
    private Coroutine currentZoomCoroutine;

    private bool canMove = true;
    private bool dialogueZoom = false;
    private float initialFOV;
    
    [Header("Zoom Settings")]
    public Camera playerCamera;
    public float defaultFOV = 60f;
    public float zoomFOV = 45f;
    public float zoomSpeed = 2.0f;
    
    private float targetFOV;         // К какому значению мы сейчас стремимся
    
    public static bool isGameEnded = false;

    // --- Ссылка на ObjectInteraction ---
    private ObjectInteraction objectInteraction;

    void Start()
    {
        
        isGameEnded = false;
        
        if (playerCamera == null) playerCamera = Camera.main;
        targetFOV = defaultFOV;
        
        characterController = GetComponent<CharacterController>();
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        objectInteraction = GetComponent<ObjectInteraction>();
        if (objectInteraction == null)
            Debug.LogError("На игроке отсутствует скрипт ObjectInteraction!");

        if (ambientEvent.IsNull == false)
        {
            ambientInstance = RuntimeManager.CreateInstance(ambientEvent);
            ambientInstance.start();
        }

        if (virtualCam != null)
            initialFOV = virtualCam.Lens.FieldOfView;
    }

    void Update()
    {
        // --- если диалог активен, разрешаем только "E" для переключения ---
        DialogueManager dm = FindObjectOfType<DialogueManager>();
        if (dm != null && dm.IsDialogueActive())
        {
            // ВАЖНО: останавливаем шаги, если вдруг шли
            StopFootsteps();

            if (Input.GetKeyDown(KeyCode.E) && !Pause.isPaused)
                dm.DisplayNextSentence(); // листаем реплики

            return; // блокируем остальное управление
        }

        HandleMovement();
        HandleCameraRotation();
        HandleDialogueZoom();
        HandleFootsteps();
        CheckInteractionInput();
        UpdateOutline();

        if (playerCamera != null)
        {
            
            playerCamera.fieldOfView = Mathf.Lerp(playerCamera.fieldOfView, targetFOV, Time.deltaTime * zoomSpeed);
        }
        
    }
    private void StopFootsteps()
    {
        isWalking = false;

        if (footstepCoroutine != null)
        {
            StopCoroutine(footstepCoroutine);
            footstepCoroutine = null;
        }
    }

    private void HandleMovement()
    {
        if(!canMove) return;
        Vector3 forward = transform.TransformDirection(Vector3.forward);
        Vector3 right = transform.TransformDirection(Vector3.right);

        float curSpeedX = canMove ? (Input.GetKey(KeyCode.LeftShift) ? runSpeed : walkSpeed) * Input.GetAxis("Vertical") : 0f;
        float curSpeedY = canMove ? (Input.GetKey(KeyCode.LeftShift) ? runSpeed : walkSpeed) * Input.GetAxis("Horizontal") : 0f;
        float movementDirectionY = moveDirection.y;
        moveDirection = (forward * curSpeedX) + (right * curSpeedY);

        if (Input.GetButton("Jump") && canMove && characterController.isGrounded)
            moveDirection.y = jumpPower;
        else
            moveDirection.y = movementDirectionY;

        if (!characterController.isGrounded)
            moveDirection.y -= gravity * Time.deltaTime;

        characterController.Move(moveDirection * Time.deltaTime);
    }

    private void HandleCameraRotation()
    {
        if (!canMove || isCinematic) return;

        rotationX -= Input.GetAxis("Mouse Y") * lookSpeed;
        rotationX = Mathf.Clamp(rotationX, -lookXLimit, lookXLimit);
        rotationY += Input.GetAxis("Mouse X") * lookSpeed;

        Quaternion targetRotationX = Quaternion.Euler(rotationX, 0, 0);
        Quaternion targetRotationY = Quaternion.Euler(0, rotationY, 0);

        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotationY, Time.deltaTime * cameraRotationSmooth);

        if (virtualCam != null)
            virtualCam.transform.localRotation = Quaternion.Slerp(virtualCam.transform.localRotation, targetRotationX, Time.deltaTime * cameraRotationSmooth);
    }

    private void HandleDialogueZoom()
    {
        if (virtualCam == null) return;

        //float targetFOV = dialogueZoom ? dialogueZoomFOV : initialFOV;

        //virtualCam.Lens.FieldOfView = Mathf.Lerp(
        //    virtualCam.Lens.FieldOfView,
        //    targetFOV,
        //    Time.deltaTime * dialogueZoomSpeed
        //);
    }

    
    public void ZoomIn()
    {
        if (currentZoomCoroutine != null)
            StopCoroutine(currentZoomCoroutine);

        currentZoomCoroutine = StartCoroutine(SmoothZoom(virtualCam.Lens.FieldOfView, virtualCam.Lens.FieldOfView*0.8f));
    }

    public void ZoomOut()
    {
        if (currentZoomCoroutine != null)
            StopCoroutine(currentZoomCoroutine);

        currentZoomCoroutine = StartCoroutine(SmoothZoom(virtualCam.Lens.FieldOfView, virtualCam.Lens.FieldOfView/0.8f));
    }

    private IEnumerator SmoothZoom(float from, float to)
    {
        float elapsed = 0f;
        while (elapsed < dialogueZoomSpeed)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.SmoothStep(0f, 1f, elapsed / dialogueZoomSpeed);
            virtualCam.Lens.FieldOfView = Mathf.Lerp(from, to, t);
            yield return null;
        }
        virtualCam.Lens.FieldOfView = to;
    }

    private void HandleFootsteps()
    {
        bool isMoving = (Input.GetAxis("Vertical") != 0f || Input.GetAxis("Horizontal") != 0f) && canMove;

        if (isMoving && footstepCoroutine == null)
        {
            isWalking = true;
            footstepCoroutine = StartCoroutine(PlayFootstepSounds());
        }
        else if (!isMoving && isWalking)
        {
            // Перестали двигаться — останавливаем шаги
            StopFootsteps();
        }
    }

    private IEnumerator PlayFootstepSounds()
    {
        float previousDelay = 0f;

        while (isWalking)
        {
            bool isRunning = Input.GetKey(KeyCode.LeftShift);
            float delay = isRunning ? runStepInterval : walkStepInterval;

            if (Mathf.Abs(delay - previousDelay) > 0.001f)
                previousDelay = delay;

            RuntimeManager.PlayOneShot(footstepEvent, transform.position);
            yield return new WaitForSeconds(delay);
        }

        // корутина закончилась
        footstepCoroutine = null;
    }


    void OnDestroy()
    {
        if (ambientInstance.isValid())
        {
            ambientInstance.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
            ambientInstance.release();
        }
    }

    // --- Методы для внешних систем ---
    public void SetCanMove(bool value)
    {
        canMove = value;

        if (!canMove)
            StopFootsteps();
    }

    public void SetDialogueZoom(bool value)
    {
        dialogueZoom = value;
    }
    void UpdateOutline()
    {
        if (objectInteraction != null && objectInteraction.IsHoldingObject())
        {
            ClearOutline();
            return;
        }

        Ray ray = playerCam.ScreenPointToRay(new Vector3(Screen.width / 2, Screen.height / 2));
        if (Physics.Raycast(ray, out RaycastHit hit, interactionDistance, interactionLayerMask))
        {
            OutlineInteractable outline = hit.collider.GetComponentInParent<OutlineInteractable>();
            PlaceableItem placeable = hit.collider.GetComponentInParent<PlaceableItem>();

            if (outline != null && placeable != null && !placeable.isPlaced)
            {
                if (currentOutline != outline)
                {
                    ClearOutline();
                    currentOutline = outline;
                    currentOutline.Show();
                }
                return;
            }
        }

        ClearOutline();
    }

    void ClearOutline()
    {
        if (currentOutline != null)
        {
            currentOutline.Hide();
            currentOutline = null;
        }
    }
    void CheckInteractionInput()
    {
        // 1) Если активен диалог – игнорируем взаимодействия
        DialogueManager dm = FindFirstObjectByType<DialogueManager>();
        if (dm != null && dm.IsDialogueActive())
            return;

        // 2) Если активен QTE – тоже игнорируем E, чтобы не перезапускать щиток
        RepairQTE qte = FindFirstObjectByType<RepairQTE>();
        if (qte != null && qte.isQTEActive)
            return;

        // 3) Дальше реагируем только если реально нажали E
        if (!Input.GetKeyDown(KeyCode.E))
            return;

        if (Pause.isPaused)
            return;

        Ray ray = playerCam.ScreenPointToRay(
            new Vector3(Screen.width / 2, Screen.height / 2)
        );
        RaycastHit hit;
        bool hitSomething = Physics.Raycast(
            ray, out hit, interactionDistance, interactionLayerMask
        );

        // --- Если что-то держим в руках ---
        if (objectInteraction.IsHoldingObject())
        {
            if (hitSomething)
            {
                PlacementSpot spot = hit.collider.GetComponent<PlacementSpot>();
                if (spot != null && spot.requiredItemID == objectInteraction.GetHeldItemID())
                {
                    // КЛАДЕМ предмет в нужное место
                    objectInteraction.PlaceObject(spot);
                    return;
                }

                if (hit.collider.CompareTag("Door"))
                {
                    hit.collider.GetComponent<DoorController>().TryOpenDoor();
                    return;
                }
            }

            // Иначе просто роняем
            objectInteraction.DropObject();
            return;
        }

        // --- Если ничего не держим, пробуем взаимодействовать с тем, во что попал луч ---
        if (hitSomething)
        {
            // 1) Диалог с NPC
            NPC_Dialogue npcDialogue = hit.collider.GetComponent<NPC_Dialogue>();
            if (npcDialogue != null)
            {
                Debug.Log("[PlayerController] Trigger NPC_Dialogue");
                npcDialogue.TriggerDialogue();
                return;
            }

            // 2) Подбираемый предмет
            if (hit.collider.CompareTag("Pickable"))
            {
                Debug.Log("[PlayerController] Pickup Pickable");
                objectInteraction.PickupObject(hit.collider.gameObject);
                return;
            }

            // 3) Прочие интерактивные объекты (щиток и т.п.)
            InteractableObject interactable = hit.collider.GetComponent<InteractableObject>();
            if (interactable != null)
            {
                Debug.Log("[PlayerController] Interact with " + hit.collider.name);
                interactable.Interact();
                return;
            }

            if(hit.collider.CompareTag("Door"))
            {
                hit.collider.GetComponent<DoorController>().TryOpenDoor();
                return;
            }
        }
    }
  

    public void LockMovementButAllowLook()
    {
        canMove = false; // блокируем движение
                         // но не трогаем камеру — игрок может осматриваться
    }
    public void StartCinematicPan(Transform target, float duration)
    {
        isCinematic = true; // Блокируем мышь
        StartCoroutine(PanToTarget(target, duration));
    }

    private IEnumerator PanToTarget(Transform target, float duration)
    {
        if (virtualCam == null || target == null) yield break;

        Transform cam = virtualCam.transform;

        Vector3 e = cam.eulerAngles;
        e.z = 0f;
        cam.rotation = Quaternion.Euler(e);

        Quaternion startRot = cam.rotation;

        Vector3 targetDir = (target.position - cam.position).normalized;
        Quaternion targetRot = Quaternion.LookRotation(targetDir, cam.up);

        float time = 0f;

        while (time < duration)
        {
            float t = time / duration;

            Quaternion rot = Quaternion.Slerp(startRot, targetRot, t);
            cam.rotation = RemoveRoll(rot);

            time += Time.deltaTime;
            yield return null;
        }

        cam.rotation = targetRot;
        cam.rotation = Quaternion.Euler(cam.eulerAngles.x, cam.eulerAngles.y, 0f);
    }

    Quaternion RemoveRoll(Quaternion q)
    {
        Vector3 fwd = q * Vector3.forward;

        return Quaternion.LookRotation(fwd, Vector3.up);
    }

    public void SetRotation(float yaw, float pitch)
    {
        rotationY = yaw;
        rotationX = pitch;

        transform.rotation = Quaternion.Euler(0f, rotationY, 0f);

        if (virtualCam != null)
            virtualCam.transform.localRotation = Quaternion.Euler(rotationX, 0f, 0f);
    }
}
