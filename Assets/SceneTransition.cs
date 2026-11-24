using System.Collections;
using UnityEngine;

public class SceneTransition : MonoBehaviour
{
    [Header("カメラ")]
    [SerializeField] private ShooterCameraRig cameraRig;

    [Header("シュート後の待ち時間")]
    [SerializeField] private float transitionDelay = 5f;

    // 0 = プレイヤー1
    // 1 = プレイヤー2
    private static int currentPlayer = 0;

    public static int CurrentPlayer => currentPlayer;

    private bool isTransitioning = false;

    [RuntimeInitializeOnLoadMethod(
        RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetStaticData()
    {
        currentPlayer = 0;
    }

    public static void ResetTurn()
    {
        currentPlayer = 0;
    }

    private void Start()
    {
        if (cameraRig == null)
        {
            cameraRig =
                FindAnyObjectByType<ShooterCameraRig>();
        }

        ApplyTurnSettings();

        if (cameraRig != null)
        {
            cameraRig.StartAim();
        }
    }

    // 現在のターンに合わせて設定
    private void ApplyTurnSettings()
    {
        TurnSkinManager skin =
            FindAnyObjectByType<TurnSkinManager>();

        if (skin != null)
        {
            skin.ApplyTurn(currentPlayer + 1);
        }

        GKPositionTimer timer =
            FindAnyObjectByType<GKPositionTimer>();

        if (timer != null)
        {
            timer.StartTimer();
        }
        else
        {
            Debug.LogError("GKPositionTimerが見つかりません！");
        }

        Debug.Log(
            "キッカー：プレイヤー" + (currentPlayer + 1) +
            " / GK操作：プレイヤー" + (2 - currentPlayer)
        );
    }

    // シュートした瞬間に呼び出す
    public void StartTransitionTimer()
    {
        if (isTransitioning)
            return;

        isTransitioning = true;

        Debug.Log("シュートを検知しました！");

        // ここではGKにポーズをコピーしない
        // GKSlowDiveの反応ディレイ終了後にポーズを反映する

        StartCoroutine(TransitionAfterShot());
    }

    private IEnumerator TransitionAfterShot()
    {
        Debug.Log(
            transitionDelay + "秒後にターンを交代します。"
        );

        yield return new WaitForSecondsRealtime(
            transitionDelay
        );

        if (ScoreManager.Instance == null)
        {
            Debug.LogError("ScoreManagerが見つかりません！");
            isTransitioning = false;
            yield break;
        }

        // 今回シュートしたプレイヤー
        int shotPlayer = currentPlayer;

        // スコアと投数を記録
        ScoreManager.Instance.CompleteShot(shotPlayer);

        // 10投終了なら試合終了
        if (ScoreManager.Instance.IsGameFinished)
        {
            Debug.Log("10投終了！試合終了です！");
            yield break;
        }

        // 次のプレイヤーに交代
        currentPlayer = 1 - currentPlayer;

        Debug.Log(
            "次はプレイヤー" + (currentPlayer + 1) + "の番です！"
        );

        // スコア表示を更新
        ScoreManager.Instance.RefreshDisplay();

        // ボールを初期位置に戻す
        BallShooter shooter =
            FindAnyObjectByType<BallShooter>();

        if (shooter != null)
        {
            shooter.ResetBall();
        }
        else
        {
            Debug.LogError("BallShooterが見つかりません！");
        }

        // キッカーのアニメーション状態をリセット
        ShooterKick shooterKick =
            FindAnyObjectByType<ShooterKick>();

        if (shooterKick != null)
        {
            shooterKick.ResetKick();

            Debug.Log(
                "キッカーのアニメーションをリセットしました！"
            );
        }
        else
        {
            Debug.LogError("ShooterKickが見つかりません！");
        }

        // カメラをAimに戻す
        if (cameraRig != null)
        {
            cameraRig.StartAim();
        }

        // 1P・2P両方の頭を元に戻す
        GKHeadFly[] heads =
            FindObjectsByType<GKHeadFly>(
                FindObjectsSortMode.None
            );

        foreach (GKHeadFly head in heads)
        {
            head.ResetHead();
        }

        Debug.Log("両プレイヤーの頭をリセットしました！");

        // 次のターンの設定
        ApplyTurnSettings();

        isTransitioning = false;

        Debug.Log("ターン交代完了！");
    }
}