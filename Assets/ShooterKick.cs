using UnityEngine;
using UnityEngine.InputSystem;

public class ShooterKick : MonoBehaviour
{
    [SerializeField] private Animator animator;
    [SerializeField] private ShooterCameraRig cameraRig;

    private bool hasKicked = false;

    void Reset()
    {
        animator = GetComponentInChildren<Animator>();
    }

    void OnEnable()
    {
        hasKicked = false;
    }

    void Update()
    {
        if (hasKicked) return;

        bool gamepadKick =
            Gamepad.current != null &&
            Gamepad.current.buttonSouth.wasPressedThisFrame;

        bool keyboardKick =
            Keyboard.current != null &&
            Keyboard.current.spaceKey.wasPressedThisFrame;

        if (gamepadKick || keyboardKick)
        {
            hasKicked = true;

            if (animator != null)
            {
                animator.SetTrigger("Kick");
            }
            if (cameraRig != null)
            {
                cameraRig.StartKick();
            }
        }
    }


    public void ResetKick()
    {
        hasKicked = false;
        if (animator != null)
        {
            animator.ResetTrigger("Kick");
        }
    }
}