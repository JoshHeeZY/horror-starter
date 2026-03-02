using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Component placed on each note GameObject.
/// Handles player interaction via raycast and displays note content on pickup.
/// Assign the InputActionAsset in the Inspector.
/// Action map must be named "Player" with an "Interact" button action.
/// </summary>
public class NotePickup : MonoBehaviour
{
    [Header("Input")]
    [SerializeField] private InputActionAsset inputActionsAsset;

    [Tooltip("The text content written on this note.")]
    [SerializeField] public string NoteText;

    [Tooltip("Unique index for this note (1–8).")]
    [SerializeField] public int NoteIndex;

    public bool IsCollected { get; private set; }

    private const float InteractDistance = 2.5f;
    private const string PlayerTag = "Player";

    private InputAction interactAction;
    private Camera playerCamera;
    private bool playerIsLooking = false;

    private void Awake()
    {
        if (inputActionsAsset != null)
        {
            InputActionMap playerMap = inputActionsAsset.FindActionMap("Player", throwIfNotFound: false);
            interactAction = playerMap?.FindAction("Interact");
        }
    }

    private void OnEnable()  => interactAction?.Enable();
    private void OnDisable() => interactAction?.Disable();

    private void Start()
    {
        GameObject playerObj = GameObject.FindWithTag(PlayerTag);
        if (playerObj != null)
            playerCamera = playerObj.GetComponentInChildren<Camera>();
    }

    private void Update()
    {
        if (IsCollected) return;
        if (GameManager.Instance != null && GameManager.Instance.CurrentState != GameManager.GameState.Playing) return;
        if (playerCamera == null) return;

        CheckPlayerLookingAt();

        if (playerIsLooking && (interactAction?.WasPressedThisFrame() ?? false))
            Collect();
    }

    private void CheckPlayerLookingAt()
    {
        Ray ray = new Ray(playerCamera.transform.position, playerCamera.transform.forward);
        if (Physics.Raycast(ray, out RaycastHit hit, InteractDistance))
        {
            if (hit.collider.gameObject == gameObject)
            {
                playerIsLooking = true;
                UIManager.Instance?.ShowInteractPrompt(true);
                return;
            }
        }

        playerIsLooking = false;
        UIManager.Instance?.ShowInteractPrompt(false);
    }

    private void Collect()
    {
        IsCollected = true;
        UIManager.Instance?.ShowInteractPrompt(false);
        UIManager.Instance?.ShowNoteText(NoteText);
        AudioManager.Instance?.PlayNotePickup();
        NoteCollectionManager.Instance?.CollectNote(this);
        gameObject.SetActive(false);
    }
}
