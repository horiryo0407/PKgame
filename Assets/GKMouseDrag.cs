
using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections.Generic;

public class GKMouseBoneDrag : MonoBehaviour
{
    [Header("カメラ")]
    public Camera targetCamera;

    [Header("回転リング")]
    public float ringRadius = 0.18f;
    public float ringWidth = 0.008f;
    public float clickDistance = 14f;

    [Header("回転設定")]
    [Range(0.1f, 2f)]
    public float rotationSensitivity = 1f;

    [Header("胴体の左右移動")]
    public Transform playerRoot;
    public float moveSensitivity = 0.01f;

    [Header("移動矢印")]
    public float moveArrowLength = 0.65f;
    public float moveArrowWidth = 0.025f;
    public float moveArrowClickDistance = 24f;

    private class MoveArrow
    {
        public Vector3 screenAxis;
        public LineRenderer shaft;
        public LineRenderer headLeft;
        public LineRenderer headRight;
        public Color color;
    }

    private readonly List<MoveArrow> moveArrows = new List<MoveArrow>();
    private bool moveGizmoVisible;
    private Vector3 selectedMoveAxis;
    private Vector3 selectedMoveScreenAxis;
    private bool draggingMoveArrow;
    private GKBoneHandle moveGizmoHandle;

    private class RotationRing
    {
        public Transform bone;
        public LineRenderer line;
        public Vector3 localAxis;
    }

    private readonly List<RotationRing> rings =
        new List<RotationRing>();

    private RotationRing selectedRing;
    private GKBoneHandle selectedMoveHandle;

    private Vector2 previousMousePosition;
    private Vector3 selectedWorldAxis;
    private Vector3 previousWorldVector;

    private Plane rotationPlane;
    private bool useWorldPlane;

    // ドラッグ開始時のリングの画面上の形を保存する
    private Vector2[] dragRingScreenPoints;
    private float previousRingAngle;

    void Start()
    {
        if (targetCamera == null)
        {
            targetCamera = Camera.main;
        }

        CreateRotationRings();
        CreateMoveArrows();
    }

    void Update()
    {
        if (Mouse.current == null || targetCamera == null)
        {
            return;
        }

        UpdateMoveArrowPositions();

        Vector2 mousePosition =
            Mouse.current.position.ReadValue();

        if (Mouse.current.leftButton.wasPressedThisFrame)
        {
            TrySelect(mousePosition);
        }

        if (Mouse.current.leftButton.isPressed)
        {
            if (selectedRing != null)
            {
                RotateSelectedBone(mousePosition);
            }
            else if (draggingMoveArrow)
            {
                MoveBody(mousePosition);
            }
        }

        if (Mouse.current.leftButton.wasReleasedThisFrame)
        {
            selectedRing = null;
            selectedMoveHandle = null;
            draggingMoveArrow = false;
            dragRingScreenPoints = null;
        }
    }

    // ------------------------------
    // 移動矢印を作成・表示
    // ------------------------------

    void CreateMoveArrows()
    {
        moveArrows.Clear();
        moveArrows.Add(CreateMoveArrow("MoveArrow_Horizontal", Vector3.right, Color.red));
        moveArrows.Add(CreateMoveArrow("MoveArrow_Vertical", Vector3.up, Color.green));
        SetMoveArrowRenderers(false);
    }

    MoveArrow CreateMoveArrow(string objectName, Vector3 screenAxis, Color color)
    {
        GameObject arrowObject = new GameObject(objectName);
        arrowObject.transform.SetParent(transform, false);

        MoveArrow arrow = new MoveArrow();
        arrow.screenAxis = screenAxis;
        arrow.color = color;
        arrow.shaft = CreateArrowLine(arrowObject.transform, "Shaft", color);
        arrow.headLeft = CreateArrowLine(arrowObject.transform, "HeadLeft", color);
        arrow.headRight = CreateArrowLine(arrowObject.transform, "HeadRight", color);
        return arrow;
    }

