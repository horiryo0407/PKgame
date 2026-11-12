
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

    [Header("シーン遷移")]
    [SerializeField] private SceneTransition sceneTransition;

    [Header("フリーズ演出")]
    [SerializeField] private BlackoutFreezeController blackoutFreezeController;

    [Header("スローモーション")]
    [SerializeField] private GameSlowMotion gameSlowMotion;

    private bool hasShot;
    private bool aimInputLocked;
    private Vector2 aimOffset;

    private const string AimXKey = "ShotTarget_X";
    private const string AimYKey = "ShotTarget_Y";

    void Start()
    {
        if (aimPoint == null) return;

        if (aimCenter == null)
        {
            Debug.LogError("Aim Centerが設定されていません。");
            return;
        }

        Vector3 localPos =
            aimCenter.InverseTransformPoint(aimPoint.position);

        aimOffset = new Vector2(localPos.x, localPos.y);

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
        if (hasShot || aimInputLocked ||
            aimPoint == null || aimCenter == null)
            return;

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

        if (saveAimWithEnter &&
            Keyboard.current != null &&
            Keyboard.current.enterKey.wasPressedThisFrame)
        {
            SaveAimPosition();
        }

        if (!allowShooting) return;

        bool shootPressed =
            (Gamepad.current != null &&
             Gamepad.current.buttonSouth.wasPressedThisFrame)
            ||
            (Keyboard.current != null &&
             Keyboard.current.spaceKey.wasPressedThisFrame);

        if (shootPressed)
        {
            BeginShoot();
        }
    }

    void UpdateAimPosition()
    {
        Vector3 localPosition =
            new Vector3(aimOffset.x, aimOffset.y, 0f);

        aimPoint.position =
            aimCenter.TransformPoint(localPosition);
    }

    public void SaveAimPosition()
    {
        PlayerPrefs.SetFloat(AimXKey, aimOffset.x);
        PlayerPrefs.SetFloat(AimYKey, aimOffset.y);

        PlayerPrefs.Save();

        Debug.Log(
            "シュート位置を保存しました！ X：" +
            aimOffset.x + " Y：" + aimOffset.y);
    }

    private void BeginShoot()
    {
        if (hasShot || aimInputLocked)
            return;

        aimInputLocked = true;

        if (blackoutFreezeController != null)
        {
            blackoutFreezeController.TryStartFreeze(Shoot);
        }
        else
        {
            Debug.LogWarning(
                "BlackoutFreezeControllerが設定されていません。通常シュートします。");

            Shoot();
        }
    }

    public void Shoot()
    {
        if (!allowShooting ||
            hasShot ||
            ballRigidbody == null ||
            aimPoint == null)
        {
            aimInputLocked = false;
            return;
        }

        Vector3 direction =
            aimPoint.position - ballRigidbody.position;

        if (direction.sqrMagnitude < 0.0001f)
        {
            aimInputLocked = false;
            return;
        }

        direction.Normalize();

        hasShot = true;

        ballRigidbody.useGravity = false;
        ballRigidbody.isKinematic = false;

        ballRigidbody.collisionDetectionMode =
            CollisionDetectionMode.ContinuousDynamic;

        ballRigidbody.linearVelocity =
            direction * shootSpeed;

        // シュート開始時にスローモーションを発動
        if (gameSlowMotion != null)
        {
            gameSlowMotion.StartSlowMotion();
        }
        else
        {
            Debug.LogWarning(
                "GameSlowMotionが設定されていません。");
        }

        if (sceneTransition != null)
        {
            sceneTransition.StartTransitionTimer();
        }
        else
        {
            Debug.LogError(
                "Scene Transitionが設定されていません。");
        }
    }
}
