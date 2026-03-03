using UnityEngine;
using UnityEngine.AI;

/// <summary>
/// The stalker monster. Uses NavMeshAgent to navigate the scene with a three-state FSM:
/// Roaming, Chasing, and Attacking. Aggression scales with each note collected.
/// Subscribes to FloorManager.OnFloorChanged to optionally pursue the player's floor via stairwell waypoints.
/// Pushes a tension level to MusicManager each frame based on current state.
/// </summary>
[RequireComponent(typeof(NavMeshAgent))]
[RequireComponent(typeof(MonsterDetection))]
public class MonsterAI : MonoBehaviour
{
    public enum MonsterState { Roaming, Chasing, Attacking }
    public MonsterState CurrentState { get; private set; } = MonsterState.Roaming;

    [Header("Movement")]
    [SerializeField] private float baseSpeed = 2f;
    [SerializeField] private float speedIncrement = 0.4f;
    [SerializeField] private float roamRadius = 20f;
    [SerializeField] private float waypointReachedThreshold = 1.5f;

    [Header("Attack")]
    [SerializeField] private float attackRange = 1.5f;

    [Header("Sanity Drain")]
    [SerializeField] private float sanityDrainRate = 0.08f;
    [SerializeField] private float sanityRestoreRate = 0.02f;

    [Header("Multi-Floor")]
    [Tooltip("One waypoint per floor transition (stairwell entry points). Index maps to floor number + 1, matching FloorManager.")]
    [SerializeField] private Transform[] stairwellWaypoints;
    [Tooltip("Probability (0–1) that the monster will move toward the player's floor when OnFloorChanged fires.")]
    [SerializeField] private float floorChaseProbability = 0.5f;

    private NavMeshAgent navMeshAgent;
    private MonsterDetection detection;
    private Transform playerTransform;

    private int notesCollected = 0;
    private int totalNotes = 8;
    private Vector3 currentWaypoint;

    private void Awake()
    {
        navMeshAgent = GetComponent<NavMeshAgent>();
        detection = GetComponent<MonsterDetection>();
    }

    private void Start()
    {
        GameObject playerObj = GameObject.FindWithTag("Player");
        if (playerObj != null)
            playerTransform = playerObj.transform;

        navMeshAgent.speed = baseSpeed;
        SetNewRoamWaypoint();

        if (NoteCollectionManager.Instance != null)
            totalNotes = NoteCollectionManager.Instance.TotalNotes;

        if (FloorManager.Instance != null)
            FloorManager.Instance.OnFloorChanged += OnPlayerFloorChanged;
    }

    private void OnDestroy()
    {
        if (FloorManager.Instance != null)
            FloorManager.Instance.OnFloorChanged -= OnPlayerFloorChanged;
    }

    private void OnPlayerFloorChanged(int newFloor)
    {
        if (CurrentState != MonsterState.Roaming) return;
        if (stairwellWaypoints == null || stairwellWaypoints.Length == 0) return;

        if (Random.value <= floorChaseProbability)
        {
            int waypointIndex = Mathf.Clamp(newFloor + 1, 0, stairwellWaypoints.Length - 1);
            Transform target = stairwellWaypoints[waypointIndex];
            if (target != null)
                navMeshAgent.SetDestination(target.position);
        }
    }

    private void Update()
    {
        if (GameManager.Instance != null && GameManager.Instance.CurrentState != GameManager.GameState.Playing)
            return;

        switch (CurrentState)
        {
            case MonsterState.Roaming:
                UpdateRoaming();
                break;
            case MonsterState.Chasing:
                UpdateChasing();
                break;
            case MonsterState.Attacking:
                UpdateAttacking();
                break;
        }

        HandleAudio();
        HandleSanity();
        PushTensionLevel();
    }

    private void UpdateRoaming()
    {
        // Check if we should transition to chasing
        if (detection.CanSeePlayer() || detection.IsNearPlayer())
        {
            TransitionTo(MonsterState.Chasing);
            return;
        }

        // Navigate to current waypoint
        if (!navMeshAgent.pathPending && navMeshAgent.remainingDistance < waypointReachedThreshold)
            SetNewRoamWaypoint();
    }

    private void UpdateChasing()
    {
        if (playerTransform == null) return;

        // Check if close enough to attack
        float distanceToPlayer = Vector3.Distance(transform.position, playerTransform.position);
        if (distanceToPlayer <= attackRange)
        {
            TransitionTo(MonsterState.Attacking);
            return;
        }

        // If lost sight for a moment, keep chasing — revert to roam only when very far
        if (distanceToPlayer > detection.DetectionRange * 2f && !detection.CanSeePlayer())
        {
            TransitionTo(MonsterState.Roaming);
            return;
        }

        navMeshAgent.SetDestination(playerTransform.position);
    }

    private void UpdateAttacking()
    {
        navMeshAgent.ResetPath();
        GameManager.Instance?.OnPlayerCaught();
        // State will be changed by GameManager — stop further updates
        TransitionTo(MonsterState.Roaming);
    }

    private void SetNewRoamWaypoint()
    {
        currentWaypoint = GetRandomNavMeshPoint(transform.position, roamRadius);
        navMeshAgent.SetDestination(currentWaypoint);
    }

    private Vector3 GetRandomNavMeshPoint(Vector3 origin, float range)
    {
        Vector3 randomDirection = Random.insideUnitSphere * range + origin;
        if (NavMesh.SamplePosition(randomDirection, out NavMeshHit hit, range, NavMesh.AllAreas))
            return hit.position;
        return origin;
    }

    private void TransitionTo(MonsterState newState)
    {
        CurrentState = newState;
    }

    private void HandleAudio()
    {
        if (playerTransform == null) return;
        float distance = Vector3.Distance(transform.position, playerTransform.position);
        bool chasing = CurrentState == MonsterState.Chasing || CurrentState == MonsterState.Attacking;
        AudioManager.Instance?.PlayMonsterBreathing(chasing);
        AudioManager.Instance?.SetMonsterAudioDistance(distance);
    }

    private void HandleSanity()
    {
        if (detection.CanSeePlayer())
            SanityManager.Instance?.DrainSanity(sanityDrainRate);
        else
            SanityManager.Instance?.RestoreSanity(sanityRestoreRate);
    }

    /// <summary>
    /// Called by GameManager when a note is collected. Increases speed and detection range.
    /// </summary>
    public void OnNoteCollected(int totalNotesCollected)
    {
        notesCollected = totalNotesCollected;
        navMeshAgent.speed = baseSpeed + notesCollected * speedIncrement;
        detection.ScaleDetectionRange(notesCollected);
    }

    private void PushTensionLevel()
    {
        float tension = CurrentState switch
        {
            MonsterState.Roaming   => 0f,
            MonsterState.Chasing   => Mathf.Lerp(0.3f, 0.8f, (float)notesCollected / Mathf.Max(1, totalNotes)),
            MonsterState.Attacking => 1f,
            _                      => 0f
        };

        MusicManager.Instance?.SetTensionLevel(tension);
    }
}
