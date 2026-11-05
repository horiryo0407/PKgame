
using UnityEngine;
using TMPro;

public class ScoreManager : MonoBehaviour
{
    public static ScoreManager Instance;

    [SerializeField] private TMP_Text scoreText;

    private int kickerScore = 0;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        UpdateScoreText();
    }

    public void AddScore(int point)
    {
        kickerScore += point;
        UpdateScoreText();

        Debug.Log("キッカーに " + point + " 点！ 合計：" + kickerScore);
    }

    public void ResetScore()
    {
        kickerScore = 0;
        UpdateScoreText();
    }

    private void UpdateScoreText()
    {
        if (scoreText != null)
        {
            scoreText.text = "キッカー：" + kickerScore + "点";
        }
    }
}
