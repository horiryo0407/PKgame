
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

    [Header("Sceneごとの動作設定")]
    [SerializeField] private bool allowAimControl = true;
    [SerializeField] private bool allowShooting = true;
    [SerializeField] private bool loadSavedAimOnStart = false;
    [SerializeField] private bool saveAimWithEnter = false;

    private bool hasShot;

    private Vector2 aimOffset;

    private const string AimXKey = "ShotTarget_X";
    private const string AimYKey = "ShotTarget_Y";

    void Start()
    {
        if (aimPoint == null)
            return;

        if (aimCenter == null)
        {
            Debug.LogError("Aim Centerが設定されていません。");
            return;
        }

        // AimCenterを基準に初期位置を取得
        Vector3 localPos =
            aimCenter.InverseTransformPoint(aimPoint.position);

        aimOffset = new Vector2(localPos.x, localPos.y);

        // 保存データがあれば読み込む
        if (loadSavedAimOnStart)
        {
            if (PlayerPrefs.HasKey(AimXKey) &&
                PlayerPrefs.HasKey(AimYKey))
            {
                aimOffset.x = PlayerPrefs.GetFloat(AimXKey);
                aimOffset.y = PlayerPrefs.GetFloat(AimYKey);

                Debug.Log("保存したシュート位置を読み込みました。");
            }
            else
            {
                Debug.LogWarning("シュート位置の保存データがありません。");
            }
        }

        UpdateAimPosition();
    }

    void Update()
    {
        if (hasShot || aimPoint == null || aimCenter == null)
            return;

        // 照準移動
        if (allowAimControl)
        {
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

            input = Vector2.ClampMagnitude(input, 1f);

            aimOffset += input * aimMoveSpeed * Time.deltaTime;

            aimOffset.x = Mathf.Clamp(
                aimOffset.x, -aimWidth, aimWidth);

            aimOffset.y = Mathf.Clamp(
                aimOffset.y, -aimHeight, aimHeight);

            UpdateAimPosition();
        }

        // Enterキーで保存（テスト用）
        if (saveAimWithEnter &&
            Keyboard.current != null &&
            Keyboard.current.enterKey.wasPressedThisFrame)
        {
            SaveAimPosition();
        }

        // シュートを許可していない場合は終了
        if (!allowShooting)
            return;

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

    // 照準位置を保存する
    public void SaveAimPosition()
    {
        PlayerPrefs.SetFloat(AimXKey, aimOffset.x);
        PlayerPrefs.SetFloat(AimYKey, aimOffset.y);

        PlayerPrefs.Save();

        Debug.Log(
            "シュート位置を保存しました！ X：" +
            aimOffset.x + " Y：" + aimOffset.y
        );
    }

    public void Shoot()
    {
        if (!allowShooting || hasShot ||
            ballRigidbody == null || aimPoint == null)
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
