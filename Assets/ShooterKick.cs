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
        // テスト用：スペースキーでキック
        if (Keyboard.current != null && Keyboard.current.spaceKey.wasPressedThisFrame)
        {
            animator.SetTrigger("Kick");
        }
    }
}