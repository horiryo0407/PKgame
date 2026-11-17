
using UnityEngine;

public class MeshInfoChecker : MonoBehaviour
{
    public SkinnedMeshRenderer target;

    [ContextMenu("メッシュ情報を確認")]
    public void CheckMesh()
    {
        if (target == null)
        {
            Debug.LogError("TargetにSkinnedMeshRendererを設定してください！");
            return;
        }

        Mesh mesh = target.sharedMesh;

        if (mesh == null)
        {
            Debug.LogError("メッシュが見つかりません！");
            return;
        }

        Debug.Log("===== メッシュ情報 =====");
        Debug.Log("メッシュ名：" + mesh.name);
        Debug.Log("頂点数：" + mesh.vertexCount);
        Debug.Log("SubMesh数：" + mesh.subMeshCount);
        Debug.Log("ボーン数：" + target.bones.Length);
        Debug.Log("BindPose数：" + mesh.bindposes.Length);

        foreach (Transform bone in target.bones)
        {
            if (bone != null)
                Debug.Log("ボーン：" + bone.name);
        }

        Debug.Log("===== 確認終了 =====");
    }
}
