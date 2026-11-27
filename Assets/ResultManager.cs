
using UnityEngine;

public class ResultManager : MonoBehaviour
{
    [Header("勝敗表示")]
    [SerializeField] private GameObject player1Win;
    [SerializeField] private GameObject player2Win;
    [SerializeField] private GameObject draw;

    [Header("テスト用：0=1P、1=2P、-1=引き分け")]
    [SerializeField] private int testWinner = 0;

    private void Awake()
    {
        // 2画面目を有効にする
        if (Display.displays.Length > 1)
        {
            Display.displays[1].Activate();
        }
    }

    private void Start()
    {
        int winner = testWinner;

        if (ScoreManager.Instance != null)
        {
            winner = ScoreManager.Instance.Winner;
        }

        if (player1Win != null)
            player1Win.SetActive(winner == 0);

        if (player2Win != null)
            player2Win.SetActive(winner == 1);

        if (draw != null)
            draw.SetActive(winner == -1);
    }
}
