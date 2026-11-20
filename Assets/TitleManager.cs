
using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class TitleManager : MonoBehaviour
{
    [Header("ゲーム開始時に読み込むシーン")]
    [SerializeField] private string kickerSceneName = "ShootAimScene";
    [SerializeField] private string goalkeeperSceneName = "GKPoseSetting";

    private bool isStarting = false;

    private void Awake()
    {
        DontDestroyOnLoad(gameObject);
    }

    public void StartGame()
    {
        if (isStarting) return;

        isStarting = true;
        StartCoroutine(LoadGameScenes());
    }

    public void ExitGame()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
    Application.Quit(); 
#endif
    }

    private IEnumerator LoadGameScenes()
    {
        // ShootAimSceneを読み込む
        yield return SceneManager.LoadSceneAsync(
            kickerSceneName,
            LoadSceneMode.Single
        );

        // GKPoseSettingを追加読み込み
        yield return SceneManager.LoadSceneAsync(
            goalkeeperSceneName,
            LoadSceneMode.Additive
        );

        Debug.Log("2画面のゲームシーンを読み込みました！");

        // 読み込み完了後、TitleManagerを破棄
        Destroy(gameObject);
    }
}
