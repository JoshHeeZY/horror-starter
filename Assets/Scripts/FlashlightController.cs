using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Controls the player's flashlight — toggling, battery drain, and flickering as battery dies.
/// Attach to the Player GameObject. Assign the InputActionAsset and the Flashlight Light in the Inspector.
/// Action map must be named "Player" with a "Flashlight" button action.
/// </summary>
public class FlashlightController : MonoBehaviour
{
    [Header("Input")]
    [SerializeField] private InputActionAsset inputActionsAsset;

    [Header("Light")]
    [SerializeField] private Light flashlight;

    [Header("Battery")]
    [SerializeField] private float drainRate = 0.02f;
    private const float FlickerThreshold = 0.2f;
    private const float FlickerIntervalMin = 0.05f;
    private const float FlickerIntervalMax = 0.3f;

    [Header("Arms")]
    [SerializeField] private FPSArmsController armsController;

    public float BatteryLevel { get; private set; } = 1f;
    public bool IsOn { get; private set; } = true;

    private InputAction flashlightAction;
    private float defaultIntensity;
    private bool isFlickering = false;
    private bool batteryWarningTriggered = false;

    private void Awake()
    {
        if (inputActionsAsset != null)
        {
            InputActionMap playerMap = inputActionsAsset.FindActionMap("Player", throwIfNotFound: false);
            flashlightAction = playerMap?.FindAction("Flashlight");
        }

        if (flashlight != null)
            defaultIntensity = flashlight.intensity;
    }

    private void OnEnable()
    {
        flashlightAction?.Enable();
        if (flashlightAction != null)
            flashlightAction.performed += OnFlashlightToggle;
    }

    private void OnDisable()
    {
        if (flashlightAction != null)
            flashlightAction.performed -= OnFlashlightToggle;
        flashlightAction?.Disable();
    }

    private void Update()
    {
        if (!IsOn || BatteryLevel <= 0f) return;

        BatteryLevel = Mathf.Clamp01(BatteryLevel - drainRate * Time.deltaTime);
        UIManager.Instance?.UpdateBatteryBar(BatteryLevel);

        if (BatteryLevel <= 0f)
        {
            ForceOff();
            return;
        }

        // Trigger low-battery warning once when crossing the threshold
        if (BatteryLevel < FlickerThreshold && !batteryWarningTriggered)
        {
            batteryWarningTriggered = true;
            UIManager.Instance?.SetBatteryWarning(true);
        }
        else if (BatteryLevel >= FlickerThreshold && batteryWarningTriggered)
        {
            batteryWarningTriggered = false;
            UIManager.Instance?.SetBatteryWarning(false);
        }

        if (BatteryLevel < FlickerThreshold && !isFlickering)
            StartCoroutine(FlickerCoroutine());
    }

    /// <summary>Toggles the flashlight on or off if battery remains.</summary>
    public void Toggle()
    {
        if (BatteryLevel <= 0f) return;
        IsOn = !IsOn;
        if (flashlight != null)
            flashlight.enabled = IsOn;

        armsController?.SetFlashlightOn(IsOn);
    }

    private void OnFlashlightToggle(InputAction.CallbackContext ctx) => Toggle();

    private void ForceOff()
    {
        IsOn = false;
        if (flashlight != null)
            flashlight.enabled = false;
    }

    private IEnumerator FlickerCoroutine()
    {
        isFlickering = true;
        while (BatteryLevel > 0f && BatteryLevel < FlickerThreshold && IsOn)
        {
            if (flashlight != null)
                flashlight.intensity = Random.Range(0f, defaultIntensity);

            float waitTime = Random.Range(FlickerIntervalMin, FlickerIntervalMax);
            yield return new WaitForSeconds(waitTime);
        }

        // Restore intensity if battery recovered or light was toggled
        if (flashlight != null && IsOn)
            flashlight.intensity = defaultIntensity;

        isFlickering = false;
    }
}
