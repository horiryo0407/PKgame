
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

        HashSet<int> headBoneIndices = new HashSet<int>();

        for (int i = 0; i < bones.Length; i++)
        {
            if (bones[i] != null &&
                (bones[i] == bones[headIndex] ||
                 bones[i].IsChildOf(bones[headIndex])))
            {
                headBoneIndices.Add(i);
            }
        }

        List<Vector3> vertices = new List<Vector3>();
        List<Vector3> normals = new List<Vector3>();
        List<Vector2> uv = new List<Vector2>();

        List<int>[] subTriangles =
            new List<int>[source.subMeshCount];

        Dictionary<int, int> vertexMap =
            new Dictionary<int, int>();

        for (int i = 0; i < subTriangles.Length; i++)
        {
            subTriangles[i] = new List<int>();
        }

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

                if (!IsHead(weights[a], headBoneIndices) ||
                    !IsHead(weights[b], headBoneIndices) ||
                    !IsHead(weights[c], headBoneIndices))
                    continue;

                AddVertex(a, source, vertices, normals, uv,
                    vertexMap, subTriangles[sub]);

                AddVertex(b, source, vertices, normals, uv,
                    vertexMap, subTriangles[sub]);

                AddVertex(c, source, vertices, normals, uv,
                    vertexMap, subTriangles[sub]);
            }
        }

        int triangleCount = 0;

        foreach (var list in subTriangles)
            triangleCount += list.Count / 3;

        Debug.Log("頭の三角形数：" + triangleCount);

        if (triangleCount == 0)
        {
            Debug.LogWarning("頭の三角形が見つかりませんでした。");
            return;
        }

        Mesh result = new Mesh();
        result.name = "ExtractedHead_Test";
        result.indexFormat = source.indexFormat;
        result.SetVertices(vertices);

        if (normals.Count == vertices.Count)
            result.SetNormals(normals);

        if (uv.Count == vertices.Count)
            result.SetUVs(0, uv);

        result.subMeshCount = source.subMeshCount;

        for (int i = 0; i < subTriangles.Length; i++)
            result.SetTriangles(subTriangles[i], i);

        result.RecalculateBounds();

        // 境界の辺を調べる
        Dictionary<Edge, int> edgeCounts =
            new Dictionary<Edge, int>();

        foreach (var triangles in subTriangles)
        {
            for (int i = 0; i < triangles.Count; i += 3)
            {
                AddEdge(edgeCounts, triangles[i], triangles[i + 1]);
                AddEdge(edgeCounts, triangles[i + 1], triangles[i + 2]);
                AddEdge(edgeCounts, triangles[i + 2], triangles[i]);
            }
        }

        int boundaryCount = 0;

        foreach (var pair in edgeCounts)
        {
            if (pair.Value == 1)
                boundaryCount++;
        }

        Debug.Log("切り口などの境界辺の数：" + boundaryCount);

        GameObject preview = new GameObject("HeadMesh_Test");
        preview.transform.SetParent(transform, false);

        MeshFilter filter = preview.AddComponent<MeshFilter>();
        filter.sharedMesh = result;

        MeshRenderer renderer =
            preview.AddComponent<MeshRenderer>();

        renderer.sharedMaterials = target.sharedMaterials;

        Debug.Log("頭メッシュ切り出し成功！");
        Debug.Log("頂点数：" + result.vertexCount);
        Debug.Log("頭メッシュの下端Y：" + result.bounds.min.y);
        Debug.Log("頭メッシュの上端Y：" + result.bounds.max.y);
    }

    private struct Edge
    {
        public int a;
        public int b;

        public Edge(int x, int y)
        {
            a = Mathf.Min(x, y);
            b = Mathf.Max(x, y);
        }

        public override int GetHashCode()
        {
            return a * 397 ^ b;
        }

        public override bool Equals(object obj)
        {
            if (!(obj is Edge))
                return false;

            Edge other = (Edge)obj;
            return a == other.a && b == other.b;
        }
    }

    private void AddEdge(
        Dictionary<Edge, int> edges,
        int a,
        int b)
    {
        Edge edge = new Edge(a, b);

        if (edges.ContainsKey(edge))
            edges[edge]++;
        else
            edges.Add(edge, 1);
    }

    private void AddVertex(
        int oldIndex,
        Mesh source,
        List<Vector3> vertices,
        List<Vector3> normals,
        List<Vector2> uv,
        Dictionary<int, int> vertexMap,
        List<int> triangles)
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

        triangles.Add(newIndex);
    }

    private bool IsHead(
        BoneWeight weight,
        HashSet<int> headBoneIndices)
    {
        return
            (headBoneIndices.Contains(weight.boneIndex0) &&
             weight.weight0 > 0.05f) ||
            (headBoneIndices.Contains(weight.boneIndex1) &&
             weight.weight1 > 0.05f) ||
            (headBoneIndices.Contains(weight.boneIndex2) &&
             weight.weight2 > 0.05f) ||
            (headBoneIndices.Contains(weight.boneIndex3) &&
             weight.weight3 > 0.05f);
    }
}
