using UnityEngine;
using UnityEngine.InputSystem;

public class GKSlowDive : MonoBehaviour
{
    [Header("対象")]
    public Rigidbody ballBody;
    public Transform goalkeeperRoot;
    public Transform goalPlane;

    [Header("GKの飛び込み")]
    public float diveDuration = 1.2f;
    public float maxHorizontalMove = 1.5f;
    public float maxVerticalMove = 0.8f;
    [Range(0f, 1f)]
    public float verticalMultiplier = 0.5f;

    [Header("スローモーション")]
    [Range(0.1f, 1f)]
    public float slowMotionScale = 0.4f;
    public float slowMotionDuration = 2.5f;
    public float minimumKickSpeed = 1f;

    private Vector3 startPosition;
    private Vector3 targetPosition;

    private bool sequenceStarted;
    private bool targetCalculated;
    private bool slowMotionActive;

    private float diveTimer;
    private float slowTimer;
    private float previousTimeScale = 1f;

    void Start()
    {
        if (goalkeeperRoot == null)
        {
            goalkeeperRoot = transform;
        }
    }

    void Update()
    {
        // スペースキーで開始
        if (!sequenceStarted &&
            Keyboard.current != null &&
            Keyboard.current.spaceKey.wasPressedThisFrame)
        {
            BeginSequence();
        }

        if (!sequenceStarted)
        {
            return;
        }

        // スローモーションの時間を計測
        if (slowMotionActive)
        {
            slowTimer += Time.unscaledDeltaTime;

            if (slowTimer >= slowMotionDuration)
            {
                Time.timeScale = previousTimeScale;
                slowMotionActive = false;
            }
        }

        // ボールが動き始めたら飛び込み先を計算
        if (!targetCalculated &&
            ballBody != null &&
            goalPlane != null &&
            ballBody.linearVelocity.magnitude >= minimumKickSpeed)
        {
            CalculateTarget();
        }

        // GK全体をゆっくり移動
        if (targetCalculated && diveTimer < diveDuration)
        {
            diveTimer += Time.unscaledDeltaTime;

            float t = Mathf.Clamp01(diveTimer / diveDuration);

            // 最初と最後がなめらかになる動き
            t = t * t * (3f - 2f * t);

            goalkeeperRoot.position =
                Vector3.Lerp(startPosition, targetPosition, t);
        }
    }

    void BeginSequence()
    {
        if (ballBody == null ||
            goalkeeperRoot == null ||
            goalPlane == null)
        {
            Debug.LogWarning(
                "GKSlowDive：Ball Body、Goalkeeper Root、Goal Planeを設定してください。");
            return;
        }

        sequenceStarted = true;
        targetCalculated = false;

        startPosition = goalkeeperRoot.position;
        targetPosition = startPosition;

        diveTimer = 0f;
        slowTimer = 0f;

        // 現在のゲーム速度を保存してスローにする
        previousTimeScale = Time.timeScale;
        Time.timeScale = slowMotionScale;
        slowMotionActive = true;
    }

    void CalculateTarget()
    {
        Vector3 ballPosition = ballBody.position;
        Vector3 ballVelocity = ballBody.linearVelocity;

        // ゴール面の向き
        Vector3 normal = goalPlane.forward;

        float denominator = Vector3.Dot(ballVelocity, normal);

        targetPosition = startPosition;

        if (Mathf.Abs(denominator) > 0.001f)
        {
            // ボールがゴール面に到達するまでの時間
            float timeToPlane =
                Vector3.Dot(
                    goalPlane.position - ballPosition,
                    normal
                ) / denominator;

            if (timeToPlane > 0f)
            {
                // ゴール面に到達する予測位置
                Vector3 predictedPosition =
                    ballPosition + ballVelocity * timeToPlane;

                Vector3 difference =
                    predictedPosition - startPosition;

                // 左右の移動量
                float horizontal = Mathf.Clamp(
                    Vector3.Dot(difference, goalPlane.right),
                    -maxHorizontalMove,
                    maxHorizontalMove
                );

                // 上下の移動量
                float vertical = Mathf.Clamp(
                    Vector3.Dot(difference, goalPlane.up)
                        * verticalMultiplier,
                    -maxVerticalMove,
                    maxVerticalMove
                );

                targetPosition =
                    startPosition
                    + goalPlane.right * horizontal
                    + goalPlane.up * vertical;
            }
        }

        targetCalculated = true;
    }

    void OnDisable()
    {
        // スクリプトが無効になったときも速度を戻す
        if (slowMotionActive)
        {
            Time.timeScale = previousTimeScale;
            slowMotionActive = false;
        }
    }
}