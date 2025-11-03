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
    private float rotationY = 0f;
    

    [Header("Dialogue Zoom Settings")]
    public CinemachineCamera virtualCam;   // виртуальная камера для зума
    public float dialogueZoomFOV = 40f;
    public float dialogueZoomSpeed = 2f;

    private bool canMove = true;
    private bool dialogueZoom = false;
    private float initialFOV;

    // --- Ссылка на ObjectInteraction ---
    private ObjectInteraction objectInteraction;

    void Start()
    {
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
            if (Input.GetKeyDown(KeyCode.E))
                dm.DisplayNextSentence(); // листаем реплики
            return; // блокируем остальное управление
        }

        HandleMovement();
        HandleCameraRotation();
        HandleDialogueZoom();
        HandleFootsteps();
        CheckInteractionInput();
    }


    private void HandleMovement()
    {
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
        if (!canMove) return;

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

        float targetFOV = dialogueZoom ? dialogueZoomFOV : initialFOV;

        virtualCam.Lens.FieldOfView = Mathf.Lerp(
            virtualCam.Lens.FieldOfView,
            targetFOV,
            Time.deltaTime * dialogueZoomSpeed
        );
    }

    private void HandleFootsteps()
    {
        bool isMoving = (Input.GetAxis("Vertical") != 0f || Input.GetAxis("Horizontal") != 0f) && canMove;

        if (isMoving && !isFootstepCoroutineRunning)
        {
            isWalking = true;
            StartCoroutine(PlayFootstepSounds());
        }
        else if (!isMoving)
        {
            isWalking = false;
        }
    }

    private IEnumerator PlayFootstepSounds()
    {
        isFootstepCoroutineRunning = true;
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

        isFootstepCoroutineRunning = false;
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
    }

    public void SetDialogueZoom(bool value)
    {
        dialogueZoom = value;
    }

    // --- Механика взаимодействия с предметами ---
    // Файл: PlayerController.cs
    // Вставь этот метод целиком (вместо старого)

    void CheckInteractionInput()
    {
        // --- ФИКС: если диалог активен, игнорируем взаимодействие (чтобы E не перезапускал его)
        DialogueManager dm = FindObjectOfType<DialogueManager>();
        if (dm != null && dm.IsDialogueActive())
            return;

        if (Input.GetKeyDown(KeyCode.E))
        {
            Ray ray = playerCam.ScreenPointToRay(new Vector3(Screen.width / 2, Screen.height / 2));
            RaycastHit hit;
            bool hitSomething = Physics.Raycast(ray, out hit, interactionDistance, interactionLayerMask);

            if (objectInteraction.IsHoldingObject())
            {
                if (hitSomething)
                {
                    PlacementSpot spot = hit.collider.GetComponent<PlacementSpot>();
                    if (spot != null && spot.requiredItemID == objectInteraction.GetHeldItemID())
                    {
                        objectInteraction.PlaceObject(spot);
                        return;
                    }
                }
                objectInteraction.DropObject();
            }
            else
            {
                if (hitSomething)
                {
                    // 1️⃣ Проверяем — есть ли NPC_Dialogue
                    NPC_Dialogue npcDialogue = hit.collider.GetComponent<NPC_Dialogue>();
                    if (npcDialogue != null)
                    {
                        Debug.Log("[PlayerController] Trigger NPC_Dialogue");
                        npcDialogue.TriggerDialogue();
                        return;
                    }

                    // 2️⃣ Проверяем предмет
                    if (hit.collider.CompareTag("Pickable"))
                    {
                        Debug.Log("[PlayerController] Pickup Pickable");
                        objectInteraction.PickupObject(hit.collider.gameObject);
                        return;
                    }

                    // 3️⃣ Проверяем InteractableObject (щиток и т.д.)
                    InteractableObject interactable = hit.collider.GetComponent<InteractableObject>();
                    if (interactable != null)
                    {
                        Debug.Log("[PlayerController] Interact with InteractableObject");
                        interactable.Interact();
                        return;
                    }
                }
            }
        }
    }
    public void LockMovementButAllowLook()
    {
        canMove = false; // блокируем движение
                         // но не трогаем камеру — игрок может осматриваться
    }

}
