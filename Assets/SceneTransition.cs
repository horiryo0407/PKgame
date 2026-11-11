
using System.Collections;
using Unity.Multiplayer.PlayMode;
using UnityEditor.Rendering;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class SceneTransition : MonoBehaviour
{

    [SerializeField] private TurnSkinManager turnSkinManager;//プレイヤー切り替え
    [SerializeField] private ShooterCameraRig cameraRig;//カメラ

    [Header("遷移先")]
    [SerializeField] private string nextSceneName = "GKPoseSetting";

    [Header("シュート後の遷移時間")]
    [SerializeField] private float transitionDelay = 5f;

    private bool isTransitioning = false;

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

        // シュート終了を記録
        if (ScoreManager.Instance != null)
        {
            ScoreManager.Instance.CompleteShot();
        }
        else
        {
            Debug.LogError("ScoreManagerが見つかりません！");
        }

        // 10投終了ならシーン移動しない
        if (ScoreManager.Instance != null &&
            ScoreManager.Instance.IsGameFinished)
        {
            Debug.Log("10投終了！試合終了です！");
            yield break;
        }

        Debug.Log("シーン遷移を実行します！");

        SceneManager.LoadScene(nextSceneName);
        cameraRig.StartAim();
    }

    public void ChangeScene(string sceneName)
    {
        if (isTransitioning) return;

        isTransitioning = true;

        SceneManager.LoadScene(sceneName);
    }
}
