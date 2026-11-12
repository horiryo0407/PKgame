
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

    [Header("リング照準")]
    [SerializeField] private float ringRadius = 0.2f;
    [SerializeField] private float ringWidth = 0.015f;
    [SerializeField] private int ringSegments = 96;
    [SerializeField] private Color ringColor = Color.cyan;

    [Header("リング位置")]
    [SerializeField] private float ringForwardOffset = 0.3f;

    [Header("リングを見るカメラ")]
    [SerializeField] private Camera aimCamera;

    [Header("Sceneごとの動作設定")]
    [SerializeField] private bool allowAimControl = true;
    [SerializeField] private bool allowShooting = true;
    [SerializeField] private bool loadSavedAimOnStart = false;
    [SerializeField] private bool saveAimWithEnter = false;

    [Header("シーン遷移")]
    [SerializeField] private SceneTransition sceneTransition;

    [Header("フリーズ演出")]
    [SerializeField] private BlackoutFreezeController blackoutFreezeController;

    private bool hasShot;
    private bool aimInputLocked;
    private Vector2 aimOffset;

    private LineRenderer ringRenderer;
    private Transform ringObject;

    private const string AimXKey = "ShotTarget_X";
    private const string AimYKey = "ShotTarget_Y";

    void Start()
    {
        if (aimPoint == null)
        {
            Debug.LogError("Aim Pointが設定されていません。");
            return;
        }

        if (aimCenter == null)
        {
            Debug.LogError("Aim Centerが設定されていません。");
            return;
        }

        // カメラを取得
        if (aimCamera == null)
        {
            aimCamera = Camera.main;
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

        CreateAimRing();
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

    // リングの向きを毎フレームカメラに合わせる
    void LateUpdate()
    {
        if (ringObject == null)
            return;

        if (aimCamera == null)
        {
            aimCamera = Camera.main;
        }

        if (aimCamera != null)
        {
            ringObject.rotation = aimCamera.transform.rotation;
        }
    }

    // AimPointとリングの位置を更新
    void UpdateAimPosition()
    {
        Vector3 localPosition =
            new Vector3(
                aimOffset.x,
                aimOffset.y,
                ringForwardOffset
            );

        // リングの中心となる目標座標
        aimPoint.position =
            aimCenter.TransformPoint(localPosition);

        // リングをAimPointと同じ位置にする
        if (ringObject != null)
        {
            ringObject.position = aimPoint.position;
        }
    }

    // リングをコードで生成
    void CreateAimRing()
    {
        GameObject ring = new GameObject("AimRing");

        ringObject = ring.transform;

        // 親オブジェクトにはしない
        ringObject.position = aimPoint.position;
        ringObject.rotation = Quaternion.identity;
        ringObject.localScale = Vector3.one;

        ringRenderer = ring.AddComponent<LineRenderer>();

        ringRenderer.useWorldSpace = false;
        ringRenderer.loop = true;
        ringRenderer.positionCount = ringSegments;

        ringRenderer.startWidth = ringWidth;
        ringRenderer.endWidth = ringWidth;

        ringRenderer.startColor = ringColor;
        ringRenderer.endColor = ringColor;

        ringRenderer.numCornerVertices = 4;
        ringRenderer.numCapVertices = 4;

        // マテリアルを作成
        Shader shader =
            Shader.Find("Universal Render Pipeline/Unlit");

        if (shader == null)
        {
            shader = Shader.Find("Sprites/Default");
        }

        if (shader != null)
        {
            Material material = new Material(shader);
            material.color = ringColor;
            ringRenderer.material = material;
        }
        else
        {
            Debug.LogError("リング用Shaderが見つかりません。");
        }

        // 正円の座標を生成
        for (int i = 0; i < ringSegments; i++)
        {
            float angle =
                2f * Mathf.PI * i / ringSegments;

            float x = Mathf.Cos(angle) * ringRadius;
            float y = Mathf.Sin(angle) * ringRadius;

            ringRenderer.SetPosition(
                i,
                new Vector3(x, y, 0f)
            );
        }

        // 最初からカメラに正対させる
        if (aimCamera != null)
        {
            ringObject.rotation = aimCamera.transform.rotation;
        }

        Debug.Log("正円のエイムリングを生成しました！");
    }

    // エイム位置を保存
    public void SaveAimPosition()
    {
        PlayerPrefs.SetFloat(AimXKey, aimOffset.x);
        PlayerPrefs.SetFloat(AimYKey, aimOffset.y);

        PlayerPrefs.Save();

        Debug.Log(
            "シュート位置を保存しました！ X：" +
            aimOffset.x + " Y：" + aimOffset.y);
    }

    // シュート開始
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

    // 実際にボールを飛ばす
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

        // リングの中心に向かう方向を計算
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
