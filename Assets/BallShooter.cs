
using UnityEngine;
using UnityEngine.InputSystem;

public class BallShooter : MonoBehaviour
{
    [Header("シュート設定")]
    [SerializeField] private Rigidbody ballRigidbody;
    [SerializeField] private Transform aimPoint;
    [SerializeField] private float shootSpeed = 8f;

    private bool hasShot;

    void Update()
    {
        if (hasShot)
            return;

        if ((Gamepad.current != null &&
     Gamepad.current.buttonSouth.wasPressedThisFrame) ||
    Keyboard.current.spaceKey.wasPressedThisFrame)
        {
            Shoot();
        }
    }

    public void Shoot()
    {
        if (hasShot || ballRigidbody == null || aimPoint == null)
            return;

        hasShot = true;

        Vector3 direction =
            (aimPoint.position - ballRigidbody.position).normalized;

        ballRigidbody.useGravity = false;
        ballRigidbody.isKinematic = false;
        ballRigidbody.collisionDetectionMode =
            CollisionDetectionMode.ContinuousDynamic;

        ballRigidbody.linearVelocity = direction * shootSpeed;
    }
}
