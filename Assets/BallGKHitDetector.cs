
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

        // 得点を加算
        ScoreManager.Instance.AddScore(hitPoint.Point);

        scoredThisShot = true;

        // HeadColliderに当たった場合
        if (collision.collider.name == "HeadCollider")
        {
            GKHeadFly headFly = FindObjectOfType<GKHeadFly>();

            if (headFly != null)
            {
                headFly.Fly(collision.relativeVelocity);
            }
            else
            {
                Debug.LogError("GKHeadFlyが見つかりません！");
            }
        }
    }

    // 次のシュートを始めるときに呼び出す
    public void ResetForNextShot()
    {
        scoredThisShot = false;
    }
}
