
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

    [Header("勝敗結果表示")]
    [SerializeField] private TMP_Text resultText;

    [Header("文字サイズ")]
    [SerializeField] private float fontSize = 18f;

    private int player1Score = 0;
    private int player2Score = 0;

    private int player1Shots = 0;
    private int player2Shots = 0;

    private int totalShots = 0;

    private const int MaxShots = 10;

    public int TotalShots => totalShots;
    public bool IsGameFinished => totalShots >= MaxShots;

    // 勝者：0 = プレイヤー1、1 = プレイヤー2、-1 = 引き分け
    private int winner = -2;
    public int Winner => winner;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
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

    private void OnDestroy()
    {
        if (Instance == this)
        {
            Instance = null;
        }
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        FindTextObjects();
        UpdateScoreText();
    }

    private void FindTextObjects()
    {
        GameObject scoreObject = GameObject.Find("ScoreText");
        GameObject turnObject = GameObject.Find("TurnText");
        GameObject resultObject = GameObject.Find("ResultText");

        scoreText = scoreObject != null
            ? scoreObject.GetComponent<TMP_Text>()
            : null;

        turnText = turnObject != null
            ? turnObject.GetComponent<TMP_Text>()
            : null;

        resultText = resultObject != null
            ? resultObject.GetComponent<TMP_Text>()
            : null;

        // 試合終了までは非表示
        if (resultText != null)
        {
            resultText.text = "";
            resultText.enabled = false;
        }
    }

    // 得点加算
    public void AddScore(int point)
    {
        if (IsGameFinished)
            return;

        int currentPlayer = SceneTransition.CurrentPlayer;

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

    // シュート終了
    public void CompleteShot(int shotPlayer)
    {
        if (IsGameFinished)
            return;

        if (shotPlayer == 0)
            player1Shots++;
        else
            player2Shots++;

        totalShots++;

        Debug.Log("シュート終了：" + totalShots + "/10");

        if (IsGameFinished)
        {
            JudgeWinner();
        }

        UpdateScoreText();
    }

    public void CompleteShot()
    {
        CompleteShot(SceneTransition.CurrentPlayer);
    }

    // 勝敗判定
    private void JudgeWinner()
    {
        if (player1Score > player2Score)
        {
            winner = 0;
            Debug.Log("プレイヤー1の勝利！");
        }
        else if (player2Score > player1Score)
        {
            winner = 1;
            Debug.Log("プレイヤー2の勝利！");
        }
        else
        {
            winner = -1;
            Debug.Log("引き分け！");
        }

        ShowFinalResult();
    }

    // 最終結果表示
    private void ShowFinalResult()
    {
        if (resultText == null)
        {
            Debug.LogWarning("ResultTextが見つかりません。");
            return;
        }

        // 結果を表示
        resultText.enabled = true;

        if (winner == 0)
        {
            resultText.text = "プレイヤー1の勝利！";
        }
        else if (winner == 1)
        {
            resultText.text = "プレイヤー2の勝利！";
        }
        else
        {
            resultText.text = "引き分け！";
        }

        resultText.text +=
            "\n\n最終スコア\n" +
            "プレイヤー1：" + player1Score + "点\n" +
            "プレイヤー2：" + player2Score + "点";
    }

    // 勝者を取得
    // 戻り値：0 = プレイヤー1、1 = プレイヤー2、-1 = 引き分け、-2 = 未判定
    public int GetWinner()
    {
        return winner;
    }

    // スコアリセット
    public void ResetScore()
    {
        player1Score = 0;
        player2Score = 0;

        player1Shots = 0;
        player2Shots = 0;

        totalShots = 0;

        winner = -2;

        SceneTransition.ResetTurn();

        if (resultText != null)
        {
            resultText.text = "";
            resultText.enabled = false;
        }

        UpdateScoreText();
    }

    public void RefreshDisplay()
    {
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
                    (SceneTransition.CurrentPlayer + 1);
            }
        }
    }
}
