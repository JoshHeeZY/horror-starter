using UnityEngine;

/// <summary>
/// Attach to any invisible trigger BoxCollider placed at a stairwell landing.
/// When the Player capsule enters this trigger, the FloorManager updates its current floor.
/// </summary>
[RequireComponent(typeof(BoxCollider))]
public class FloorTriggerZone : MonoBehaviour
{
    [Tooltip("-1 = Basement, 0 = Floor 1, 1 = Floor 2, 2 = Floor 3, 3 = Floor 4")]
    [SerializeField] private int floorNumber;

    private void Awake()
    {
        GetComponent<BoxCollider>().isTrigger = true;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
            FloorManager.Instance?.SetFloor(floorNumber);
    }
}
