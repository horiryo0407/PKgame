using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;
using System.Collections;

public class SceneTransition : MonoBehaviour
{
    [Header("遷移先")]
    [SerializeField] private string nextSceneName;

    [Header("シュート後の自動遷移")]
    [SerializeField] private float transitionDelay = 5f;

    private bool isTransitioning = false;

    // 次のキーパーを記録する
    private const string KeeperKey = "NextKeeperIsShooter";

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

        StartCoroutine(TransitionAfterShot());
    }

    // シュート後のタイマー
    IEnumerator TransitionAfterShot()
    {
        isTransitioning = true;

        Debug.Log(transitionDelay + "秒後にシーンを切り替えます。");

        yield return new WaitForSeconds(transitionDelay);

        // 次のキーパーを交替
        bool nextKeeper = PlayerPrefs.GetInt(KeeperKey, 0) == 0;
        PlayerPrefs.SetInt(KeeperKey, nextKeeper ? 1 : 0);
        PlayerPrefs.Save();

        Debug.Log("次のキーパー：" +
            (nextKeeper ? "Shooter" : "Dida"));

        SceneManager.LoadScene(nextSceneName);
    }

    // シーン移動
    public void ChangeScene(string sceneName)
    {
        if (isTransitioning) return;

        isTransitioning = true;
        SceneManager.LoadScene(sceneName);
    }
}