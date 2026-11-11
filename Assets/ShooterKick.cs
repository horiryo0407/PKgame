
using UnityEngine;
using UnityEngine.InputSystem;

public class ShooterKick : MonoBehaviour
{
    [SerializeField] private Animator animator;

    void Reset()
    {
        animator = GetComponentInChildren<Animator>();
    }

    void Update()
    {
        // ゲームパッド：Aボタン / ×ボタン
        bool gamepadKick =
            Gamepad.current != null &&
            Gamepad.current.buttonSouth.wasPressedThisFrame;

        // キーボード：スペースキー（テスト用）
        bool keyboardKick =
            Keyboard.current != null &&
            Keyboard.current.spaceKey.wasPressedThisFrame;

        if (gamepadKick || keyboardKick)
        {
            if (animator != null)
            {
                animator.SetTrigger("Kick");
            }
        }
    }
}
