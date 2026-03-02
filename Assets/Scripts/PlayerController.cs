using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// First-person character controller using the new Input System.
/// Handles movement, mouse look, stamina-based sprinting, and camera bob.
/// Assign the InputActionAsset (e.g. PlayerInputActions) in the Inspector.
/// Action map must be named "Player" with actions: Move, Look, Sprint, Pause.
/// </summary>
[RequireComponent(typeof(CharacterController))]
public class PlayerController : MonoBehaviour
{
    [Header("Input")]
    [SerializeField] private InputActionAsset inputActionsAsset;

    [Header("Movement")]
    public float WalkSpeed = 3f;
    public float SprintSpeed = 5.5f;

    [Header("Look")]
    [SerializeField] private float mouseSensitivity = 0.15f;
    private const float PitchMin = -80f;
    private const float PitchMax = 80f;

    [Header("Stamina")]
    [SerializeField] private float staminaDrainRate = 0.25f;
    [SerializeField] private float staminaRecoveryRate = 0.15f;
    public float Stamina { get; private set; } = 1f;
    public bool IsSprinting { get; private set; }

    [Header("Camera Bob")]
    [SerializeField] private float bobFrequency = 2f;
    [SerializeField] private float bobAmplitude = 0.05f;

    [Header("References")]
    [SerializeField] private Transform cameraTransform;
    public Transform CameraTransform => cameraTransform;

    [Header("Gravity")]
    [SerializeField] private float gravity = -9.81f;

    private CharacterController characterController;
    private InputAction moveAction;
    private InputAction lookAction;
    private InputAction sprintAction;
    private InputAction pauseAction;

    private float pitchAngle = 0f;
    private float verticalVelocity = 0f;
    private float bobTimer = 0f;
    private Vector3 cameraBaseLocalPosition;

    private void Awake()
    {
        characterController = GetComponent<CharacterController>();
        BindActions();
    }

    private void BindActions()
    {
        if (inputActionsAsset == null) return;
        InputActionMap playerMap = inputActionsAsset.FindActionMap("Player", throwIfNotFound: false);
        if (playerMap == null) return;

        moveAction   = playerMap.FindAction("Move");
        lookAction   = playerMap.FindAction("Look");
        sprintAction = playerMap.FindAction("Sprint");
        pauseAction  = playerMap.FindAction("Pause");
    }

    private void OnEnable()
    {
        moveAction?.Enable();
        lookAction?.Enable();
        sprintAction?.Enable();
        pauseAction?.Enable();
    }

    private void OnDisable()
    {
        moveAction?.Disable();
        lookAction?.Disable();
        sprintAction?.Disable();
        pauseAction?.Disable();
    }

    private void Start()
    {
        if (cameraTransform != null)
            cameraBaseLocalPosition = cameraTransform.localPosition;
    }

    private void Update()
    {
        if (GameManager.Instance != null && GameManager.Instance.CurrentState != GameManager.GameState.Playing)
            return;

        HandleLook();
        HandleMovement();
        HandleCameraBob();
        HandlePause();
    }

    private void HandleLook()
    {
        if (lookAction == null) return;
        Vector2 lookDelta = lookAction.ReadValue<Vector2>();
        float yaw   = lookDelta.x * mouseSensitivity;
        float pitch = lookDelta.y * mouseSensitivity;

        transform.Rotate(Vector3.up, yaw);

        pitchAngle = Mathf.Clamp(pitchAngle - pitch, PitchMin, PitchMax);
        if (cameraTransform != null)
            cameraTransform.localEulerAngles = new Vector3(pitchAngle, 0f, 0f);
    }

    private void HandleMovement()
    {
        Vector2 moveInput  = moveAction?.ReadValue<Vector2>() ?? Vector2.zero;
        bool    sprintHeld = sprintAction?.IsPressed() ?? false;

        IsSprinting = sprintHeld && Stamina > 0f && moveInput.magnitude > 0.1f;

        if (IsSprinting)
        {
            Stamina = Mathf.Clamp01(Stamina - staminaDrainRate * Time.deltaTime);
            if (Stamina <= 0f) IsSprinting = false;
        }
        else
        {
            Stamina = Mathf.Clamp01(Stamina + staminaRecoveryRate * Time.deltaTime);
        }

        float   speed   = IsSprinting ? SprintSpeed : WalkSpeed;
        Vector3 moveDir = transform.right * moveInput.x + transform.forward * moveInput.y;

        if (characterController.isGrounded)
            verticalVelocity = -0.5f;
        else
            verticalVelocity += gravity * Time.deltaTime;

        Vector3 velocity = moveDir * speed + Vector3.up * verticalVelocity;
        characterController.Move(velocity * Time.deltaTime);
    }

    private void HandleCameraBob()
    {
        if (cameraTransform == null) return;

        bool isMoving = (moveAction?.ReadValue<Vector2>().magnitude ?? 0f) > 0.1f
                        && characterController.isGrounded;

        if (isMoving)
        {
            bobTimer += Time.deltaTime * bobFrequency * (IsSprinting ? 1.5f : 1f);
            float bobOffset = Mathf.Sin(bobTimer) * bobAmplitude;
            cameraTransform.localPosition = cameraBaseLocalPosition + new Vector3(0f, bobOffset, 0f);
        }
        else
        {
            bobTimer = 0f;
            cameraTransform.localPosition = Vector3.Lerp(
                cameraTransform.localPosition,
                cameraBaseLocalPosition,
                Time.deltaTime * 5f);
        }
    }

    private void HandlePause()
    {
        if (pauseAction == null || !pauseAction.WasPressedThisFrame()) return;
        if (GameManager.Instance == null) return;

        bool isPaused = GameManager.Instance.CurrentState == GameManager.GameState.Paused;
        GameManager.Instance.ChangeState(isPaused ? GameManager.GameState.Playing : GameManager.GameState.Paused);
        UIManager.Instance?.ShowPauseMenu(!isPaused);
    }
}
