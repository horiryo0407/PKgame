
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneTransition : MonoBehaviour
{
    [Header("カメラ")]
    [SerializeField] private ShooterCameraRig cameraRig;

    [Header("役割交代用カメラ")]
    [SerializeField] private Camera kkCamera;
    [SerializeField] private Camera gkCamera;

    [Header("キッカー側のUI表示")]
    [SerializeField] private Canvas kkSideCanvas;
    [Header("キーパー側のUI表示")]
    [SerializeField] private Canvas gkSideCanvas;

    [Header("シュート後の待ち時間")]
    [SerializeField] private float transitionDelay = 2f;

    [Header("ボールリセット後の待機時間")]
    [SerializeField] private float timerStartDelay = 0.5f;

    [Header("結果シーンへ移るまでの待ち時間")]
    [SerializeField] private float resultSceneDelay = 5f;

    // 0 = プレイヤー1
    // 1 = プレイヤー2
    private static int currentPlayer = 0;

    public static int CurrentPlayer => currentPlayer;

    private bool isTransitioning = false;

    // GKカメラをKKカメラに同期する状態
    private bool syncGKCameraToKK = false;

    // 同期前のGKカメラの位置・角度
    private Vector3 originalGKCameraPosition;
    private Quaternion originalGKCameraRotation;

    // 同期前のGKカメラのCulling Mask
    private int originalGKCameraMask;

    private bool originalCameraSaved = false;

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

        // カメラの表示先を設定
        ApplyCameraDisplays();

        if (cameraRig != null)
        {
            cameraRig.StartAim();
        }

        // 初回ゲーム開始時もGKタイマーを開始
        StartCoroutine(StartInitialGKTimer());
    }

    // GKカメラをKKカメラに追従させる
    private void LateUpdate()
    {
        if (!syncGKCameraToKK)
            return;

        if (kkCamera == null || gkCamera == null)
            return;

        // GKカメラの位置をKKカメラに合わせる
        gkCamera.transform.position =
            kkCamera.transform.position;

        // GKカメラの角度をKKカメラに合わせる
        gkCamera.transform.rotation =
            kkCamera.transform.rotation;
    }

    // カメラの表示先を切り替える
    private void ApplyCameraDisplays()
    {
        if (kkCamera == null || gkCamera == null)
        {
            Debug.LogError(
                "KKカメラまたはGKカメラが設定されていません！");
            return;
        }

        // プレイヤー1のターン
        if (currentPlayer == 0)
        {
            kkCamera.targetDisplay = 0;
            gkCamera.targetDisplay = 1;
        }
        // プレイヤー2のターン
        else
        {
            kkCamera.targetDisplay = 1;
            gkCamera.targetDisplay = 0;
        }


        if (kkSideCanvas != null)//UI入れ替え
        {
            kkSideCanvas.targetDisplay = kkCamera.targetDisplay;
        }

        if (gkSideCanvas != null)
        {
            gkSideCanvas.targetDisplay = gkCamera.targetDisplay;
        }


        Debug.Log(
            "KKカメラ Display " +
            (kkCamera.targetDisplay + 1) +
            " / GKカメラ Display " +
            (gkCamera.targetDisplay + 1)
        );
    }

    // 初回ゲーム開始時のタイマー処理
    private IEnumerator StartInitialGKTimer()
    {
        yield return new WaitForSecondsRealtime(
            timerStartDelay
        );

        StartGKTimer();
    }

    // 現在のターンに合わせて設定
    private void ApplyTurnSettings()
    {
        // プレイヤーの色を変更
        TurnSkinManager skin =
            FindAnyObjectByType<TurnSkinManager>();

        if (skin != null)
        {
            skin.ApplyTurn(currentPlayer + 1);
        }

        // GKPoseOnSpaceの適用状態をリセット
        GKPoseOnSpace poseOnSpace =
            FindAnyObjectByType<GKPoseOnSpace>();

        if (poseOnSpace != null)
        {
            poseOnSpace.ResetPoseApplied();

            Debug.Log(
                "GKPoseOnSpaceをリセットしました！"
            );
        }
        else
        {
            Debug.LogWarning(
                "GKPoseOnSpaceが見つかりません！"
            );
        }

        Debug.Log(
            "キッカー：プレイヤー" + (currentPlayer + 1) +
            " / GK操作：プレイヤー" + (2 - currentPlayer)
        );
    }

    // GKタイマーを開始
    private void StartGKTimer()
    {
        GKPositionTimer timer =
            FindAnyObjectByType<GKPositionTimer>();

        if (timer != null)
        {
            timer.StartTimer();

            Debug.Log("GKタイマーを開始しました！");
        }
        else
        {
            Debug.LogError(
                "GKPositionTimerが見つかりません！"
            );
        }
    }

    // シュートした瞬間に呼び出す
    public void StartTransitionTimer()
    {
        if (isTransitioning)
            return;

        isTransitioning = true;

        // 結果表示用のカメラ設定
        StartResultCamera();

        Debug.Log("シュートを検知しました！");

        StartCoroutine(TransitionAfterShot());
    }

    // 結果表示用のカメラに切り替える
    private void StartResultCamera()
    {
        if (kkCamera == null || gkCamera == null)
        {
            Debug.LogError(
                "KKカメラまたはGKカメラが設定されていません！"
            );
            return;
        }

        // GKカメラの元の設定を保存
        originalGKCameraPosition =
            gkCamera.transform.position;

        originalGKCameraRotation =
            gkCamera.transform.rotation;

        originalGKCameraMask =
            gkCamera.cullingMask;

        originalCameraSaved = true;

        // 結果確認中はKKカメラと同じ対象を映す
        gkCamera.cullingMask =
            kkCamera.cullingMask;

        // GKカメラをKKカメラに同期開始
        syncGKCameraToKK = true;

        Debug.Log(
            "結果表示開始！GKカメラをKKカメラと同じ描画設定にしました！"
        );
    }

    // GKカメラを通常状態に戻す
    private void RestoreGKCamera()
    {
        if (gkCamera == null || !originalCameraSaved)
            return;

        // カメラの同期を停止
        syncGKCameraToKK = false;

        // 元の位置に戻す
        gkCamera.transform.position =
            originalGKCameraPosition;

        // 元の角度に戻す
        gkCamera.transform.rotation =
            originalGKCameraRotation;

        // 元のCulling Maskに戻す
        gkCamera.cullingMask =
            originalGKCameraMask;

        originalCameraSaved = false;

        Debug.Log(
            "GKカメラを通常状態に戻しました！"
        );
    }

    // シュート後のターン交代処理
    private IEnumerator TransitionAfterShot()
    {
        Debug.Log(
            transitionDelay + "秒後にターンを交代します。"
        );

        // 結果確認のため待機
        yield return new WaitForSecondsRealtime(
            transitionDelay
        );

        if (ScoreManager.Instance == null)
        {
            Debug.LogError("ScoreManagerが見つかりません！");

            RestoreGKCamera();

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

            // 最終結果を少し見せてから、結果シーンへ
            yield return new WaitForSecondsRealtime(
                resultSceneDelay
            );

            SceneManager.LoadScene("ResultScene");

            isTransitioning = false;
            yield break;
        }

        // GKカメラを通常状態に戻す
        RestoreGKCamera();

        // 次のプレイヤーに交代
        currentPlayer = 1 - currentPlayer;

        Debug.Log(
            "次はプレイヤー" + (currentPlayer + 1) + "の番です！"
        );

        // スコア表示を更新
        ScoreManager.Instance.RefreshDisplay();

        // カメラ表示先を交代
        ApplyCameraDisplays();

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

        Debug.Log(
            "両プレイヤーの頭をリセットしました！"
        );

        // 次のターンの色・ポーズ適用状態を設定
        ApplyTurnSettings();

        // ボールなどのリセットが終わってから待つ
        yield return new WaitForSecondsRealtime(
            timerStartDelay
        );

        // 次のGKタイマーを開始
        StartGKTimer();

        isTransitioning = false;

        Debug.Log("ターン交代完了！");
    }
}
