
using UnityEngine;

public class GKHeadFly : MonoBehaviour
{
    [Header("頭を飛ばす力")]
    public float flyForce = 8f;

    [Header("上方向への力")]
    public float upwardForce = 4f;

    private Rigidbody rb;
    private bool detached = false;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();

        if (rb == null)
        {
            Debug.LogError("HeadMesh_TestにRigidbodyがありません！");
        }
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
