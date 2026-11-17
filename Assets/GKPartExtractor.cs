
using UnityEngine;
using System.Collections.Generic;

public class GKPartExtractor : MonoBehaviour
{
    public SkinnedMeshRenderer target;

    [ContextMenu("頭メッシュを切り出す")]
    public void ExtractHead()
    {
        if (target == null || target.sharedMesh == null)
        {
            Debug.LogError("Targetが設定されていません！");
            return;
        }

        Mesh source = target.sharedMesh;
        BoneWeight[] weights = source.boneWeights;
        Transform[] bones = target.bones;

        int headIndex = -1;

        for (int i = 0; i < bones.Length; i++)
        {
            if (bones[i] != null && bones[i].name == "head")
            {
                headIndex = i;
                break;
            }
        }

        if (headIndex < 0)
        {
            Debug.LogError("headボーンが見つかりません！");
            return;
        }

        List<int> selectedTriangles = new List<int>();

        for (int sub = 0; sub < source.subMeshCount; sub++)
        {
            if (source.GetTopology(sub) != MeshTopology.Triangles)
                continue;

            int[] triangles = source.GetTriangles(sub);

            for (int i = 0; i < triangles.Length; i += 3)
            {
                int a = triangles[i];
                int b = triangles[i + 1];
                int c = triangles[i + 2];

                bool aHead = IsHead(weights[a], headIndex);
                bool bHead = IsHead(weights[b], headIndex);
                bool cHead = IsHead(weights[c], headIndex);

                if (aHead && bHead && cHead)
                {
                    selectedTriangles.Add(a);
                    selectedTriangles.Add(b);
                    selectedTriangles.Add(c);
                }
            }
        }

        Debug.Log("頭として切り出せる三角形数：" + selectedTriangles.Count / 3);

        if (selectedTriangles.Count == 0)
        {
            Debug.LogWarning("頭の三角形が見つかりませんでした。");
            return;
        }

        Mesh result = new Mesh();
        result.name = "ExtractedHead_Test";
        result.indexFormat = source.indexFormat;

        List<Vector3> vertices = new List<Vector3>();
        List<Vector3> normals = new List<Vector3>();
        List<Vector2> uv = new List<Vector2>();
        List<int> trianglesResult = new List<int>();

        Dictionary<int, int> vertexMap = new Dictionary<int, int>();

        foreach (int oldIndex in selectedTriangles)
        {
            if (!vertexMap.TryGetValue(oldIndex, out int newIndex))
            {
                newIndex = vertices.Count;
                vertexMap.Add(oldIndex, newIndex);

                vertices.Add(source.vertices[oldIndex]);

                if (source.normals.Length > oldIndex)
                    normals.Add(source.normals[oldIndex]);

                if (source.uv.Length > oldIndex)
                    uv.Add(source.uv[oldIndex]);
            }

            trianglesResult.Add(newIndex);
        }

        result.SetVertices(vertices);
        result.SetTriangles(trianglesResult, 0);

        if (normals.Count == vertices.Count)
            result.SetNormals(normals);

        if (uv.Count == vertices.Count)
            result.SetUVs(0, uv);

        result.RecalculateBounds();

        GameObject preview = new GameObject("HeadMesh_Test");
        preview.transform.SetParent(transform, false);

        MeshFilter filter = preview.AddComponent<MeshFilter>();
        filter.sharedMesh = result;

        MeshRenderer renderer = preview.AddComponent<MeshRenderer>();
        renderer.sharedMaterials = target.sharedMaterials;

        Debug.Log("頭メッシュのテスト切り出しに成功！");
        Debug.Log("切り出した頂点数：" + result.vertexCount);
    }

    private bool IsHead(BoneWeight weight, int headIndex)
    {
        return
            (weight.boneIndex0 == headIndex && weight.weight0 > 0.1f) ||
            (weight.boneIndex1 == headIndex && weight.weight1 > 0.1f) ||
            (weight.boneIndex2 == headIndex && weight.weight2 > 0.1f) ||
            (weight.boneIndex3 == headIndex && weight.weight3 > 0.1f);
    }
}