    LineRenderer CreateArrowLine(Transform parent, string objectName, Color color)
    {
        GameObject lineObject = new GameObject(objectName);
        lineObject.transform.SetParent(parent, false);

        LineRenderer line = lineObject.AddComponent<LineRenderer>();
        line.useWorldSpace = true;
        line.positionCount = 2;
        line.startWidth = moveArrowWidth;
        line.endWidth = moveArrowWidth;
        line.startColor = color;
        line.endColor = color;
        line.numCapVertices = 4;
        line.alignment = LineAlignment.View;
        line.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        line.receiveShadows = false;

        Shader shader = Shader.Find("Sprites/Default");
        if (shader != null)
        {
            line.material = new Material(shader);
            // できるだけ前面に描画する
            line.material.renderQueue = 5000;
        }

        return line;
    }

    void SetMoveArrowRenderers(bool visible)
    {
        foreach (MoveArrow arrow in moveArrows)
        {
            if (arrow.shaft != null) arrow.shaft.enabled = visible;
            if (arrow.headLeft != null) arrow.headLeft.enabled = visible;
            if (arrow.headRight != null) arrow.headRight.enabled = visible;
        }
    }

    void UpdateMoveArrowPositions()
    {
        if (!moveGizmoVisible || moveGizmoHandle == null || targetCamera == null)
            return;

        // hipは胴体の内側にあることが多いため、
        // カメラ側へ大きめにずらして矢印が体に隠れないようにする
        Vector3 anchor = moveGizmoHandle.transform.position
            - targetCamera.transform.forward * 0.5f;

        foreach (MoveArrow arrow in moveArrows)
        {
            Vector3 axis = arrow.screenAxis == Vector3.right
                ? targetCamera.transform.right
                : targetCamera.transform.up;

            Vector3 end = anchor + axis * moveArrowLength;
            Vector3 side = Vector3.Cross(targetCamera.transform.forward, axis).normalized
                * moveArrowLength * 0.12f;

            arrow.shaft.SetPosition(0, anchor);
            arrow.shaft.SetPosition(1, end);

            arrow.headLeft.SetPosition(0, end);
            arrow.headLeft.SetPosition(1, end - axis * moveArrowLength * 0.18f + side);

            arrow.headRight.SetPosition(0, end);
            arrow.headRight.SetPosition(1, end - axis * moveArrowLength * 0.18f - side);
        }
    }

    float GetMoveArrowScreenDistance(MoveArrow arrow, Vector2 mousePosition)
    {
        if (arrow.shaft == null || !arrow.shaft.enabled)
            return float.MaxValue;

        Vector3 a3 = targetCamera.WorldToScreenPoint(arrow.shaft.GetPosition(0));
        Vector3 b3 = targetCamera.WorldToScreenPoint(arrow.shaft.GetPosition(1));

        if (a3.z <= 0f || b3.z <= 0f)
            return float.MaxValue;

        return DistanceToSegment(
            mousePosition,
            new Vector2(a3.x, a3.y),
            new Vector2(b3.x, b3.y));
    }

    // ------------------------------
    // 回転リングを作成
    // ------------------------------

    void CreateRotationRings()
    {
        rings.Clear();

        GKBoneHandle[] handles =
            FindObjectsByType<GKBoneHandle>(
                FindObjectsSortMode.None
            );

        foreach (GKBoneHandle handle in handles)
        {
            if (handle.handleType !=
                GKBoneHandle.HandleType.Rotate)
            {
                continue;
            }

            CreateRing(
                handle.transform,
                Vector3.right,
                Color.red,
                "Ring_X"
            );

            CreateRing(
                handle.transform,
                Vector3.forward,
                Color.blue,
                "Ring_Z"
            );
        }
    }

