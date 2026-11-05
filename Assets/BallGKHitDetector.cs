
using UnityEngine;

public class BallGKHitDetector : MonoBehaviour
{
    private bool scoredThisShot = false;

    private void OnCollisionEnter(Collision collision)
    {
        if (scoredThisShot)
            return;

        GKHitPoint hitPoint =
            collision.collider.GetComponentInParent<GKHitPoint>();

        if (hitPoint == null)
            return;

        if (ScoreManager.Instance == null)
        {
            Debug.LogError("ScoreManagerが見つかりません！");
            return;
        }

        ScoreManager.Instance.AddScore(hitPoint.Point);

        scoredThisShot = true;
    }

    // 次のシュートを始めるときに呼び出す
    public void ResetForNextShot()
    {
        scoredThisShot = false;
    }
}
