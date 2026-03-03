using System.Collections;
using UnityEngine;

/// <summary>
/// Controls the main menu cinematic camera that slowly pans across the school exterior.
/// Attach to the Main Camera in MainMenu.unity.
/// Assign 2–3 panWaypoints positioned along the facade (left → right or center → right).
/// </summary>
public class ExteriorViewController : MonoBehaviour
{
    [Header("Pan Settings")]
    [SerializeField] private Transform[] panWaypoints;
    [SerializeField] private float panDuration = 14f;
    [SerializeField] private AnimationCurve panCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);

    [Header("Look Target")]
    [SerializeField] private Transform facadeLookTarget;

    private Coroutine panCoroutine;

    private void Start()
    {
        StartPan();
    }

    /// <summary>Begins the looping cinematic pan. Called automatically on Start.</summary>
    public void StartPan()
    {
        if (panWaypoints == null || panWaypoints.Length < 2)
        {
            Debug.LogWarning("[ExteriorViewController] At least 2 panWaypoints are required.");
            return;
        }

        if (panCoroutine != null)
            StopCoroutine(panCoroutine);

        panCoroutine = StartCoroutine(PanCoroutine());
    }

    /// <summary>Stops the pan. Call before loading the game scene.</summary>
    public void StopPan()
    {
        if (panCoroutine != null)
        {
            StopCoroutine(panCoroutine);
            panCoroutine = null;
        }
    }

    private IEnumerator PanCoroutine()
    {
        int waypointCount = panWaypoints.Length;

        while (true)
        {
            for (int i = 0; i < waypointCount - 1; i++)
            {
                Transform fromWaypoint = panWaypoints[i];
                Transform toWaypoint   = panWaypoints[i + 1];
                float elapsed = 0f;

                while (elapsed < panDuration)
                {
                    elapsed += Time.deltaTime;
                    float t = panCurve.Evaluate(Mathf.Clamp01(elapsed / panDuration));
                    transform.position = Vector3.Lerp(fromWaypoint.position, toWaypoint.position, t);

                    if (facadeLookTarget != null)
                        transform.LookAt(facadeLookTarget);

                    yield return null;
                }
            }

            // Reverse direction for a seamless back-and-forth loop
            System.Array.Reverse(panWaypoints);
        }
    }
}