    void CreateRing(
        Transform bone,
        Vector3 axis,
        Color color,
        string ringName)
    {
        GameObject ringObject = new GameObject(ringName);

        ringObject.transform.SetParent(bone, false);
        ringObject.transform.localPosition = Vector3.zero;
        ringObject.transform.localRotation = Quaternion.identity;
        ringObject.transform.localScale = Vector3.one;

        LineRenderer line =
            ringObject.AddComponent<LineRenderer>();

        line.useWorldSpace = false;
        line.loop = true;
        line.positionCount = 49;
        line.startWidth = ringWidth;
        line.endWidth = ringWidth;
        line.startColor = color;
        line.endColor = color;
        line.numCapVertices = 2;
        line.numCornerVertices = 2;

        Shader shader = Shader.Find("Sprites/Default");

        if (shader != null)
        {
            line.material = new Material(shader);
        }

        for (int i = 0; i < 49; i++)
        {
            float angle = i * Mathf.PI * 2f / 48f;

            float x = Mathf.Cos(angle) * ringRadius;
            float y = Mathf.Sin(angle) * ringRadius;

            Vector3 point;

            if (axis == Vector3.right)
            {
                // 赤リング：X軸を中心に回転
                point = new Vector3(0f, y, x);
            }
            else
            {
                // 青リング：Z軸を中心に回転
                point = new Vector3(x, y, 0f);
            }

            line.SetPosition(i, point);
        }

        rings.Add(new RotationRing
        {
            bone = bone,
            line = line,
            localAxis = axis
        });
    }

    // ------------------------------
    // リング・hipの選択
    // ------------------------------

    void TrySelect(Vector2 mousePosition)
    {
        selectedRing = null;
        selectedMoveHandle = null;
        draggingMoveArrow = false;

        // まず、表示中の移動矢印をクリックしたか確認する
        if (moveGizmoVisible)
        {
            float nearestArrowDistance = moveArrowClickDistance;
            MoveArrow nearestArrow = null;

            foreach (MoveArrow arrow in moveArrows)
            {
                float distance = GetMoveArrowScreenDistance(arrow, mousePosition);
                if (distance < nearestArrowDistance)
                {
                    nearestArrowDistance = distance;
                    nearestArrow = arrow;
                }
            }

            if (nearestArrow != null)
            {
                selectedMoveScreenAxis = nearestArrow.screenAxis;
                selectedMoveAxis = nearestArrow.screenAxis == Vector3.right
                    ? targetCamera.transform.right
                    : targetCamera.transform.up;
                draggingMoveArrow = true;
                previousMousePosition = mousePosition;
                return;
            }
        }

        // 先にhipなどの移動ハンドルを探す。
        // 胴体をクリックしたとき、近くの回転リングに判定を取られないようにする。
        Ray ray =
            targetCamera.ScreenPointToRay(mousePosition);

        RaycastHit[] hits = Physics.RaycastAll(
            ray,
            100f,
            Physics.DefaultRaycastLayers,
            QueryTriggerInteraction.Collide
        );

        System.Array.Sort(
            hits,
            (a, b) => a.distance.CompareTo(b.distance)
        );

        foreach (RaycastHit hit in hits)
        {
            GKBoneHandle handle =
                hit.collider.GetComponent<GKBoneHandle>();

            if (handle == null)
            {
                handle =
                    hit.collider.GetComponentInParent<GKBoneHandle>();
            }

            if (handle != null &&
                handle.handleType ==
                GKBoneHandle.HandleType.MoveSideways)
            {
                // 胴体をクリックしたら矢印を表示する。ここではまだ移動しない
                moveGizmoHandle = handle;
                moveGizmoVisible = true;
                SetMoveArrowRenderers(true);
                UpdateMoveArrowPositions();
                Debug.Log("GK移動矢印を表示しました: " + handle.name);
                return;
            }
        }

        // hipをクリックしていない場合は、回転リングを探す
        float nearestDistance = clickDistance;

        foreach (RotationRing ring in rings)
        {
            if (ring.bone == null || ring.line == null)
            {
                continue;
            }

            float distance =
                GetRingScreenDistance(ring, mousePosition);

            if (distance < nearestDistance)
            {
                nearestDistance = distance;
                selectedRing = ring;
            }
        }

        if (selectedRing != null)
        {
            moveGizmoVisible = false;
            SetMoveArrowRenderers(false);
            BeginRingDrag(mousePosition);
            return;
        }
    }

