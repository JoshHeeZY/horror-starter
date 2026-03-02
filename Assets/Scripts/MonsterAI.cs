using UnityEngine;
using UnityEngine.AI;

/// <summary>
/// The stalker monster. Uses NavMeshAgent to navigate the scene with a three-state FSM:
/// Roaming, Chasing, and Attacking. Aggression scales with each note collected.
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

    private NavMeshAgent navMeshAgent;
    private MonsterDetection detection;
    private Transform playerTransform;

    private int notesCollected = 0;
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
    public void OnNoteCollected(int totalNotes)
    {
        notesCollected = totalNotes;
        navMeshAgent.speed = baseSpeed + notesCollected * speedIncrement;
        detection.ScaleDetectionRange(notesCollected);
    }
}
