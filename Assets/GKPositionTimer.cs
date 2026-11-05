
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class GKPositionTimer : MonoBehaviour
{
    [Header("時間設定")]
    public float limitTime = 10f;

    [Header("時間表示")]
    public TMP_Text timerText;

    private float remainingTime;
    private bool isFinished = false;

    void Start()
    {
        remainingTime = limitTime;
        UpdateTimerText();
    }

    void Update()
    {
        if (isFinished)
            return;

        remainingTime -= Time.deltaTime;

        if (remainingTime <= 0f)
        {
            remainingTime = 0f;
            UpdateTimerText();

            isFinished = true;

            SceneManager.LoadScene("ShootAimScene");
            return;
        }

        UpdateTimerText();
    }

    void UpdateTimerText()
    {
        if (timerText != null)
        {
            timerText.text =
                "残り：" + Mathf.CeilToInt(remainingTime) + "秒";
        }
    }
}
