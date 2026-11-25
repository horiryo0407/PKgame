using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class GKPositionTimer : MonoBehaviour
{
    [Header("時間設定")]
    public float limitTime = 10f;

    [Header("時間表示")]
    public TMP_Text[] timerTexts;
    public Image[] timerGauges;

    [Header("GK操作")]
    public GKMouseBoneDrag gkMouseBoneDrag;

    [Header("GKポーズ保存")]
    public GKBonePoseSave poseSave;

    [Header("キッカー側GKポーズ保存")]
    public GKBonePoseSave kickerPoseSave;

    [Header("タイマーのCanvas")]
    public GameObject[] timerCanvases;

    void HideTimer()
    {
        foreach (var c in timerCanvases)
            if (c != null) c.SetActive(false);
    }
    private float remainingTime;
    private bool isFinished = true;

    public void StartTimer()
    {
        remainingTime = limitTime;
        isFinished = false;

        // Didaを初期状態に戻す
        if (poseSave != null)
        {
            poseSave.ResetToInitialPose();
        }
        else
        {
            Debug.LogError("DidaのPose Saveが未設定です！");
        }

        // KKdidaを初期状態に戻す
        if (kickerPoseSave != null)
        {
            kickerPoseSave.ResetToInitialPose();
        }
        else
        {
            Debug.LogError("KKdidaのPose Saveが未設定です！");
        }

        // GKSlowDiveの状態をリセット
        GKSlowDive slowDive =
            FindAnyObjectByType<GKSlowDive>();

        if (slowDive != null)
        {
            slowDive.ResetDiveState();
        }

        UpdateTimerText();

        if (gkMouseBoneDrag != null)
        {
            gkMouseBoneDrag.SetEditing(true);
        }
        else
        {
            Debug.LogError("DidaのGKMouseBoneDragが未設定です！");
        }

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


            HideTimer();

            UpdateTimerText();

            if (poseSave != null)
            {
                poseSave.SavePose();
            }
            else
            {
                Debug.LogError("DidaのPose Saveが未設定です！");
                return;
            }

            if (gkMouseBoneDrag != null)
            {
                gkMouseBoneDrag.SetEditing(false);
            }

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

    // 既存の即時コピー機能
    public void ApplyKickerPose()
    {
        if (poseSave == null || kickerPoseSave == null)
        {
            Debug.LogError("GKポーズ保存の参照がありません！");
            return;
        }

        kickerPoseSave.CopyPoseFrom(poseSave);

        Debug.Log("KKdidaにポーズを即時反映しました！");
    }

    // 指定時間をかけてKKdidaにポーズを反映
    public void ApplyKickerPoseSmooth(float duration)
    {
        if (poseSave == null || kickerPoseSave == null)
        {
            Debug.LogError("GKポーズ保存の参照がありません！");
            return;
        }

        kickerPoseSave.SmoothCopyPoseFrom(
            poseSave,
            duration
        );

        Debug.Log(
            "KKdidaのポーズを" + duration +
            "秒かけて変更します！"
        );
    }

    void UpdateTimerText()
    {
        string s = Mathf.CeilToInt(remainingTime).ToString();
        foreach (var t in timerTexts)
            if (t != null) t.text = s;

        float f = remainingTime / limitTime;
        foreach (var g in timerGauges)
            if (g != null) g.fillAmount = f;
    }
}