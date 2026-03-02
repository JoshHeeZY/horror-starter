using System.Collections;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Plays the jumpscare sequence (monster face flash + loud audio) before
/// transitioning to the Game Over state.
/// </summary>
public class JumpscareController : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private GameObject blackOverlayPanel;
    [SerializeField] private Image monsterFaceImage;

    private const float FadeInDuration = 0.3f;
    private const float HoldDuration = 1.5f;

    /// <summary>
    /// Coroutine that plays the full jumpscare sequence and then triggers Game Over.
    /// </summary>
    public IEnumerator PlayJumpscare()
    {
        // Show black overlay immediately
        blackOverlayPanel?.SetActive(true);

        // Fade in monster face
        if (monsterFaceImage != null)
        {
            monsterFaceImage.gameObject.SetActive(true);
            Color c = monsterFaceImage.color;
            c.a = 0f;
            monsterFaceImage.color = c;

            float elapsed = 0f;
            while (elapsed < FadeInDuration)
            {
                elapsed += Time.deltaTime;
                c.a = Mathf.Clamp01(elapsed / FadeInDuration);
                monsterFaceImage.color = c;
                yield return null;
            }
        }

        AudioManager.Instance?.PlayJumpscare();

        yield return new WaitForSeconds(HoldDuration);

        GameManager.Instance?.ChangeState(GameManager.GameState.GameOver);
    }
}
