using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;
using System.Collections;

public class SceneTransition : MonoBehaviour
{
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