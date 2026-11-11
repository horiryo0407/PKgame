
using UnityEngine;
using UnityEngine.Video;
using UnityEngine.UI;
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

    // シュート開始時に呼び出す
    public void TryStartFreeze()
    {
        if (isPlaying)
            return;

        // 1 / 8192 の確率で当選
        int result = Random.Range(0, lotteryDenominator);

        if (result == 0)
        {
            StartCoroutine(PlayFreezeVideo());
        }
    }

    private IEnumerator PlayFreezeVideo()
    {
        isPlaying = true;

        // 動画表示
        blackoutVideo.SetActive(true);

        // 動画を最初から再生
        videoPlayer.Stop();
        videoPlayer.time = 0;
        videoPlayer.Play();

        // ゲームを停止
        Time.timeScale = 0f;

        // 動画が終わるまで待つ
        while (videoPlayer.isPlaying)
        {
            yield return null;
        }

        // ゲーム再開
        Time.timeScale = 1f;

        // 動画表示を消す
        blackoutVideo.SetActive(false);

        isPlaying = false;
    }
}
