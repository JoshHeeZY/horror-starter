using System.Collections;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

/// <summary>
/// Tracks a 0–1 sanity value and drives URP Volume post-processing effects for atmosphere.
/// </summary>
public class SanityManager : MonoBehaviour
{
    public static SanityManager Instance { get; private set; }

    public float Sanity { get; private set; } = 1f;

    [SerializeField] private Volume postProcessVolume;

    private Vignette vignette;
    private ChromaticAberration chromaticAberration;
    private FilmGrain filmGrain;

    private const float LowSanityThreshold = 0.3f;
    private const float HeartbeatCooldownMin = 5f;
    private const float HeartbeatCooldownMax = 12f;

    private bool heartbeatCoroutineRunning = false;

    private void Awake()
    {
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    private void Start()
    {
        if (postProcessVolume != null && postProcessVolume.profile != null)
        {
            postProcessVolume.profile.TryGet(out vignette);
            postProcessVolume.profile.TryGet(out chromaticAberration);
            postProcessVolume.profile.TryGet(out filmGrain);
        }

        ApplyPostProcessEffects();
    }

    /// <summary>Reduces the player's sanity by the given amount.</summary>
    public void DrainSanity(float amount)
    {
        Sanity = Mathf.Clamp01(Sanity - amount * Time.deltaTime);
        ApplyPostProcessEffects();

        if (Sanity < LowSanityThreshold && !heartbeatCoroutineRunning)
            StartCoroutine(HeartbeatRoutine());
    }

    /// <summary>Slightly restores the player's sanity by the given amount.</summary>
    public void RestoreSanity(float amount)
    {
        Sanity = Mathf.Clamp01(Sanity + amount * Time.deltaTime);
        ApplyPostProcessEffects();
    }

    private void ApplyPostProcessEffects()
    {
        float t = 1f - Sanity;

        if (vignette != null)
            vignette.intensity.value = Mathf.Lerp(0.2f, 0.8f, t);

        if (chromaticAberration != null)
            chromaticAberration.intensity.value = Mathf.Lerp(0f, 1f, t);

        if (filmGrain != null)
            filmGrain.intensity.value = Mathf.Lerp(0f, 0.8f, t);
    }

    private IEnumerator HeartbeatRoutine()
    {
        heartbeatCoroutineRunning = true;
        while (Sanity < LowSanityThreshold)
        {
            AudioManager.Instance?.PlayHeartbeat();
            float waitTime = Random.Range(HeartbeatCooldownMin, HeartbeatCooldownMax);
            yield return new WaitForSeconds(waitTime);
        }
        heartbeatCoroutineRunning = false;
    }
}
