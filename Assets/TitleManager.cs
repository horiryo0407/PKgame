
using UnityEngine;
using UnityEngine.SceneManagement;

public class TitleManager : MonoBehaviour
{
    [Header("ゲーム本編のシーン名")]
    public string gameSceneName;

    public void StartGame()
    {
        if (string.IsNullOrEmpty(gameSceneName))
        {
            Debug.LogError("ゲーム本編のシーン名が設定されていません！");
            return;
        }

        SceneManager.LoadScene(gameSceneName);
    }
}
