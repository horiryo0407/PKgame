
using UnityEngine;
using System.Collections;

public class GKSlowDive : MonoBehaviour
{
    [Header("対象")]
    public Rigidbody ballBody;
    public Transform goalkeeperRoot;
    public Transform goalPlane;

    [Header("GKの反応ディレイ")]
    public float reactionDelay = 0.4f;

    [Header("GKの飛び込み")]
    public float diveDuration = 1.2f;
    public float maxHorizontalMove = 1.5f;
    public float maxVerticalMove = 0.8f;

    [Range(0f, 1f)]
    public float verticalMultiplier = 0.5f;

    [Header("シュート判定")]
    public float minimumKickSpeed = 1f;

    private Vector3 startPosition;
    private Vector3 targetPosition;

    private bool diveStarted;
    private bool targetCalculated;

    private float diveTimer;

    void Start()
    {
        if (goalkeeperRoot == null)
        {
            goalkeeperRoot = transform;
        }

        if (ballBody == null || goalPlane == null)
        {
            Debug.LogWarning(
                "GKSlowDive：Ball BodyとGoal Planeを設定してください。");
        }
    }

    void Update()
    {
        if (diveStarted || ballBody == null || goalPlane == null)
        {
            return;
        }

        // ボールがシュートされたか確認
        if (ballBody.linearVelocity.magnitude >= minimumKickSpeed)
        {
            startPosition = goalkeeperRoot.position;

            CalculateTarget();

            if (targetCalculated)
            {
                diveStarted = true;
                StartCoroutine(BeginDiveAfterDelay());
            }
        }
    }

    IEnumerator BeginDiveAfterDelay()
    {
        Debug.Log("GK：シュートを検知！反応待機中");

        // GKだけ反応を遅らせる
        yield return new WaitForSeconds(reactionDelay);

        Debug.Log("GK：飛び込み開始！");

        diveTimer = 0f;

        while (diveTimer < diveDuration)
        {
            diveTimer += Time.deltaTime;

            float t = Mathf.Clamp01(diveTimer / diveDuration);

            // なめらかな移動
            t = t * t * (3f - 2f * t);

            goalkeeperRoot.position =
                Vector3.Lerp(startPosition, targetPosition, t);

            yield return null;
        }

        goalkeeperRoot.position = targetPosition;
    }

    void CalculateTarget()
    {
        Vector3 ballPosition = ballBody.position;
        Vector3 ballVelocity = ballBody.linearVelocity;

        Vector3 normal = goalPlane.forward;

        float denominator = Vector3.Dot(ballVelocity, normal);

        targetPosition = startPosition;

        if (Mathf.Abs(denominator) <= 0.001f)
        {
            Debug.LogWarning("GKSlowDive：ボールの進行方向を計算できません。");
            return;
        }

        // ゴール面に到達する時間
        float timeToPlane =
            Vector3.Dot(
                goalPlane.position - ballPosition,
                normal
            ) / denominator;

        if (timeToPlane <= 0f)
        {
            Debug.LogWarning("GKSlowDive：ゴール面への到達位置を計算できません。");
            return;
        }

        // ボールの到達予測位置
        Vector3 predictedPosition =
            ballPosition + ballVelocity * timeToPlane;

        Vector3 difference =
            predictedPosition - startPosition;

        // 左右の移動
        float horizontal = Mathf.Clamp(
            Vector3.Dot(difference, goalPlane.right),
            -maxHorizontalMove,
            maxHorizontalMove
        );

        // 上下の移動
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

        targetCalculated = true;
    }
}
