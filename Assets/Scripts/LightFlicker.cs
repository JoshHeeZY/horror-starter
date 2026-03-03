using UnityEngine;

/// <summary>
/// Simulates a broken fluorescent tube by randomly spiking and recovering the light's intensity.
/// Attach to any Point or Spot Light that should flicker atmospherically.
/// </summary>
[RequireComponent(typeof(Light))]
public class LightFlicker : MonoBehaviour
{
    [Header("Intensity Range")]
    [SerializeField] private float normalIntensity  = 0.2f;
    [SerializeField] private float minIntensity     = 0.0f;
    [SerializeField] private float maxIntensity     = 0.25f;

    [Header("Behaviour")]
    [Tooltip("Probability per frame that a flicker spike occurs.")]
    [SerializeField][Range(0f, 1f)] private float flickerProbability = 0.04f;
    [Tooltip("How fast the intensity lerps toward the target each frame.")]
    [SerializeField] private float smoothSpeed = 18f;

    private Light flickerLight;
    private float targetIntensity;

    private void Awake()
    {
        flickerLight      = GetComponent<Light>();
        targetIntensity   = normalIntensity;
        flickerLight.intensity = normalIntensity;
    }

    private void Update()
    {
        targetIntensity = Random.value < flickerProbability
            ? Random.Range(minIntensity, maxIntensity)
            : normalIntensity;

        flickerLight.intensity = Mathf.Lerp(
            flickerLight.intensity,
            targetIntensity,
            smoothSpeed * Time.deltaTime);
    }
}
