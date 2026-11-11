using UnityEngine;

public class ShooterCameraRig : MonoBehaviour
{
    public enum Phase { Aim, Kick, Follow }

    [Header("位置の目印")]
    [SerializeField] private Transform aimPose;   // 斜め後ろ
    [SerializeField] private Transform kickPose;  // シューターの正面

    [Header("ボール")]
    [SerializeField] private Transform ball;
    [SerializeField] private Transform lookTarget; // 追従先

    [Header("追従の設定")]
    [SerializeField] private float followDistance = 1f; // ボールがこれだけ進んだら追従する
    [SerializeField] private Vector3 followOffset = new Vector3(0f, 0.5f, -2f);

    [Header("切り替えの滑らかさ")]
    [SerializeField] private float smooth = 8f;

    public Phase Current { get; private set; } = Phase.Aim;
    private Vector3 ballStartPos;

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
                if (Vector3.Distance(ball.position, ballStartPos) >= followDistance)
                {
                    Current = Phase.Follow;
                }
                break;

            default:
                targetPos = ball.position + followOffset;
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