    // ------------------------------
    // ドラッグ開始時の準備
    // ------------------------------

    void BeginRingDrag(Vector2 mousePosition)
    {
        selectedWorldAxis =
            selectedRing.bone.TransformDirection(
                selectedRing.localAxis
            ).normalized;

        rotationPlane = new Plane(
            selectedWorldAxis,
            selectedRing.bone.position
        );

        // カメラ方向と回転軸の角度を調べる
        float axisViewDot = Mathf.Abs(
            Vector3.Dot(
                targetCamera.transform.forward,
                selectedWorldAxis
            )
        );

        // 正面からリングを見ている場合は
        // 3D平面上で角度を計算する
        useWorldPlane = axisViewDot >= 0.15f;

        Ray ray =
            targetCamera.ScreenPointToRay(mousePosition);

        if (useWorldPlane)
        {
            if (rotationPlane.Raycast(ray, out float enter))
            {
                previousWorldVector =
                    ray.GetPoint(enter) -
                    selectedRing.bone.position;

                if (previousWorldVector.sqrMagnitude < 0.0001f)
                {
                    useWorldPlane = false;
                }
            }
            else
            {
                useWorldPlane = false;
            }
        }

        // リングがカメラから見て横向きに近い場合は
        // 画面上のリング形状を基準にする
        if (!useWorldPlane)
        {
            SaveRingScreenPoints();
            previousRingAngle =
                FindClosestRingAngle(mousePosition, 0f, false);
        }

        previousMousePosition = mousePosition;
    }

    void SaveRingScreenPoints()
    {
        int count = selectedRing.line.positionCount;

        dragRingScreenPoints = new Vector2[count];

        for (int i = 0; i < count; i++)
        {
            Vector3 world =
                selectedRing.line.transform.TransformPoint(
                    selectedRing.line.GetPosition(i)
                );

            Vector3 screen =
                targetCamera.WorldToScreenPoint(world);

            dragRingScreenPoints[i] =
                new Vector2(screen.x, screen.y);
        }
    }

    // ------------------------------
    // 回転処理
    // ------------------------------

    void RotateSelectedBone(Vector2 mousePosition)
    {
        if (selectedRing == null ||
            selectedRing.bone == null)
        {
            return;
        }

        float angle;

        if (useWorldPlane)
        {
            // カメラからマウス位置へレイを飛ばす
            Ray ray =
                targetCamera.ScreenPointToRay(mousePosition);

            if (!rotationPlane.Raycast(ray, out float enter))
            {
                return;
            }

            Vector3 currentWorldVector =
                ray.GetPoint(enter) -
                selectedRing.bone.position;

            if (currentWorldVector.sqrMagnitude < 0.0001f ||
                previousWorldVector.sqrMagnitude < 0.0001f)
            {
                previousWorldVector = currentWorldVector;
                return;
            }

            // 3D空間でマウスの角度変化を計算
            angle = Vector3.SignedAngle(
                previousWorldVector,
                currentWorldVector,
                selectedWorldAxis
            );

            previousWorldVector = currentWorldVector;
        }
        else
        {
            // リングが画面上で細い線に見える場合は、
            // リング上のどの位置をマウスが移動したかを調べる
            float currentRingAngle =
                FindClosestRingAngle(
                    mousePosition,
                    previousRingAngle,
                    true
                );

            angle = Mathf.DeltaAngle(
                previousRingAngle,
                currentRingAngle
            );

            // 赤リングの点の並びはX軸の正方向に対して
            // 逆向きになるため補正する
            if (selectedRing.localAxis == Vector3.right)
            {
                angle = -angle;
            }

            previousRingAngle = currentRingAngle;
        }

        angle *= rotationSensitivity;

        if (Mathf.Abs(angle) > 0.0001f)
        {
            selectedRing.bone.Rotate(
                selectedWorldAxis,
                angle,
                Space.World
            );
        }

        previousMousePosition = mousePosition;
    }

