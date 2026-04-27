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
    public Flashlight flashlight;

    [Header("Interaction Settings")]
    public Camera playerCam;
    public float interactionDistance = 3f;
    public LayerMask interactionLayerMask;


    [Header("FMOD Footsteps")]
    [SerializeField] private EventReference footstepEvent;
    [SerializeField] private float walkStepInterval = 0.5f;
    [SerializeField] private float runStepInterval = 0.3f;
    private int currentTerrain = 0;

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


    [Header("Dialogue Zoom Settings")]
    public CinemachineCamera virtualCam;
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
    
    private float targetFOV;
    
    public static bool isGameEnded = false;

    public static PlayerComments playerComments;

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

        
        if (TutorialManager.Instance != null)
        {
            TutorialManager.Instance.ShowHint(TutorialManager.HintType.Movement_WASD);
        }
    }

    void Update()
    {
        
        DialogueManager dm = FindObjectOfType<DialogueManager>();
        if (dm != null && dm.IsDialogueActive())
        {
            
            StopFootsteps();

            if (Input.GetKeyDown(KeyCode.E) && !Pause.isPaused)
                dm.DisplayNextSentence();

            return;
        }

        if(playerComments != null)
        {
            if (Input.GetKeyDown(KeyCode.E) && !Pause.isPaused)
                playerComments.DisplayNextSentence();
        }

        HandleMovement();
        HandleCameraRotation();
        HandleDialogueZoom();
        HandleFootsteps();
        if (Input.GetKeyDown(KeyCode.E))
        {
            CheckInteractionInput();
        }
        
        if (playerCamera != null)
        {
            
            playerCamera.fieldOfView = Mathf.Lerp(playerCamera.fieldOfView, targetFOV, Time.deltaTime * zoomSpeed);
        }
        
    }

    public void SetBasementFootsteps()
    {
        currentTerrain = 1;
    }

    public void SetNormalFootsteps()
    {
        currentTerrain = 0;
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

        

        
    }

    public void ZoomIn(float value = 0.8f, float t = -1f)
    {
        if (currentZoomCoroutine != null)
            StopCoroutine(currentZoomCoroutine);

        float duration = (t < 0) ? dialogueZoomSpeed : t;

        currentZoomCoroutine = StartCoroutine(SmoothZoom(virtualCam.Lens.FieldOfView, virtualCam.Lens.FieldOfView * value, duration));
    }

    public void ZoomOut(float value = 0.8f, float t = -1f)
    {
        if (currentZoomCoroutine != null)
            StopCoroutine(currentZoomCoroutine);

        float duration = (t < 0) ? dialogueZoomSpeed : t;

        currentZoomCoroutine = StartCoroutine(SmoothZoom(virtualCam.Lens.FieldOfView, virtualCam.Lens.FieldOfView / value, duration));
    }

    private IEnumerator SmoothZoom(float from, float to, float time)
    {
        float elapsed = 0f;
        while (elapsed < time)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.SmoothStep(0f, 1f, elapsed / time);
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

            var instance = RuntimeManager.CreateInstance(footstepEvent);
            instance.set3DAttributes(RuntimeUtils.To3DAttributes(transform.position));
            instance.setParameterByName("Terrain", currentTerrain);
            instance.start();
            instance.release();

            yield return new WaitForSeconds(delay);
        }

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
     

    
    void CheckInteractionInput()
    {
        DialogueManager dm = FindFirstObjectByType<DialogueManager>();
        if (dm != null && dm.IsDialogueActive())
            return;

        RepairQTE qte = FindFirstObjectByType<RepairQTE>();
        if (qte != null && qte.isQTEActive)
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
        if (objectInteraction.IsHoldingObject())
        {
            if (hitSomething)
            {
                Debug.Log(hitSomething);
                PlacementSpot spot = hit.collider.GetComponent<PlacementSpot>();
                if (spot != null && spot.requiredItemID == objectInteraction.GetHeldItemID())
                {
                    objectInteraction.PlaceObject(spot);
                    return;
                }

                if (hit.collider.CompareTag("Door"))
                {
                    hit.collider.GetComponent<DoorController>().TryOpenDoor();
                    return;
                }

                if (hit.collider.CompareTag("Switch"))
                {
                    hit.collider.GetComponent<LightSwitch>().Interact();
                    return;
                }

                if (hit.collider.CompareTag("LesterDoor") && QuestManagerV2.Instance.IsGoalRequired("Trigger Q2 (Door vremenaja)", GoalType.TalkToNPC))
                {
                    hit.collider.GetComponent<LesterDoor>().Interact();
                    return;
                }
            }

            objectInteraction.DropObject();
            return;
        }

        
        if (hitSomething)
        {
            if(hit.collider.CompareTag("Flashlight") && QuestManagerV2.Instance.IsGoalRequired("flashlight", GoalType.ReturnItem))
            {
                hit.transform.gameObject.SetActive(false);
                flashlight.enabled = true;
            }

            NPC_Dialogue npcDialogue = hit.collider.GetComponent<NPC_Dialogue>();
            if (npcDialogue != null)
            {
                Debug.Log("[PlayerController] Trigger NPC_Dialogue");
                npcDialogue.TriggerDialogue();
                return;
            }

            if (hit.collider.CompareTag("LesterDoor") && QuestManagerV2.Instance.IsGoalRequired("Trigger Q2 (Door vremenaja)", GoalType.TalkToNPC))
            {
                hit.collider.GetComponent<LesterDoor>().Interact();
                return;
            }

            if (hit.collider.CompareTag("Pickable"))
            {
                objectInteraction.PickupObject(hit.collider.gameObject);
                return;
            }

            if (hit.collider.CompareTag("Switch"))
            {
                hit.collider.GetComponent<LightSwitch>().Interact();
                return;
            }

            InteractableObject interactable = hit.collider.GetComponent<InteractableObject>();
            if (interactable != null)
            {
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
        canMove = false;
                         
    }
    public void StartCinematicPan(Transform target, float duration)
    {
        isCinematic = true;
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
