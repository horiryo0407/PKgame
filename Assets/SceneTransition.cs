
using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class SceneTransition : MonoBehaviour
{
    [Header("カメラ")]
    [SerializeField] private ShooterCameraRig cameraRig;

    [Header("遷移先")]
    [SerializeField] private string nextSceneName = "GKPoseSetting";

    [Header("シュート後の遷移時間")]
    [SerializeField] private float transitionDelay = 5f;

    // 0 = プレイヤー1
    // 1 = プレイヤー2
    private static int currentPlayer = 0;

    public static int CurrentPlayer => currentPlayer;

    private bool isTransitioning = false;

    // ゲーム開始時にプレイヤー1から始める
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetStaticData()
    {
        currentPlayer = 0;
    }

    // ターンをリセットする
    public static void ResetTurn()
    {
        currentPlayer = 0;
    }

    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    // シーン読み込み完了後の処理
    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // 新しいシーンのカメラを取得
        if (cameraRig == null)
        {
            cameraRig = FindFirstObjectByType<ShooterCameraRig>();
        }

        // エイムカメラに戻す
        if (cameraRig != null)
        {
            cameraRig.StartAim();
            Debug.Log("カメラをAimに戻しました！");
        }
    }

    void Update()
    {
        if (isTransitioning) return;

        if (Keyboard.current != null &&
            Keyboard.current.enterKey.wasPressedThisFrame)
        {
            ChangeScene(nextSceneName);
        }
    }

    // シュートした瞬間に呼び出す
    public void StartTransitionTimer()
    {
        if (isTransitioning) return;

        Debug.Log("シュート後のタイマーを開始しました！");
        StartCoroutine(TransitionAfterShot());
    }

    IEnumerator TransitionAfterShot()
    {
        isTransitioning = true;

        Debug.Log(transitionDelay + "秒後に遷移します。");

        yield return new WaitForSecondsRealtime(transitionDelay);

        if (ScoreManager.Instance == null)
        {
            Debug.LogError("ScoreManagerが見つかりません！");
            yield break;
        }

        // 今回シュートしたプレイヤーを保存
        int shotPlayer = currentPlayer;

        // 今回のシュートを記録
        ScoreManager.Instance.CompleteShot(shotPlayer);

        // 10投終了ならシーン移動しない
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

        // 交代後の表示に更新
        ScoreManager.Instance.RefreshDisplay();

        Debug.Log("シーン遷移を実行します！");

        SceneManager.LoadScene(nextSceneName);
    }

    public void ChangeScene(string sceneName)
    {
        if (isTransitioning) return;

        isTransitioning = true;
        SceneManager.LoadScene(sceneName);
    }
}
