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
    public CinemachineCamera virtualCam;   // указываем виртуальную камеру в инспекторе
    public float dialogueZoomFOV = 40f;
    public float dialogueZoomSpeed = 2f;

    private bool canMove = true;
    private bool dialogueZoom = false;
    private float initialFOV;

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

        if (virtualCam != null)
            initialFOV = virtualCam.Lens.FieldOfView;
    }

    void Update()
    {
        HandleMovement();
        HandleCameraRotation();
        HandleDialogueZoom();
        HandleFootsteps();
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
        {
            virtualCam.transform.localRotation = Quaternion.Slerp(virtualCam.transform.localRotation, targetRotationX, Time.deltaTime * cameraRotationSmooth);
        }
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

    // --- Публичные методы для PlayerInteraction ---
    public void SetCanMove(bool value)
    {
        canMove = value;
    }

    public void SetDialogueZoom(bool value)
    {
        dialogueZoom = value;
    }
}
