
using UnityEngine;

public class MeshInfoChecker : MonoBehaviour
{
    public SkinnedMeshRenderer target;

    [ContextMenu("頭の頂点数を確認")]
    public void CheckHead()
    {
        if (target == null || target.sharedMesh == null)
        {
            Debug.LogError("Targetが設定されていません！");
            return;
        }

        Mesh mesh = target.sharedMesh;
        Transform[] bones = target.bones;
        BoneWeight[] weights = mesh.boneWeights;

        int headIndex = -1;

        for (int i = 0; i < bones.Length; i++)
        {
            if (bones[i] != null && bones[i].name == "head")
            {
                headIndex = i;
                break;
            }
        }

        if (headIndex == -1)
        {
            Debug.LogError("headボーンが見つかりません！");
            return;
        }

        int headVertices = 0;

        foreach (BoneWeight weight in weights)
        {
            if (weight.boneIndex0 == headIndex && weight.weight0 > 0.5f ||
                weight.boneIndex1 == headIndex && weight.weight1 > 0.5f ||
                weight.boneIndex2 == headIndex && weight.weight2 > 0.5f ||
                weight.boneIndex3 == headIndex && weight.weight3 > 0.5f)
            {
                headVertices++;
            }
        }

        Debug.Log("頭に強く割り当てられた頂点数：" + headVertices);
        Debug.Log("メッシュ全体の頂点数：" + mesh.vertexCount);
    }
}
