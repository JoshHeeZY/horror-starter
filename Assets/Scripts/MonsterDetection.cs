using UnityEngine;

/// <summary>
/// Handles line-of-sight and proximity checks for the monster.
/// Decoupled from MonsterAI movement logic.
/// </summary>
public class MonsterDetection : MonoBehaviour
{
    [Tooltip("Maximum distance at which the monster can see the player.")]
    public float DetectionRange = 15f;

    [Tooltip("Field of view angle in degrees.")]
    public float FieldOfView = 110f;

    [Tooltip("Radius within which the monster always detects the player regardless of LOS.")]
    public float ProximityRadius = 2.5f;

    private const string PlayerTag = "Player";
    private Transform playerTransform;

    private void Start()
    {
        GameObject playerObj = GameObject.FindWithTag(PlayerTag);
        if (playerObj != null)
            playerTransform = playerObj.transform;
    }

    /// <summary>
    /// Returns true if the player is within the monster's field of view and line of sight.
    /// </summary>
    public bool CanSeePlayer()
    {
        if (playerTransform == null) return false;

        Vector3 directionToPlayer = playerTransform.position - transform.position;
        float distanceToPlayer = directionToPlayer.magnitude;

        if (distanceToPlayer > DetectionRange) return false;

        // FOV angle check
        float angle = Vector3.Angle(transform.forward, directionToPlayer);
        if (angle > FieldOfView * 0.5f) return false;

        // Line of sight raycast — ignore the monster's own colliders
        int layerMask = ~LayerMask.GetMask("Monster");
        if (Physics.Raycast(transform.position + Vector3.up * 0.5f, directionToPlayer.normalized, out RaycastHit hit, DetectionRange, layerMask))
        {
            if (hit.transform.CompareTag(PlayerTag))
                return true;
        }

        return false;
    }

    /// <summary>
    /// Returns true if the player is within the proximity radius (always detects, no LOS required).
    /// </summary>
    public bool IsNearPlayer()
    {
        if (playerTransform == null) return false;
        return Vector3.Distance(transform.position, playerTransform.position) < ProximityRadius;
    }

    /// <summary>
    /// Scales detection range slightly as the monster becomes more aggressive.
    /// </summary>
    public void ScaleDetectionRange(int notesCollected)
    {
        DetectionRange = 15f + notesCollected * 1.5f;
    }
}
