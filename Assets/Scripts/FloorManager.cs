using System;
using UnityEngine;

/// <summary>
/// Singleton that tracks which floor the player is on via trigger colliders placed at stairwell landings.
/// Floor numbering: -1 = Basement, 0 = Ground (Floor 1), 1 = Floor 2, 2 = Floor 3, 3 = Floor 4.
/// </summary>
public class FloorManager : MonoBehaviour
{
    public static FloorManager Instance { get; private set; }

    /// <summary>Current floor the player is on. -1 = Basement, 0–3 = Floors 1–4.</summary>
    public int CurrentFloor { get; private set; } = 0;

    /// <summary>Fired when the player crosses into a new floor trigger.</summary>
    public event Action<int> OnFloorChanged;

    private void Awake()
    {
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    /// <summary>
    /// Reports the player's current floor. Called by <see cref="FloorTriggerZone"/> components
    /// placed at each stairwell landing. -1 = Basement, 0 = Floor 1, 1 = Floor 2, etc.
    /// </summary>
    public void SetFloor(int floor)
    {
        if (floor == CurrentFloor) return;
        CurrentFloor = floor;
        OnFloorChanged?.Invoke(CurrentFloor);
    }
}
