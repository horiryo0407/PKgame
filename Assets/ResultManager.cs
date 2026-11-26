using UnityEngine;

public class ResultManager : MonoBehaviour
{
    [SerializeField] private GameObject player1Win;
    [SerializeField] private GameObject player2Win;
    [SerializeField] private GameObject draw;

    [Header("このシーン単体で試すとき用（0=1P、1=2P、-1=引き分け）")]
    [SerializeField] private int testWinner = 0;

    void Start()
    {
        int winner = testWinner;

        if (ScoreManager.Instance != null)
        {
            winner = ScoreManager.Instance.Winner;
        }

        player1Win.SetActive(winner == 0);
        player2Win.SetActive(winner == 1);
        draw.SetActive(winner == -1);
    }
}