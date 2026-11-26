
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class ScoreManager : MonoBehaviour
{
    public static ScoreManager Instance;

    [Header("スコア表示")]
    private readonly List<TMP_Text> player1ScoreTexts = new List<TMP_Text>();
    private readonly List<TMP_Text> player2ScoreTexts = new List<TMP_Text>();

    [Header("ターン表示")]
    private readonly List<TMP_Text> turnTexts = new List<TMP_Text>();

    [Header("勝敗結果表示")]
    private readonly List<TMP_Text> resultTexts = new List<TMP_Text>();

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

    public int Player1Score => player1Score;
    public int Player2Score => player2Score;

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
        player1ScoreTexts.Clear();
        player2ScoreTexts.Clear();
        turnTexts.Clear();
        resultTexts.Clear();

        // 名前の先頭が同じテキストを、すべて集める（両モニターぶん）
        TMP_Text[] allTexts =
            FindObjectsByType<TMP_Text>(FindObjectsSortMode.None);

        foreach (TMP_Text t in allTexts)
        {
            if (t.name.StartsWith("Player1ScoreText"))
                player1ScoreTexts.Add(t);
            else if (t.name.StartsWith("Player2ScoreText"))
                player2ScoreTexts.Add(t);
            else if (t.name.StartsWith("TurnText"))
                turnTexts.Add(t);
            else if (t.name.StartsWith("ResultText"))
                resultTexts.Add(t);
        }

        // 試合終了までは非表示
        foreach (TMP_Text r in resultTexts)
        {
            r.text = "";
            r.enabled = false;
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
        if (resultTexts.Count == 0)
        {
            Debug.LogWarning("ResultTextが見つかりません。");
            return;
        }

        string message;

        if (winner == 0)
        {
            message = "プレイヤー1の勝利！";
        }
        else if (winner == 1)
        {
            message = "プレイヤー2の勝利！";
        }
        else
        {
            message = "引き分け！";
        }

        message +=
            "\n\n最終スコア\n" +
            "プレイヤー1：" + player1Score + "点\n" +
            "プレイヤー2：" + player2Score + "点";

        // 結果を表示
        foreach (TMP_Text r in resultTexts)
        {
            r.enabled = true;
            r.text = message;
        }
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

        foreach (TMP_Text r in resultTexts)
        {
            r.text = "";
            r.enabled = false;
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
        foreach (TMP_Text t in player1ScoreTexts)
        {
            t.text = "1P   " + player1Score;
        }

        foreach (TMP_Text t in player2ScoreTexts)
        {
            t.text = player2Score + "   2P";
        }

        foreach (TMP_Text t in turnTexts)
        {
            t.text =
                Mathf.Min(totalShots / 2 + 1, MaxShots / 2).ToString();
        }
    }
}