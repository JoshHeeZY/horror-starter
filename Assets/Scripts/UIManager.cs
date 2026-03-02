using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Singleton that manages all UI panels: HUD, note display, pause menu, game over, and win screens.
/// All panels are child GameObjects toggled via SetActive.
/// </summary>
public class UIManager : MonoBehaviour
{
    public static UIManager Instance { get; private set; }

    [Header("HUD")]
    [SerializeField] private GameObject hudPanel;
    [SerializeField] private TextMeshProUGUI noteCounterText;
    [SerializeField] private Slider batteryBar;
    [SerializeField] private GameObject interactPrompt;

    [Header("Note Display")]
    [SerializeField] private GameObject noteTextPanel;
    [SerializeField] private TextMeshProUGUI noteContentText;

    [Header("Screens")]
    [SerializeField] private GameObject pauseMenuPanel;
    [SerializeField] private GameObject gameOverPanel;
    [SerializeField] private GameObject winPanel;

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
    }

    /// <summary>Updates the note counter label on the HUD.</summary>
    public void UpdateNoteCounter(int collected, int total)
    {
        if (noteCounterText != null)
            noteCounterText.text = $"Notes: {collected} / {total}";
    }

    /// <summary>Updates the battery bar slider value (0–1).</summary>
    public void UpdateBatteryBar(float batteryLevel)
    {
        if (batteryBar != null)
            batteryBar.value = batteryLevel;
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
