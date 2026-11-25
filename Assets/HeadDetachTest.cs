
using UnityEngine;

public class HeadDetachTest : MonoBehaviour
{
    private SkinnedMeshRenderer skinned;
    private MeshFilter meshFilter;
    private MeshRenderer meshRenderer;

    private Mesh originalMesh;
    private Material[] originalMaterials;

    private Vector3 originalPosition;
    private Quaternion originalRotation;
    private Vector3 originalScale;

    private bool initialized = false;

    private void Awake()
    {
        skinned = GetComponent<SkinnedMeshRenderer>();
        meshFilter = GetComponent<MeshFilter>();
        meshRenderer = GetComponent<MeshRenderer>();

        if (skinned == null) return;

        originalMesh = meshFilter != null ? meshFilter.sharedMesh : null;
        originalMaterials = meshRenderer != null ? meshRenderer.sharedMaterials : null;

        originalPosition = transform.localPosition;
        originalRotation = transform.localRotation;
        originalScale = transform.localScale;

        initialized = true;
    }

    [ContextMenu("頭を独立メッシュにする（テスト）")]
    public void DetachHead()
    {
        if (skinned == null)
            skinned = GetComponent<SkinnedMeshRenderer>();

        if (skinned == null)
        {
            Debug.LogError("SkinnedMeshRendererが見つかりません！");
            return;
        }

        Mesh bakedMesh = new Mesh();
        bakedMesh.name = "DetachedHeadMesh";

        skinned.BakeMesh(bakedMesh);

        Material[] materials = skinned.sharedMaterials;

        if (meshFilter == null)
            meshFilter = GetComponent<MeshFilter>();

        if (meshRenderer == null)
            meshRenderer = GetComponent<MeshRenderer>();

        meshFilter.sharedMesh = bakedMesh;
        meshRenderer.sharedMaterials = materials;

        meshRenderer.enabled = true;
        skinned.enabled = false;

        Debug.Log("頭を独立メッシュにしました！");
    }

    public void ResetHead()
    {
        if (!initialized || skinned == null) return;

        Rigidbody rb = GetComponent<Rigidbody>();

        if (rb != null)
        {
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
            rb.isKinematic = true;
        }

        transform.localPosition = originalPosition;
        transform.localRotation = originalRotation;
        transform.localScale = originalScale;

        if (meshFilter != null)
            meshFilter.sharedMesh = originalMesh;

        if (meshRenderer != null)
            meshRenderer.enabled = false;

        skinned.enabled = true;

        Debug.Log("頭を元の状態に戻しました！");
    }
}
