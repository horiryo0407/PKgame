
using UnityEngine;
using System.Collections;

public class GameSlowMotion : MonoBehaviour
{
    [Header("スローモーション設定")]
    [Range(0.1f, 1f)]
    public float slowMotionScale = 0.4f;

    public float slowMotionDuration = 2.5f;

    private float previousTimeScale;
    private float previousFixedDeltaTime;

    private bool isActive;

    public void StartSlowMotion()
    {
        if (isActive)
            return;

        previousTimeScale = Time.timeScale;
        previousFixedDeltaTime = Time.fixedDeltaTime;

        Time.timeScale = slowMotionScale;
        Time.fixedDeltaTime =
            previousFixedDeltaTime *
            (slowMotionScale / previousTimeScale);

        isActive = true;

        StartCoroutine(SlowMotionTimer());
    }

    private IEnumerator SlowMotionTimer()
    {
        yield return new WaitForSecondsRealtime(
            slowMotionDuration
        );

        RestoreTime();
    }

    private void RestoreTime()
    {
        if (!isActive)
            return;

        Time.timeScale = previousTimeScale;
        Time.fixedDeltaTime = previousFixedDeltaTime;

        isActive = false;

        Debug.Log("スローモーション終了");
    }

    private void OnDisable()
    {
        RestoreTime();
    }
}
