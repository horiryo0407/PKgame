
using UnityEngine;

public class HeadDetachTest : MonoBehaviour
{
    [ContextMenu("頭を独立メッシュにする（テスト）")]
    public void DetachHead()
    {
        SkinnedMeshRenderer skinned =
            GetComponent<SkinnedMeshRenderer>();

        if (skinned == null)
        {
            Debug.LogError("SkinnedMeshRendererが見つかりません！");
            return;
        }

        // 現在の頭の形状をメッシュに焼き付ける
        Mesh bakedMesh = new Mesh();
        bakedMesh.name = "DetachedHeadMesh";

        skinned.BakeMesh(bakedMesh);

        // マテリアルを保存
        Material[] materials = skinned.sharedMaterials;

        // 独立した通常メッシュとして表示
        MeshFilter meshFilter = GetComponent<MeshFilter>();

        if (meshFilter == null)
            meshFilter = gameObject.AddComponent<MeshFilter>();

        MeshRenderer meshRenderer = GetComponent<MeshRenderer>();

        if (meshRenderer == null)
            meshRenderer = gameObject.AddComponent<MeshRenderer>();

        meshFilter.sharedMesh = bakedMesh;
        meshRenderer.sharedMaterials = materials;

        // スキンメッシュの表示を停止
        skinned.enabled = false;

        Debug.Log("頭を独立メッシュにしました！");
    }
}
