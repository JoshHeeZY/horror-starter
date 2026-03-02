using UnityEngine;

/// <summary>
/// Singleton that manages ambient audio, monster sounds, and event-based one-shots.
/// Uses multiple AudioSource components — one per audio category.
/// </summary>
public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    [Header("Audio Sources")]
    [SerializeField] private AudioSource ambientSource;
    [SerializeField] private AudioSource monsterSource;
    [SerializeField] private AudioSource sfxSource;

    [Header("Clips")]
    [SerializeField] private AudioClip ambientClip;
    [SerializeField] private AudioClip monsterBreathingClip;
    [SerializeField] private AudioClip jumpscareClip;
    [SerializeField] private AudioClip notePickupClip;
    [SerializeField] private AudioClip heartbeatClip;

    private const float MaxMonsterVolume = 1f;
    private const float MinMonsterVolume = 0f;
    private const float MonsterAudioMaxDistance = 20f;

    private void Awake()
    {
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    private void Start()
    {
        PlayAmbient();
    }

    /// <summary>Starts looping the ambient audio track.</summary>
    public void PlayAmbient()
    {
        if (ambientClip == null || ambientSource == null) return;
        ambientSource.clip = ambientClip;
        ambientSource.loop = true;
        ambientSource.Play();
    }

    /// <summary>Starts or stops the looping monster breathing audio.</summary>
    public void PlayMonsterBreathing(bool play)
    {
        if (monsterSource == null || monsterBreathingClip == null) return;
        if (play)
        {
            if (!monsterSource.isPlaying)
            {
                monsterSource.clip = monsterBreathingClip;
                monsterSource.loop = true;
                monsterSource.Play();
            }
        }
        else
        {
            monsterSource.Stop();
        }
    }

    /// <summary>Plays the jumpscare audio clip as a one-shot.</summary>
    public void PlayJumpscare()
    {
        if (sfxSource == null || jumpscareClip == null) return;
        sfxSource.PlayOneShot(jumpscareClip);
    }

    /// <summary>Plays the note pickup sound effect.</summary>
    public void PlayNotePickup()
    {
        if (sfxSource == null || notePickupClip == null) return;
        sfxSource.PlayOneShot(notePickupClip);
    }

    /// <summary>Plays the heartbeat audio cue as a one-shot.</summary>
    public void PlayHeartbeat()
    {
        if (sfxSource == null || heartbeatClip == null) return;
        sfxSource.PlayOneShot(heartbeatClip);
    }

    /// <summary>Adjusts monster audio volume based on proximity to the player.</summary>
    public void SetMonsterAudioDistance(float distance)
    {
        if (monsterSource == null) return;
        float normalizedDistance = Mathf.Clamp01(distance / MonsterAudioMaxDistance);
        monsterSource.volume = Mathf.Lerp(MaxMonsterVolume, MinMonsterVolume, normalizedDistance);
    }
}
