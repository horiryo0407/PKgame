
using UnityEngine;
using UnityEngine.Video;
using System;
using System.Collections;

public class BlackoutFreezeController : MonoBehaviour
{
    [Header("動画設定")]
    public VideoPlayer videoPlayer;
    public GameObject blackoutVideo;

    [Header("抽選設定")]
    [Range(1, 100000)]
    public int lotteryDenominator = 8192;

    private bool isPlaying = false;
    private bool videoFinished = false;

    public void TryStartFreeze(Action onComplete)
    {
        if (isPlaying)
            return;

        int result = UnityEngine.Random.Range(
            0, lotteryDenominator);

        if (result == 0)
        {
            StartCoroutine(PlayFreezeVideo(onComplete));
        }
        else
        {
            // ハズレならすぐに通常シュート
            onComplete?.Invoke();
        }
    }

    private IEnumerator PlayFreezeVideo(Action onComplete)
    {
        isPlaying = true;

        blackoutVideo.SetActive(true);

        videoPlayer.Stop();
        videoPlayer.isLooping = false;
        videoPlayer.timeUpdateMode =
            VideoTimeUpdateMode.UnscaledGameTime;

        // 動画の準備ができるまで待つ
        videoPlayer.Prepare();

        while (!videoPlayer.isPrepared)
        {
            yield return null;
        }

        // ゲームを停止
        float previousTimeScale = Time.timeScale;
        Time.timeScale = 0f;

        videoFinished = false;

        videoPlayer.loopPointReached += OnVideoFinished;

        // 動画再生
        videoPlayer.Play();

        // 動画終了まで待機
        while (!videoFinished)
        {
            yield return null;
        }

        videoPlayer.loopPointReached -= OnVideoFinished;
        videoPlayer.Stop();

        // ゲーム再開
        Time.timeScale = previousTimeScale;

        blackoutVideo.SetActive(false);

        isPlaying = false;

        // シュートを実行
        onComplete?.Invoke();
    }

    private void OnVideoFinished(VideoPlayer player)
    {
        videoFinished = true;
    }
}
