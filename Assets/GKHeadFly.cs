
using UnityEngine;

public class GKHeadFly : MonoBehaviour
{
    [Header("追従させる元の頭ボーン")]
    public Transform originalHead;

    [Header("頭を飛ばす力")]
    public float flyForce = 8f;

    [Header("上方向への力")]
    public float upwardForce = 4f;

    private Rigidbody rb;
    private bool detached = false;

    private Vector3 followPositionOffset;
    private Quaternion followRotationOffset;

    private void Start()
    {
        rb = GetComponent<Rigidbody>();

        if (originalHead == null)
        {
            Debug.LogError("元のheadボーンが設定されていません！");
            return;
        }

        followPositionOffset =
            Quaternion.Inverse(originalHead.rotation) *
            (transform.position - originalHead.position);

        followRotationOffset =
            Quaternion.Inverse(originalHead.rotation) *
            transform.rotation;

        rb.isKinematic = true;
    }

    private void LateUpdate()
    {
        if (detached || originalHead == null)
            return;

        transform.position =
            originalHead.position +
            originalHead.rotation * followPositionOffset;

        transform.rotation =
            originalHead.rotation * followRotationOffset;
    }

    public void Fly(Vector3 hitDirection)
    {
        if (detached || rb == null)
            return;

        detached = true;

        rb.isKinematic = false;
        rb.useGravity = true;

        Vector3 force =
            hitDirection.normalized * flyForce +
            Vector3.up * upwardForce;

        rb.AddForce(force, ForceMode.Impulse);

        Debug.Log("頭が吹っ飛びました！");
    }
}
