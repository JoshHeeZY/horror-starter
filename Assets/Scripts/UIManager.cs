using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Singleton that manages all UI panels: HUD, note display, pause menu, game over, and win screens.
/// All panels are child GameObjects toggled via SetActive.
/// Battery bar uses an Image with FillMethod set to Horizontal (fill amount 0–1).
/// </summary>
public class UIManager : MonoBehaviour
{
    public static UIManager Instance { get; private set; }

    [Header("HUD")]
    [SerializeField] private GameObject hudPanel;
    [SerializeField] private TextMeshProUGUI noteCounterText;
    [SerializeField] private Image batteryFillImage;
    [SerializeField] private GameObject interactPrompt;

    [Header("Battery Warning")]
    [SerializeField] private Color batteryWarningColor = new Color(1f, 0.25f, 0f);
    [SerializeField] private Color batteryNormalColor  = Color.white;
    [SerializeField] private float batteryPulseSpeed   = 2.5f;

    [Header("Note Display")]
    [SerializeField] private GameObject noteTextPanel;
    [SerializeField] private TextMeshProUGUI noteContentText;

    [Header("Screens")]
    [SerializeField] private GameObject pauseMenuPanel;
    [SerializeField] private GameObject gameOverPanel;
    [SerializeField] private GameObject winPanel;

    private Coroutine batteryPulseCoroutine;

    private void Awake()
    {
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    private void Start()
    {
        ShowPauseMenu(false);
        gameOverPanel?.SetActive(false);
        winPanel?.SetActive(false);
        noteTextPanel?.SetActive(false);
        interactPrompt?.SetActive(false);
        hudPanel?.SetActive(true);
        UpdateNoteCounter(0, NoteCollectionManager.Instance != null ? NoteCollectionManager.Instance.TotalNotes : 8);

        if (batteryFillImage != null)
            batteryFillImage.color = batteryNormalColor;
    }

    /// <summary>Updates the note counter label on the HUD.</summary>
    public void UpdateNoteCounter(int collected, int total)
    {
        if (noteCounterText != null)
            noteCounterText.text = $"Notes: {collected} / {total}";
    }

    /// <summary>
    /// Updates the battery fill image amount (0–1).
    /// The Image must have Image Type set to Filled with Fill Method Horizontal.
    /// </summary>
    public void UpdateBatteryBar(float batteryLevel)
    {
        if (batteryFillImage != null)
            batteryFillImage.fillAmount = Mathf.Clamp01(batteryLevel);
    }

    /// <summary>
    /// Enables or disables the low-battery pulsing color warning on the battery fill image.
    /// </summary>
    public void SetBatteryWarning(bool active)
    {
        if (batteryFillImage == null) return;

        if (active)
        {
            if (batteryPulseCoroutine == null)
                batteryPulseCoroutine = StartCoroutine(BatteryPulseCoroutine());
        }
        else
        {
            if (batteryPulseCoroutine != null)
            {
                StopCoroutine(batteryPulseCoroutine);
                batteryPulseCoroutine = null;
            }
            batteryFillImage.color = batteryNormalColor;
        }
    }

    private IEnumerator BatteryPulseCoroutine()
    {
        while (true)
        {
            float t = (Mathf.Sin(Time.unscaledTime * batteryPulseSpeed) + 1f) * 0.5f;
            if (batteryFillImage != null)
                batteryFillImage.color = Color.Lerp(batteryNormalColor, batteryWarningColor, t);
            yield return null;
        }
    }

    /// <summary>Displays the note content panel with the given text.</summary>
    public void ShowNoteText(string text)
    {
        if (noteContentText != null)
            noteContentText.text = text;
        noteTextPanel?.SetActive(true);
    }

    /// <summary>Hides the note content panel.</summary>
    public void HideNoteText()
    {
        noteTextPanel?.SetActive(false);
    }

    /// <summary>Shows or hides the interaction prompt ("E to pick up").</summary>
    public void ShowInteractPrompt(bool show)
    {
        interactPrompt?.SetActive(show);
    }

    /// <summary>Activates the Game Over screen.</summary>
    public void ShowGameOver()
    {
        hudPanel?.SetActive(false);
        gameOverPanel?.SetActive(true);
    }

    /// <summary>Activates the Win screen.</summary>
    public void ShowWin()
    {
        hudPanel?.SetActive(false);
        winPanel?.SetActive(true);
    }

    /// <summary>Shows or hides the pause menu panel.</summary>
    public void ShowPauseMenu(bool show)
    {
        pauseMenuPanel?.SetActive(show);
    }
}
