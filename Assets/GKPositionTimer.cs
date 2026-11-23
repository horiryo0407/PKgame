
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class GKPositionTimer : MonoBehaviour
{
    [Header("時間設定")]
    public float limitTime = 10f;

    [Header("時間表示")]
    public TMP_Text timerText;
    public Image timerGauge;

    [Header("GK操作")]
    public GKMouseBoneDrag gkMouseBoneDrag;

    [Header("GKポーズ保存")]
    public GKBonePoseSave poseSave;

    [Header("キッカー側GKポーズ保存")]
    public GKBonePoseSave kickerPoseSave;

    private float remainingTime;
    private bool isFinished = true;

    public void StartTimer()
    {
        remainingTime = limitTime;
        isFinished = false;

        // Didaを初期位置・初期ポーズに戻す
        if (poseSave != null)
        {
            poseSave.ResetToInitialPose();
        }
        else
        {
            Debug.LogError("DidaのPose Saveが未設定です！");
        }

        // 保存した最新ポーズをKKdidaに反映
        if (kickerPoseSave != null)
        {
            kickerPoseSave.LoadPose(false);
        }
        else
        {
            Debug.LogError("KKdidaのPose Saveが未設定です！");
        }

        // KKdidaを初期位置・初期ポーズに戻す
        if (kickerPoseSave != null)
        {
            kickerPoseSave.ResetToInitialPose();
        }
        else
        {
            Debug.LogError("KKdidaのPose Saveが未設定です！");
        }

        UpdateTimerText();

        // GK操作を有効にする
        if (gkMouseBoneDrag != null)
        {
            gkMouseBoneDrag.SetEditing(true);
        }
        else
        {
            Debug.LogError("DidaのGKMouseBoneDragが未設定です！");
        }

        // キッカー操作を無効にする
        BallShooter shooter =
            FindAnyObjectByType<BallShooter>();

        if (shooter != null)
        {
            shooter.SetShooterControl(false);
        }

        Debug.Log("GKのポーズ設定タイマー開始！");
    }

    void Update()
    {
        if (isFinished)
            return;

        remainingTime -= Time.deltaTime;

        if (remainingTime <= 0f)
        {
            remainingTime = 0f;
            isFinished = true;

            UpdateTimerText();

            // Didaのポーズを保存
            if (poseSave != null)
            {
                poseSave.SavePose();
            }
            else
            {
                Debug.LogError("DidaのPose Saveが未設定です！");
                return;
            }

            // GK操作を無効にする
            if (gkMouseBoneDrag != null)
            {
                gkMouseBoneDrag.SetEditing(false);
            }

            // キッカー操作を有効にする
            BallShooter shooter =
                FindAnyObjectByType<BallShooter>();

            if (shooter != null)
            {
                shooter.SetShooterControl(true);
            }

            Debug.Log("GKのポーズ保存完了！キッカー操作開始！");
        }
        else
        {
            UpdateTimerText();
        }
    }

    void UpdateTimerText()
    {
        if (timerText != null)
        {
            timerText.text =
                Mathf.CeilToInt(remainingTime).ToString();
        }

        if (timerGauge != null)
        {
            timerGauge.fillAmount =
                remainingTime / limitTime;

            timerGauge.color =
                remainingTime <= 3f ? Color.red : Color.white;
        }
    }
}
