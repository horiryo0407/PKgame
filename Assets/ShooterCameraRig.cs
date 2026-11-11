using UnityEngine;

public class ShooterCameraRig : MonoBehaviour
{
    public enum Phase { Aim, Kick, Follow }

    [Header("位置の目印")]
    [SerializeField] private Transform aimPose;   // 斜め後ろ
    [SerializeField] private Transform kickPose;  // シューターの正面

    [Header("ボール")]
    [SerializeField] private Transform ball;
    [SerializeField] private Transform lookTarget; // 追従先（ゴールなど）

    [Header("追従の設定")]
    [SerializeField] private float followDistance = 0.1f; // ボールがこれだけ進んだら追従する
    [SerializeField] private float followDelay = 1f;    // キックから何秒で追従に切り替えるか
    [SerializeField] private float followBack = 2f;       // ボールの何m後ろに付くか
    [SerializeField] private float followHeight = 0.5f;   // ボールより何m上か
    private Vector3 lastDir = Vector3.forward;

    [Header("切り替えの滑らかさ")]
    [SerializeField] private float smooth = 8f;

    public Phase Current { get; private set; } = Phase.Aim;
    private Vector3 ballStartPos;
    private float kickStartTime;

    void Start() { StartAim(); }

    // シュート位置を決め始めるときに呼ぶ
    public void StartAim()
    {
        Current = Phase.Aim;
    }

    // シュートを決定したときに呼ぶ
    public void StartKick()
    {
        ballStartPos = ball.position;
        kickStartTime = Time.unscaledTime;

        if (lookTarget != null)
        {
            Vector3 toGoal = lookTarget.position - ball.position;
            toGoal.y = 0f;
            if (toGoal.sqrMagnitude > 0.0001f) lastDir = toGoal.normalized;
        }

        Current = Phase.Kick;
    }

    void LateUpdate()
    {
        Vector3 targetPos;
        Quaternion targetRot;

        switch (Current)
        {
            case Phase.Aim:
                targetPos = aimPose.position;
                targetRot = aimPose.rotation;
                break;

            case Phase.Kick:
                targetPos = kickPose.position;
                targetRot = kickPose.rotation;

              
                bool byDistance = Vector3.Distance(ball.position, ballStartPos) >= followDistance;
                bool byTime = followDelay > 0f && Time.unscaledTime - kickStartTime >= followDelay;
                if (byDistance || byTime)
                {
                    Current = Phase.Follow;
                }
                break;

            default:
                Vector3 travel = ball.position - ballStartPos;
                travel.y = 0f;
                if (travel.sqrMagnitude > 0.0001f)
                {
                    lastDir = travel.normalized;
                }
                targetPos = ball.position - lastDir * followBack + Vector3.up * followHeight;
                Transform look = lookTarget != null ? lookTarget : ball;

                Vector3 dir = look.position - targetPos;
                targetRot = dir.sqrMagnitude > 0.0001f
                    ? Quaternion.LookRotation(dir)
                    : transform.rotation;
                break;
        }

        if (smooth <= 0f)
        {
            transform.SetPositionAndRotation(targetPos, targetRot);
        }
        else
        {
            float t = 1f - Mathf.Exp(-smooth * Time.unscaledDeltaTime);
            transform.position = Vector3.Lerp(transform.position, targetPos, t);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, t);
        }
    }

    [ContextMenu("Test Aim")]
    private void TestAim() { StartAim(); }

    [ContextMenu("Test Kick")]
    private void TestKick() { StartKick(); }
}