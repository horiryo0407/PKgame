
using UnityEngine;
using UnityEngine.InputSystem;

public class BallShooter : MonoBehaviour
{
    [Header("シュート設定")]
    [SerializeField] private Rigidbody ballRigidbody;
    [SerializeField] private Transform aimPoint;
    [SerializeField] private float shootSpeed = 8f;

    [Header("照準設定")]
    [SerializeField] private Transform aimCenter;
    [SerializeField] private float aimMoveSpeed = 3f;
    [SerializeField] private float aimWidth = 3f;
    [SerializeField] private float aimHeight = 2f;

    private bool hasShot;

    // 照準の初期位置を基準にした左右・上下の移動量
    private Vector2 aimOffset;

    void Start()
    {
        if (aimPoint == null)
            return;

        if (aimCenter == null)
        {
            Debug.LogError("Aim Centerが設定されていません。");
            return;
        }

        // AimCenterのローカル座標で初期位置を取得
        Vector3 localPos =
            aimCenter.InverseTransformPoint(aimPoint.position);

        aimOffset = new Vector2(localPos.x, localPos.y);

        UpdateAimPosition();
    }

    void Update()
    {
        if (hasShot || aimPoint == null || aimCenter == null)
            return;

        // ゲームパッド・キーボードの照準入力
        Vector2 input = Vector2.zero;

        if (Gamepad.current != null)
        {
            input = Gamepad.current.leftStick.ReadValue();
        }

        if (Keyboard.current != null)
        {
            if (Keyboard.current.leftArrowKey.isPressed)
                input.x -= 1f;

            if (Keyboard.current.rightArrowKey.isPressed)
                input.x += 1f;

            if (Keyboard.current.downArrowKey.isPressed)
                input.y -= 1f;

            if (Keyboard.current.upArrowKey.isPressed)
                input.y += 1f;
        }

        // 斜め移動が速くなりすぎないようにする
        input = Vector2.ClampMagnitude(input, 1f);

        aimOffset += input * aimMoveSpeed * Time.deltaTime;

        // 照準をゴールの範囲内に制限
        aimOffset.x = Mathf.Clamp(
            aimOffset.x, -aimWidth, aimWidth);

        aimOffset.y = Mathf.Clamp(
            aimOffset.y, -aimHeight, aimHeight);

        UpdateAimPosition();

        // シュート入力
        bool shootPressed =
            (Gamepad.current != null &&
             Gamepad.current.buttonSouth.wasPressedThisFrame)
            ||
            (Keyboard.current != null &&
             Keyboard.current.spaceKey.wasPressedThisFrame);

        if (shootPressed)
        {
            Shoot();
        }
    }

    void UpdateAimPosition()
    {
        Vector3 localPosition = new Vector3(
            aimOffset.x,
            aimOffset.y,
            0f
        );

        aimPoint.position =
            aimCenter.TransformPoint(localPosition);
    }

    public void Shoot()
    {
        if (hasShot || ballRigidbody == null ||
            aimPoint == null)
            return;

        Vector3 direction =
            aimPoint.position - ballRigidbody.position;

        if (direction.sqrMagnitude < 0.0001f)
            return;

        direction.Normalize();
        hasShot = true;

        ballRigidbody.useGravity = false;
        ballRigidbody.isKinematic = false;

        ballRigidbody.collisionDetectionMode =
            CollisionDetectionMode.ContinuousDynamic;

        ballRigidbody.linearVelocity =
            direction * shootSpeed;
    }
}
