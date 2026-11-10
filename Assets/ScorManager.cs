
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class ScoreManager : MonoBehaviour
{
    public static ScoreManager Instance;

    [Header("スコア表示")]
    [SerializeField] private TMP_Text scoreText;

    [Header("ターン表示")]
    [SerializeField] private TMP_Text turnText;

    [Header("文字サイズ")]
    [SerializeField] private float fontSize = 18f;

    // プレイヤーごとの得点
    private int player1Score = 0;
    private int player2Score = 0;

    // プレイヤーごとのシュート回数
    private int player1Shots = 0;
    private int player2Shots = 0;

    // 0 = プレイヤー1、1 = プレイヤー2
    private int currentPlayer = 0;

    // 全体のシュート回数
    private int totalShots = 0;

    private const int MaxShots = 10;

    public int CurrentPlayer => currentPlayer;
    public int TotalShots => totalShots;
    public bool IsGameFinished => totalShots >= MaxShots;

    private void Awake()
    {
        // ScoreManagerの重複防止
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        // シーン移動後もScoreManagerを残す
        DontDestroyOnLoad(gameObject);

        FindTextObjects();
        UpdateScoreText();
    }

    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    // シーン移動後に呼び出される
    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        FindTextObjects();
        UpdateScoreText();
    }

    // 新しいシーンのTextを取得
    private void FindTextObjects()
    {
        GameObject scoreObject = GameObject.Find("ScoreText");
        GameObject turnObject = GameObject.Find("TurnText");

        scoreText = scoreObject != null
            ? scoreObject.GetComponent<TMP_Text>()
            : null;

        turnText = turnObject != null
            ? turnObject.GetComponent<TMP_Text>()
            : null;
    }

    // 得点加算
    public void AddScore(int point)
    {
        if (IsGameFinished)
            return;

        if (currentPlayer == 0)
            player1Score += point;
        else
            player2Score += point;

        Debug.Log(
            "プレイヤー" + (currentPlayer + 1) +
            "に " + point + " 点！"
        );

        UpdateScoreText();
    }

    // 1回のシュート終了時に呼び出す
    public void CompleteShot()
    {
        if (IsGameFinished)
            return;

        if (currentPlayer == 0)
            player1Shots++;
        else
            player2Shots++;

        totalShots++;

        Debug.Log("シュート終了：" + totalShots + "/10");

        if (IsGameFinished)
        {
            Debug.Log("試合終了！");
            ShowFinalResult();
        }
        else
        {
            // 次のプレイヤーに交代
            currentPlayer = 1 - currentPlayer;
        }

        UpdateScoreText();
    }

    // 最終結果
    private void ShowFinalResult()
    {
        if (player1Score > player2Score)
            Debug.Log("プレイヤー1の勝利！");
        else if (player2Score > player1Score)
            Debug.Log("プレイヤー2の勝利！");
        else
            Debug.Log("引き分け！");
    }

    // スコアリセット
    public void ResetScore()
    {
        player1Score = 0;
        player2Score = 0;

        player1Shots = 0;
        player2Shots = 0;

        currentPlayer = 0;
        totalShots = 0;

        UpdateScoreText();
    }

    // 表示更新
    private void UpdateScoreText()
    {
        if (scoreText != null)
        {
            scoreText.fontSize = fontSize;

            scoreText.text =
                "プレイヤー1：" + player1Score +
                "点 (" + player1Shots + "/5)\n" +

                "プレイヤー2：" + player2Score +
                "点 (" + player2Shots + "/5)";
        }

        if (turnText != null)
        {
            turnText.fontSize = fontSize;

            if (IsGameFinished)
            {
                turnText.text = "試合終了！";
            }
            else
            {
                turnText.text =
                    "第 " + (totalShots + 1) +
                    " 投 / 10　プレイヤー" +
                    (currentPlayer + 1);
            }
        }
    }
}
