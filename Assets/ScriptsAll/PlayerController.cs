// Assets/Scripts/PlayerController.cs
using System.Collections;
using UnityEngine;
using FMODUnity;

[RequireComponent(typeof(CharacterController))]
public class PlayerController : MonoBehaviour
{
    [Header("Camera")]
    public Camera playerCam;

    [Header("Movement Settings")]
    public float walkSpeed = 3f;
    public float runSpeed = 5f;
    public float jumpPower = 0f;
    public float gravity = 10f;

    [Header("Camera Settings")]
    public float lookSpeed = 2f;
    public float lookXLimit = 75f;
    public float cameraRotationSmooth = 5f;

    // --- ДОБАВЛЕНО (Шаг 8): Настройки взаимодействия ---
    [Header("Interaction Settings")]
    public float interactionDistance = 3f;
    [Tooltip("Выбери ВСЕ слои, с которыми можно взаимодействовать (напр. 'Default' и 'Interactable')")]
    public LayerMask interactionLayerMask; 
    // --------------------------------------------------

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
    private float rotationX = 0;
    private float rotationY = 0;

    [Header("Camera Zoom Settings")]
    public int ZoomFOV = 35;
    public int initialFOV;
    public float cameraZoomSmooth = 1;
    private bool isZoomed = false;

    private bool canMove = true;

    // --- ДОБАВЛЕНО (Шаг 8): Ссылка на ObjectInteraction ---
    private ObjectInteraction objectInteraction;
    // ----------------------------------------------------

    void Start()
    {
        characterController = GetComponent<CharacterController>();
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        if (ambientEvent.IsNull == false)
        {
            ambientInstance = RuntimeManager.CreateInstance(ambientEvent);
            ambientInstance.start();
        }
        
        // --- ДОБАВЛЕНО (Шаг 8): Получаем компонент ---
        objectInteraction = GetComponent<ObjectInteraction>();
        if (objectInteraction == null)
            Debug.LogError("На игроке отсутствует скрипт ObjectInteraction!");
        // ---------------------------------------------
    }

    void Update()
    {
        // ... (твой код движения, камеры, зума) ...
        Vector3 forward = transform.TransformDirection(Vector3.forward);
        Vector3 right = transform.TransformDirection(Vector3.right);

        float curSpeedX = canMove ? (Input.GetKey(KeyCode.LeftShift) ? runSpeed : walkSpeed) * Input.GetAxis("Vertical") : 0;
        float curSpeedY = canMove ? (Input.GetKey(KeyCode.LeftShift) ? runSpeed : walkSpeed) * Input.GetAxis("Horizontal") : 0;
        float movementDirectionY = moveDirection.y;
        moveDirection = (forward * curSpeedX) + (right * curSpeedY);

        if (Input.GetButton("Jump") && canMove && characterController.isGrounded)
            moveDirection.y = jumpPower;
        else
            moveDirection.y = movementDirectionY;

        if (!characterController.isGrounded)
            moveDirection.y -= gravity * Time.deltaTime;

        characterController.Move(moveDirection * Time.deltaTime);

        if (canMove)
        {
            rotationX -= Input.GetAxis("Mouse Y") * lookSpeed;
            rotationX = Mathf.Clamp(rotationX, -lookXLimit, lookXLimit);
            rotationY += Input.GetAxis("Mouse X") * lookSpeed;

            Quaternion targetRotationX = Quaternion.Euler(rotationX, 0, 0);
            Quaternion targetRotationY = Quaternion.Euler(0, rotationY, 0);

            playerCam.transform.localRotation = Quaternion.Slerp(playerCam.transform.localRotation, targetRotationX, Time.deltaTime * cameraRotationSmooth);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotationY, Time.deltaTime * cameraRotationSmooth);
        }

        if (Input.GetButtonDown("Fire2")) isZoomed = true;
        if (Input.GetButtonUp("Fire2")) isZoomed = false;

        playerCam.fieldOfView = Mathf.Lerp(
            playerCam.fieldOfView,
            isZoomed ? ZoomFOV : initialFOV,
            Time.deltaTime * cameraZoomSmooth
        );

        // Footstep control
        bool isMoving = (curSpeedX != 0f || curSpeedY != 0f);
        if (isMoving && !isFootstepCoroutineRunning)
        {
            isWalking = true;
            StartCoroutine(PlayFootstepSounds());
        }
        else if (!isMoving)
        {
            isWalking = false;
        }

        // --- ДОБАВЛЕНО (Шаг 8): Вызов новой функции ---
        CheckInteractionInput();
        // ----------------------------------------------
    }
    
    // --- ДОБАВЛЕН ЦЕЛЫЙ НОВЫЙ МЕТОД (Шаг 8) ---
    void CheckInteractionInput()
    {
        if (Input.GetKeyDown(KeyCode.E))
        {
            // Пускаем луч. ВАЖНО: Он использует interactionLayerMask из PlayerController!
            Ray ray = playerCam.ScreenPointToRay(new Vector3(Screen.width / 2, Screen.height / 2));
            RaycastHit hit;
            bool hitSomething = Physics.Raycast(ray, out hit, interactionDistance, interactionLayerMask);

            // --- ЛОГИКА, ЕСЛИ МЫ ДЕРЖИМ ПРЕДМЕТ ---
            if (objectInteraction.IsHoldingObject())
            {
                if (hitSomething)
                {
                    PlacementSpot spot = hit.collider.GetComponent<PlacementSpot>();
                    if (spot != null && spot.requiredItemID == objectInteraction.GetHeldItemID())
                    {
                        objectInteraction.PlaceObject(spot); // Ставим предмет
                        return;
                    }
                }
                objectInteraction.DropObject(); // Если не попали в спот - бросаем
            }
            // --- ЛОГИКА, ЕСЛИ У НАС ПУСТЫЕ РУКИ ---
            else
            {
                if (hitSomething)
                {
                    // 1. Проверяем 'Pickable' (предмет)
                    if (hit.collider.CompareTag("Pickable"))
                    {
                        objectInteraction.PickupObject(hit.collider.gameObject);
                    }
                    // 2. Проверяем 'Interactable' (дверь, щиток)
                    else
                    {
                        InteractableObject interactable = hit.collider.GetComponent<InteractableObject>();
                        if (interactable != null)
                        {
                            interactable.Interact();
                        }
                    }
                }
            }
        }
    }
    // ------------------------------------------------

    void OnDestroy()
    {
        if (ambientInstance.isValid())
        {
            ambientInstance.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
            ambientInstance.release();
        }
    }

    private IEnumerator PlayFootstepSounds()
    {
        isFootstepCoroutineRunning = true;
        // ... (твой код) ...
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
}
