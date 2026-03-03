using UnityEngine;

/// <summary>
/// Drives the FPS arms Animator based on player movement state.
/// Should be a child of or sibling to Main Camera, with the arms mesh rendered on the Arms layer.
/// </summary>
[RequireComponent(typeof(Animator))]
public class FPSArmsController : MonoBehaviour
{
    private static readonly int IsMovingHash      = Animator.StringToHash("IsMoving");
    private static readonly int IsSprintingHash   = Animator.StringToHash("IsSprinting");
    private static readonly int FlashlightOnHash  = Animator.StringToHash("FlashlightOn");

    private Animator armsAnimator;

    private void Awake()
    {
        armsAnimator = GetComponent<Animator>();
    }

    /// <summary>Updates the IsMoving animator bool.</summary>
    public void SetMoving(bool isMoving)
    {
        armsAnimator?.SetBool(IsMovingHash, isMoving);
    }

    /// <summary>Updates the IsSprinting animator bool.</summary>
    public void SetSprinting(bool isSprinting)
    {
        armsAnimator?.SetBool(IsSprintingHash, isSprinting);
    }

    /// <summary>Updates the FlashlightOn animator bool.</summary>
    public void SetFlashlightOn(bool isOn)
    {
        armsAnimator?.SetBool(FlashlightOnHash, isOn);
    }
}