    // ------------------------------
    // 画面上のリング位置から角度を求める
    // ------------------------------

    float FindClosestRingAngle(
        Vector2 mousePosition,
        float referenceAngle,
        bool useContinuity)
    {
        if (dragRingScreenPoints == null ||
            dragRingScreenPoints.Length < 2)
        {
            return referenceAngle;
        }

        float bestScore = float.MaxValue;
        float bestAngle = referenceAngle;

        int segmentCount = dragRingScreenPoints.Length - 1;

        for (int i = 0; i < segmentCount; i++)
        {
            Vector2 a = dragRingScreenPoints[i];
            Vector2 b = dragRingScreenPoints[i + 1];

            Vector2 segment = b - a;

            float t = 0f;

            if (segment.sqrMagnitude > 0.001f)
            {
                t = Mathf.Clamp01(
                    Vector2.Dot(mousePosition - a, segment) /
                    segment.sqrMagnitude
                );
            }

            Vector2 closest = a + segment * t;

            float distance =
                Vector2.Distance(mousePosition, closest);

            float parameter = i + t;

            float candidateAngle = Mathf.Repeat(
                parameter / segmentCount * 360f,
                360f
            );

            float score = distance;

            if (useContinuity)
            {
                float angleDifference = Mathf.Abs(
                    Mathf.DeltaAngle(
                        referenceAngle,
                        candidateAngle
                    )
                );

                // 重なったリング部分で角度が急に飛ぶのを抑える
                score += angleDifference * 0.05f;
            }

            if (score < bestScore)
            {
                bestScore = score;
                bestAngle = candidateAngle;
            }
        }

        return bestAngle;
    }

    // ------------------------------
    // リングの選択判定
    // ------------------------------

    float GetRingScreenDistance(
        RotationRing ring,
        Vector2 mousePosition)
    {
        float nearest = float.MaxValue;

        int count = ring.line.positionCount;

        for (int i = 0; i < count - 1; i++)
        {
            Vector3 worldA =
                ring.line.transform.TransformPoint(
                    ring.line.GetPosition(i)
                );

            Vector3 worldB =
                ring.line.transform.TransformPoint(
                    ring.line.GetPosition(i + 1)
                );

            Vector3 screenA =
                targetCamera.WorldToScreenPoint(worldA);

            Vector3 screenB =
                targetCamera.WorldToScreenPoint(worldB);

            if (screenA.z <= 0f || screenB.z <= 0f)
            {
                continue;
            }

            float distance = DistanceToSegment(
                mousePosition,
                new Vector2(screenA.x, screenA.y),
                new Vector2(screenB.x, screenB.y)
            );

            if (distance < nearest)
            {
                nearest = distance;
            }
        }

        return nearest;
    }

    float DistanceToSegment(
        Vector2 point,
        Vector2 a,
        Vector2 b)
    {
        Vector2 segment = b - a;

        if (segment.sqrMagnitude < 0.001f)
        {
            return Vector2.Distance(point, a);
        }

        float t = Mathf.Clamp01(
            Vector2.Dot(point - a, segment) /
            segment.sqrMagnitude
        );

        return Vector2.Distance(
            point,
            a + segment * t
        );
    }

    // ------------------------------
    // hipによる胴体の左右移動
    // ------------------------------

    void MoveBody(Vector2 mousePosition)
    {
        if (playerRoot == null || !draggingMoveArrow)
        {
            return;
        }

        Vector2 mouseDelta = mousePosition - previousMousePosition;
        Vector2 screenAxis = selectedMoveScreenAxis == Vector3.right
            ? Vector2.right
            : Vector2.up;

        float amount = Vector2.Dot(mouseDelta, screenAxis) * moveSensitivity;
        playerRoot.position += selectedMoveAxis * amount;

        previousMousePosition = mousePosition;
        UpdateMoveArrowPositions();
    }
}
