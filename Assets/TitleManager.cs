
using UnityEngine;
using UnityEngine.SceneManagement;

public class TitleManager : MonoBehaviour
{
    [Header("ゲーム開始時に読み込むシーン")]
    [SerializeField] private string gameSceneName = "TwiceScene";

    private bool isStarting = false;

    private void Awake()
    {
        DontDestroyOnLoad(gameObject);
    }

    public void StartGame()
    {
        if (isStarting) return;

        isStarting = true;

        SceneManager.LoadScene(
            gameSceneName,
            LoadSceneMode.Single
        );
    }

    public void ExitGame()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}
