using System.Collections;
using UnityEngine;

/// <summary>
/// Singleton that manages layered background music.
/// Maintains a base ambient layer (per-floor theme) and a tension overlay that fades based on monster state.
/// Persists across scene loads via DontDestroyOnLoad.
/// </summary>
public class MusicManager : MonoBehaviour
{
    public static MusicManager Instance { get; private set; }

    [Header("Audio Sources")]
    [SerializeField] private AudioSource baseLayer;
    [SerializeField] private AudioSource tensionLayer;

    [Header("Clips")]
    [Tooltip("One ambient clip per floor. Index 0 = Basement, 1 = Floor 1, ..., 4 = Floor 4.")]
    [SerializeField] private AudioClip[] floorAmbientClips;
    [SerializeField] private AudioClip tensionClip;
    [SerializeField] private AudioClip mainMenuClip;

    [Header("Volume")]
    [SerializeField] private float baseMaxVolume = 0.4f;
    [SerializeField] private float tensionMaxVolume = 0.6f;
    [SerializeField] private float tensionFadeSpeed = 2f;
    [SerializeField] private float crossfadeDuration = 1.5f;

    private float targetTensionVolume = 0f;
    private Coroutine crossfadeCoroutine;

    private void Awake()
    {
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    private void Start()
    {
        if (tensionLayer != null && tensionClip != null)
        {
            tensionLayer.clip = tensionClip;
            tensionLayer.loop = true;
            tensionLayer.volume = 0f;
            tensionLayer.Play();
        }

        if (baseLayer != null)
        {
            baseLayer.loop = true;
            baseLayer.volume = baseMaxVolume;
        }
    }

    private void Update()
    {
        if (tensionLayer == null) return;
        tensionLayer.volume = Mathf.MoveTowards(
            tensionLayer.volume,
            targetTensionVolume,
            tensionFadeSpeed * Time.deltaTime);
    }

    /// <summary>
    /// Sets the tension overlay level. 0 = silent, 1 = full volume.
    /// Called by MonsterAI each frame.
    /// </summary>
    public void SetTensionLevel(float tension)
    {
        targetTensionVolume = Mathf.Clamp01(tension) * tensionMaxVolume;
    }

    /// <summary>
    /// Crossfades the base ambient layer to the clip matching the given floor.
    /// Floor index: -1 = Basement (mapped to index 0), 0 = Floor 1 (index 1), etc.
    /// Called by FloorManager.OnFloorChanged.
    /// </summary>
    public void SetFloorTheme(int floor)
    {
        if (floorAmbientClips == null || floorAmbientClips.Length == 0) return;

        int clipIndex = Mathf.Clamp(floor + 1, 0, floorAmbientClips.Length - 1);
        AudioClip targetClip = floorAmbientClips[clipIndex];

        if (targetClip == null || (baseLayer != null && baseLayer.clip == targetClip)) return;

        if (crossfadeCoroutine != null)
            StopCoroutine(crossfadeCoroutine);

        crossfadeCoroutine = StartCoroutine(CrossfadeCoroutine(baseLayer, targetClip, crossfadeDuration));
    }

    /// <summary>Plays the main menu background theme on the base layer immediately.</summary>
    public void PlayMainMenuTheme()
    {
        if (baseLayer == null || mainMenuClip == null) return;

        if (crossfadeCoroutine != null)
            StopCoroutine(crossfadeCoroutine);

        crossfadeCoroutine = StartCoroutine(CrossfadeCoroutine(baseLayer, mainMenuClip, crossfadeDuration));
        targetTensionVolume = 0f;
    }

    /// <summary>Stops all music immediately.</summary>
    public void StopAll()
    {
        if (crossfadeCoroutine != null)
            StopCoroutine(crossfadeCoroutine);

        baseLayer?.Stop();
        tensionLayer?.Stop();
    }

    private IEnumerator CrossfadeCoroutine(AudioSource source, AudioClip newClip, float duration)
    {
        if (source == null) yield break;

        float halfDuration = duration * 0.5f;
        float startVolume = source.volume;

        // Fade out
        float elapsed = 0f;
        while (elapsed < halfDuration)
        {
            elapsed += Time.deltaTime;
            source.volume = Mathf.Lerp(startVolume, 0f, elapsed / halfDuration);
            yield return null;
        }

        source.Stop();
        source.clip = newClip;
        source.Play();

        // Fade in
        elapsed = 0f;
        while (elapsed < halfDuration)
        {
            elapsed += Time.deltaTime;
            source.volume = Mathf.Lerp(0f, baseMaxVolume, elapsed / halfDuration);
            yield return null;
        }

        source.volume = baseMaxVolume;
    }
}
