
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public class GKPartExtractor : MonoBehaviour
{
    [Header("切り出す対象")]
    public SkinnedMeshRenderer target;

    [Header("赤線で表示する頭の輪郭番号")]
    public int showContourNumber = 1;

    private List<int> debugContour = new List<int>();

    private struct Edge
    {
        public int a;
        public int b;

        public Edge(int v1, int v2)
        {
            a = Mathf.Min(v1, v2);
            b = Mathf.Max(v1, v2);
        }

        public override int GetHashCode()
        {
            return a * 397 ^ b;
        }

        public override bool Equals(object obj)
        {
            if (!(obj is Edge)) return false;

            Edge other = (Edge)obj;
            return a == other.a && b == other.b;
        }
    }

    private struct PositionKey
    {
        public int x;
        public int y;
        public int z;

        public PositionKey(Vector3 p)
        {
            x = Mathf.RoundToInt(p.x * 10000f);
            y = Mathf.RoundToInt(p.y * 10000f);
            z = Mathf.RoundToInt(p.z * 10000f);
        }

        public override int GetHashCode()
        {
            return (x * 73856093) ^
                   (y * 19349663) ^
                   (z * 83492791);
        }

        public override bool Equals(object obj)
        {
            if (!(obj is PositionKey)) return false;

            PositionKey other = (PositionKey)obj;

            return x == other.x &&
                   y == other.y &&
                   z == other.z;
        }
    }

    [ContextMenu("頭と胴体を切り出す")]
    public void ExtractHeadAndBody()
    {
        if (target == null)
        {
            Debug.LogError("Target が設定されていません！");
            return;
        }

        Mesh source = target.sharedMesh;

        if (source == null)
        {
            Debug.LogError("Target に Mesh がありません！");
            return;
        }

        Transform[] bones = target.bones;
        BoneWeight[] weights = source.boneWeights;

        if (weights == null || weights.Length != source.vertexCount)
        {
            Debug.LogError("ボーンウェイトが取得できません！");
            return;
        }

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
            Debug.LogError("head ボーンが見つかりません！");
            return;
        }

        // headボーンと、その子孫ボーンを調べる
        bool[] headBones = new bool[bones.Length];

        for (int i = 0; i < bones.Length; i++)
        {
            if (bones[i] == null) continue;

            Transform current = bones[i];

            while (current != null)
            {
                if (current == bones[headIndex])
                {
                    headBones[i] = true;
                    break;
                }

                current = current.parent;
            }
        }

        // 頭と胴体のメッシュをそれぞれ作る
        Mesh headMesh = CreatePartMesh(
            source, weights, headBones, true);

        Mesh bodyMesh = CreatePartMesh(
            source, weights, headBones, false);

        if (headMesh == null || bodyMesh == null)
        {
            Debug.LogError("頭または胴体のメッシュ作成に失敗しました！");
            return;
        }

        // 前回作ったプレビューを削除
        DeletePreview("HeadMesh_Test");
        DeletePreview("BodyMesh_Test");

        // 頭の首の断面をふさぐ
        List<List<int>> headLoops =
            FindOrderedContours(headMesh, true);

        if (headLoops.Count > 0)
        {
            CapContour(headMesh, headLoops[0]);
        }
        else
        {
            Debug.LogWarning("頭側の閉じた輪郭が見つかりませんでした。");
        }

        // 胴体の首の断面をふさぐ
        List<List<int>> bodyLoops =
            FindOrderedContours(bodyMesh, false);

        if (bodyLoops.Count > 0)
        {
            // 胴体は一番高い位置の輪郭を使う
            CapContour(bodyMesh, bodyLoops[bodyLoops.Count - 1]);
        }
        else
        {
            Debug.LogWarning("胴体側の閉じた輪郭が見つかりませんでした。");
        }

        // それぞれ別のオブジェクトとして作る
        CreatePreview("HeadMesh_Test", headMesh);
        CreatePreview("BodyMesh_Test", bodyMesh);

        Debug.Log("頭と胴体の切り出しが完了しました！");
        Debug.Log("頭の頂点数：" + headMesh.vertexCount);
        Debug.Log("胴体の頂点数：" + bodyMesh.vertexCount);
    }

    private Mesh CreatePartMesh(
        Mesh source,
        BoneWeight[] weights,
        bool[] headBones,
        bool makeHead)
    {
        Vector3[] vertices = source.vertices;
        Vector3[] normals = source.normals;
        Vector2[] uv = source.uv;
        Color[] colors = source.colors;

        List<Vector3> newVertices = new List<Vector3>();
        List<Vector3> newNormals = new List<Vector3>();
        List<Vector2> newUV = new List<Vector2>();
        List<Color> newColors = new List<Color>();

        Dictionary<int, int> vertexMap =
            new Dictionary<int, int>();

        List<List<int>> subMeshes =
            new List<List<int>>();

        for (int s = 0; s < source.subMeshCount; s++)
        {
            int[] triangles = source.GetTriangles(s);
            List<int> newTriangles = new List<int>();

            for (int i = 0; i < triangles.Length; i += 3)
            {
                int a = triangles[i];
                int b = triangles[i + 1];
                int c = triangles[i + 2];

                bool aIsHead = IsHeadVertex(a, weights, headBones);
                bool bIsHead = IsHeadVertex(b, weights, headBones);
                bool cIsHead = IsHeadVertex(c, weights, headBones);

                if (makeHead)
                {
                    if (!aIsHead || !bIsHead || !cIsHead)
                        continue;
                }
                else
                {
                    if (aIsHead || bIsHead || cIsHead)
                        continue;
                }

                int[] originalIndices = { a, b, c };

                for (int j = 0; j < 3; j++)
                {
                    int originalIndex = originalIndices[j];

                    if (!vertexMap.ContainsKey(originalIndex))
                    {
                        int newIndex = newVertices.Count;
                        vertexMap.Add(originalIndex, newIndex);

                        newVertices.Add(vertices[originalIndex]);

                        if (normals.Length == vertices.Length)
                            newNormals.Add(normals[originalIndex]);

                        if (uv.Length == vertices.Length)
                            newUV.Add(uv[originalIndex]);

                        if (colors.Length == vertices.Length)
                            newColors.Add(colors[originalIndex]);
                    }

                    newTriangles.Add(vertexMap[originalIndex]);
                }
            }

            subMeshes.Add(newTriangles);
        }

        if (newVertices.Count == 0)
        {
            Debug.LogError(
                makeHead ? "頭のメッシュがありません！" :
                           "胴体のメッシュがありません！");

            return null;
        }

        Mesh result = new Mesh();

        result.name = makeHead ? "ExtractedHead" : "ExtractedBody";

        if (newVertices.Count > 65535)
            result.indexFormat = IndexFormat.UInt32;

        result.SetVertices(newVertices);
        result.subMeshCount = subMeshes.Count;

        for (int s = 0; s < subMeshes.Count; s++)
            result.SetTriangles(subMeshes[s], s);

        if (newNormals.Count == newVertices.Count)
            result.SetNormals(newNormals);
        else
            result.RecalculateNormals();

        if (newUV.Count == newVertices.Count)
            result.SetUVs(0, newUV);

        if (newColors.Count == newVertices.Count)
            result.SetColors(newColors);

        result.RecalculateBounds();

        return result;
    }

    private bool IsHeadVertex(
        int index,
        BoneWeight[] weights,
        bool[] headBones)
    {
        BoneWeight w = weights[index];

        return
            IsHeadWeight(w.boneIndex0, w.weight0, headBones) ||
            IsHeadWeight(w.boneIndex1, w.weight1, headBones) ||
            IsHeadWeight(w.boneIndex2, w.weight2, headBones) ||
            IsHeadWeight(w.boneIndex3, w.weight3, headBones);
    }

    private bool IsHeadWeight(
        int boneIndex,
        float weight,
        bool[] headBones)
    {
        return weight > 0.02f &&
               boneIndex >= 0 &&
               boneIndex < headBones.Length &&
               headBones[boneIndex];
    }

    private void CreatePreview(string objectName, Mesh mesh)
    {
        GameObject preview = new GameObject(objectName);

        preview.transform.SetParent(target.transform, false);
        preview.transform.localPosition = Vector3.zero;
        preview.transform.localRotation = Quaternion.identity;
        preview.transform.localScale = Vector3.one;

        MeshFilter filter = preview.AddComponent<MeshFilter>();
        MeshRenderer meshRenderer = preview.AddComponent<MeshRenderer>();

        filter.sharedMesh = mesh;
        meshRenderer.sharedMaterials = target.sharedMaterials;
    }

    private void DeletePreview(string objectName)
    {
        Transform old = target.transform.Find(objectName);

        if (old != null)
            DestroyImmediate(old.gameObject);
    }

    private List<List<int>> FindOrderedContours(
        Mesh mesh,
        bool drawDebug)
    {
        Vector3[] vertices = mesh.vertices;

        Dictionary<PositionKey, int> positionMap =
            new Dictionary<PositionKey, int>();

        List<int> representative = new List<int>();
        int[] welded = new int[vertices.Length];

        // 同じ位置の頂点をまとめる
        for (int i = 0; i < vertices.Length; i++)
        {
            PositionKey key = new PositionKey(vertices[i]);

            if (!positionMap.TryGetValue(key, out int id))
            {
                id = representative.Count;
                positionMap.Add(key, id);
                representative.Add(i);
            }

            welded[i] = id;
        }

        Dictionary<Edge, int> edgeCounts =
            new Dictionary<Edge, int>();

        for (int s = 0; s < mesh.subMeshCount; s++)
        {
            int[] triangles = mesh.GetTriangles(s);

            for (int i = 0; i < triangles.Length; i += 3)
            {
                int a = welded[triangles[i]];
                int b = welded[triangles[i + 1]];
                int c = welded[triangles[i + 2]];

                AddEdge(edgeCounts, a, b);
                AddEdge(edgeCounts, b, c);
                AddEdge(edgeCounts, c, a);
            }
        }

        Dictionary<int, List<int>> adjacency =
            new Dictionary<int, List<int>>();

        int boundaryCount = 0;

        foreach (var pair in edgeCounts)
        {
            if (pair.Value != 1) continue;

            boundaryCount++;

            AddNeighbor(adjacency, pair.Key.a, pair.Key.b);
            AddNeighbor(adjacency, pair.Key.b, pair.Key.a);
        }

        Debug.Log("位置を統合した後の境界辺数：" + boundaryCount);

        HashSet<int> visited = new HashSet<int>();
        List<List<int>> loops = new List<List<int>>();

        foreach (var pair in adjacency)
        {
            int start = pair.Key;

            if (visited.Contains(start))
                continue;

            Queue<int> queue = new Queue<int>();
            List<int> component = new List<int>();

            queue.Enqueue(start);
            visited.Add(start);

            while (queue.Count > 0)
            {
                int current = queue.Dequeue();
                component.Add(current);

                foreach (int next in adjacency[current])
                {
                    if (visited.Contains(next))
                        continue;

                    visited.Add(next);
                    queue.Enqueue(next);
                }
            }

            bool simple = true;

            foreach (int v in component)
            {
                if (adjacency[v].Count != 2)
                {
                    simple = false;
                    break;
                }
            }

            if (!simple) continue;

            List<int> loop = new List<int>();
            int previous = -1;
            int currentVertex = start;
            bool closed = false;

            for (int i = 0; i <= component.Count; i++)
            {
                if (loop.Contains(representative[currentVertex]))
                    break;

                loop.Add(representative[currentVertex]);

                List<int> neighbors = adjacency[currentVertex];

                int next =
                    neighbors[0] != previous
                    ? neighbors[0]
                    : neighbors[1];

                if (next == start)
                {
                    closed = true;
                    break;
                }

                previous = currentVertex;
                currentVertex = next;
            }

            if (closed &&
                loop.Count == component.Count &&
                loop.Count >= 8)
            {
                loops.Add(loop);
            }
        }

        // 平均Yが低い輪郭から並べる
        loops.Sort((a, b) =>
        {
            float yA = 0f;
            float yB = 0f;

            foreach (int i in a)
                yA += vertices[i].y;

            foreach (int i in b)
                yB += vertices[i].y;

            yA /= a.Count;
            yB /= b.Count;

            return yA.CompareTo(yB);
        });

        Debug.Log("検出した閉じた輪郭数：" + loops.Count);

        for (int i = 0; i < loops.Count; i++)
        {
            Vector3 center = Vector3.zero;

            foreach (int index in loops[i])
                center += vertices[index];

            center /= loops[i].Count;

            Debug.Log(
                "輪郭 " + (i + 1) +
                " / 頂点数：" + loops[i].Count +
                " / 中心：" + center);
        }

        if (drawDebug)
        {
            debugContour.Clear();

            int selected = showContourNumber - 1;

            if (selected >= 0 && selected < loops.Count)
                debugContour.AddRange(loops[selected]);
        }

        return loops;
    }

    private void CapContour(Mesh mesh, List<int> loop)
    {
        if (loop == null || loop.Count < 3)
            return;

        List<Vector3> vertices =
            new List<Vector3>(mesh.vertices);

        List<Vector2> uv =
            new List<Vector2>(mesh.uv);

        List<Color> colors =
            new List<Color>(mesh.colors);

        bool hasUV = uv.Count == vertices.Count;
        bool hasColors = colors.Count == vertices.Count;

        Vector3 center = Vector3.zero;
        Vector2 uvCenter = Vector2.zero;
        Color colorCenter = Color.clear;

        foreach (int index in loop)
        {
            center += vertices[index];

            if (hasUV)
                uvCenter += uv[index];

            if (hasColors)
                colorCenter += colors[index];
        }

        center /= loop.Count;

        if (hasUV)
            uvCenter /= loop.Count;

        if (hasColors)
            colorCenter /= loop.Count;

        int centerIndex = vertices.Count;
        vertices.Add(center);

        if (hasUV)
            uv.Add(uvCenter);

        if (hasColors)
            colors.Add(colorCenter);

        // 輪郭を中心点から三角形でふさぐ
        List<int> triangles =
            new List<int>(mesh.GetTriangles(0));

        for (int i = 0; i < loop.Count; i++)
        {
            int current = loop[i];
            int next = loop[(i + 1) % loop.Count];

            triangles.Add(centerIndex);
            triangles.Add(next);
            triangles.Add(current);
        }

        mesh.SetVertices(vertices);
        mesh.SetTriangles(triangles, 0);

        if (hasUV)
            mesh.SetUVs(0, uv);

        if (hasColors)
            mesh.SetColors(colors);

        mesh.RecalculateNormals();
        mesh.RecalculateBounds();

        Debug.Log("断面をふさぎました。頂点数：" + loop.Count);
    }

    private void AddEdge(
        Dictionary<Edge, int> counts,
        int a,
        int b)
    {
        Edge edge = new Edge(a, b);

        if (counts.ContainsKey(edge))
            counts[edge]++;
        else
            counts.Add(edge, 1);
    }

    private void AddNeighbor(
        Dictionary<int, List<int>> adjacency,
        int a,
        int b)
    {
        if (!adjacency.ContainsKey(a))
            adjacency.Add(a, new List<int>());

        adjacency[a].Add(b);
    }

    private void OnDrawGizmos()
    {
        if (target == null ||
            debugContour == null ||
            debugContour.Count < 2)
            return;

        Transform preview =
            target.transform.Find("HeadMesh_Test");

        if (preview == null) return;

        MeshFilter filter =
            preview.GetComponent<MeshFilter>();

        if (filter == null || filter.sharedMesh == null)
            return;

        Vector3[] vertices = filter.sharedMesh.vertices;

        Gizmos.color = Color.red;

        for (int i = 0; i < debugContour.Count; i++)
        {
            Vector3 current =
                preview.TransformPoint(vertices[debugContour[i]]);

            Vector3 next =
                preview.TransformPoint(
                    vertices[
                        debugContour[(i + 1) % debugContour.Count]
                    ]);

            Gizmos.DrawLine(current, next);
            Gizmos.DrawSphere(current, 0.008f);
        }
    }
}
