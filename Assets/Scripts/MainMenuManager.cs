using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>
/// Controls the main menu scene — Play/Settings/Quit button callbacks, settings panel toggling,
/// and PlayerPrefs persistence for sensitivity and master volume.
/// Attach to a MenuController GameObject in MainMenu.unity.
/// This script is destroyed with the scene when the game loads.
/// </summary>
public class MainMenuManager : MonoBehaviour
{
    private const string KeySensitivity = "MouseSensitivity";
    private const string KeyVolume = "MasterVolume";
    private const string GameSceneName = "AbandonedSchool";

    private const float DefaultSensitivity = 0.15f;
    private const float DefaultVolume = 1f;

    [Header("Panels")]
    [SerializeField] private GameObject mainPanel;
    [SerializeField] private GameObject settingsPanel;

    [Header("Settings Sliders")]
    [SerializeField] private Slider sensitivitySlider;
    [SerializeField] private Slider volumeSlider;

    [Header("Camera Pan")]
    [SerializeField] private ExteriorViewController exteriorViewController;

    private void Start()
    {
        settingsPanel?.SetActive(false);
        mainPanel?.SetActive(true);

        LoadAndApplySavedSettings();
        MusicManager.Instance?.PlayMainMenuTheme();

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    /// <summary>Loads the game scene. Stops camera pan and music crossfade.</summary>
    public void OnPlayClicked()
    {
        exteriorViewController?.StopPan();
        SceneManager.LoadScene(GameSceneName);
    }

    /// <summary>Quits the application (or exits play mode in the Editor).</summary>
    public void OnQuitClicked()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    /// <summary>Toggles the settings panel visibility.</summary>
    public void OnSettingsClicked()
    {
        if (settingsPanel == null) return;
        bool isActive = settingsPanel.activeSelf;
        settingsPanel.SetActive(!isActive);
        mainPanel?.SetActive(isActive);
    }

    /// <summary>Saves mouse sensitivity to PlayerPrefs and writes the value immediately.</summary>
    public void OnSensitivityChanged(float value)
    {
        PlayerPrefs.SetFloat(KeySensitivity, value);
        PlayerPrefs.Save();
    }

    /// <summary>Saves master volume to PlayerPrefs and applies it to the AudioListener.</summary>
    public void OnVolumeChanged(float value)
    {
        PlayerPrefs.SetFloat(KeyVolume, value);
        PlayerPrefs.Save();
        AudioListener.volume = value;
    }

    private void LoadAndApplySavedSettings()
    {
        float savedSensitivity = PlayerPrefs.GetFloat(KeySensitivity, DefaultSensitivity);
        float savedVolume = PlayerPrefs.GetFloat(KeyVolume, DefaultVolume);

        if (sensitivitySlider != null)
        {
            sensitivitySlider.value = savedSensitivity;
        }

        if (volumeSlider != null)
        {
            volumeSlider.value = savedVolume;
        }

        AudioListener.volume = savedVolume;
    }
}
