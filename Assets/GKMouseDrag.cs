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

    [Header("基本の関節回転制限（度）")]
    public float shoulderLimit = 85f;
    public float bendLimit = 120f;
    public float bendSideLimit = 20f;
    public float thighLimit = 90f;
    public float thighSideLimit = 45f;

    [Header("個別にきつく制限する軸（度）")]
    [Tooltip("肩の青リング（横軸）の制限")]
    [Range(5f, 90f)]
    public float shoulderBlueLimit = 35f;

    [Tooltip("太もも・股関節の赤リング（縦軸）の制限")]
    [Range(5f, 90f)]
    public float hipRedLimit = 45f;

    [Tooltip("すね・膝関節の赤リング（縦軸）の制限")]
    [Range(5f, 135f)]
    public float kneeRedLimit = 70f;

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

    private readonly List<MoveArrow> moveArrows =
        new List<MoveArrow>();

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

        public float currentAngle;
        public float minAngle;
        public float maxAngle;
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

        // 起動時から移動矢印を表示
        GKBoneHandle[] handles =
            FindObjectsByType<GKBoneHandle>(
                FindObjectsSortMode.None);

        foreach (GKBoneHandle handle in handles)
        {
            if (handle.handleType ==
                GKBoneHandle.HandleType.MoveSideways)
            {
                moveGizmoHandle = handle;
                moveGizmoVisible = true;

                SetMoveArrowRenderers(true);
                UpdateMoveArrowPositions();
                break;
            }
        }

        if (moveGizmoHandle == null)
        {
            Debug.LogWarning(
                "GK移動矢印：MoveSidewaysに設定されたGKBoneHandleが見つかりません。hipのHandle Typeを確認してください。");
        }
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
    // 移動矢印を作成
    // ------------------------------

    void CreateMoveArrows()
    {
        moveArrows.Clear();

        moveArrows.Add(
            CreateMoveArrow(
                "MoveArrow_Horizontal",
                Vector3.right,
                Color.red));

        moveArrows.Add(
            CreateMoveArrow(
                "MoveArrow_Vertical",
                Vector3.up,
                Color.green));

        SetMoveArrowRenderers(false);
    }

    MoveArrow CreateMoveArrow(
        string objectName,
        Vector3 screenAxis,
        Color color)
    {
        GameObject arrowObject =
            new GameObject(objectName);

        arrowObject.transform.SetParent(transform, false);

        MoveArrow arrow = new MoveArrow();

        arrow.screenAxis = screenAxis;
        arrow.color = color;

        arrow.shaft = CreateArrowLine(
            arrowObject.transform, "Shaft", color);

        arrow.headLeft = CreateArrowLine(
            arrowObject.transform, "HeadLeft", color);

        arrow.headRight = CreateArrowLine(
            arrowObject.transform, "HeadRight", color);

        return arrow;
    }

    LineRenderer CreateArrowLine(
        Transform parent,
        string objectName,
        Color color)
    {
        GameObject lineObject = new GameObject(objectName);

        lineObject.transform.SetParent(parent, false);

        LineRenderer line =
            lineObject.AddComponent<LineRenderer>();

        line.useWorldSpace = true;
        line.positionCount = 2;
        line.startWidth = moveArrowWidth;
        line.endWidth = moveArrowWidth;
        line.startColor = color;
        line.endColor = color;
        line.numCapVertices = 4;
        line.alignment = LineAlignment.View;
        line.shadowCastingMode =
            UnityEngine.Rendering.ShadowCastingMode.Off;
        line.receiveShadows = false;

        Shader shader = Shader.Find("Sprites/Default");

        if (shader != null)
        {
            line.material = new Material(shader);
            line.material.renderQueue = 5000;
        }

        return line;
    }

    void SetMoveArrowRenderers(bool visible)
    {
        foreach (MoveArrow arrow in moveArrows)
        {
            if (arrow.shaft != null)
                arrow.shaft.enabled = visible;

            if (arrow.headLeft != null)
                arrow.headLeft.enabled = visible;

            if (arrow.headRight != null)
                arrow.headRight.enabled = visible;
        }
    }

    void UpdateMoveArrowPositions()
    {
        if (!moveGizmoVisible ||
            moveGizmoHandle == null ||
            targetCamera == null)
        {
            return;
        }

        Vector3 anchor =
            moveGizmoHandle.transform.position
            - targetCamera.transform.forward * 0.5f;

        foreach (MoveArrow arrow in moveArrows)
        {
            Vector3 axis =
                arrow.screenAxis == Vector3.right
                ? targetCamera.transform.right
                : targetCamera.transform.up;

            Vector3 end =
                anchor + axis * moveArrowLength;

            Vector3 side =
                Vector3.Cross(
                    targetCamera.transform.forward,
                    axis).normalized
                * moveArrowLength * 0.12f;

            arrow.shaft.SetPosition(0, anchor);
            arrow.shaft.SetPosition(1, end);

            arrow.headLeft.SetPosition(0, end);
            arrow.headLeft.SetPosition(
                1,
                end - axis * moveArrowLength * 0.18f + side);

            arrow.headRight.SetPosition(0, end);
            arrow.headRight.SetPosition(
                1,
                end - axis * moveArrowLength * 0.18f - side);
        }
    }

    float GetMoveArrowScreenDistance(
        MoveArrow arrow,
        Vector2 mousePosition)
    {
        if (arrow.shaft == null ||
            !arrow.shaft.enabled)
        {
            return float.MaxValue;
        }

        Vector3 a3 = targetCamera.WorldToScreenPoint(
            arrow.shaft.GetPosition(0));

        Vector3 b3 = targetCamera.WorldToScreenPoint(
            arrow.shaft.GetPosition(1));

        if (a3.z <= 0f || b3.z <= 0f)
        {
            return float.MaxValue;
        }

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
                FindObjectsSortMode.None);

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
                "Ring_X");

            CreateRing(
                handle.transform,
                Vector3.forward,
                Color.blue,
                "Ring_Z");
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
                point = new Vector3(0f, y, x);
            }
            else
            {
                point = new Vector3(x, y, 0f);
            }

            line.SetPosition(i, point);
        }

        float limit = GetJointLimit(bone.name, axis);

        rings.Add(new RotationRing
        {
            bone = bone,
            line = line,
            localAxis = axis,
            currentAngle = 0f,
            minAngle = -limit,
            maxAngle = limit
        });
    }

    // ------------------------------
    // 部位ごとの回転制限
    // ------------------------------

    float GetJointLimit(string boneName, Vector3 axis)
    {
        string name = boneName.ToLowerInvariant();

        bool isRedRing = axis == Vector3.right;
        bool isBlueRing = axis == Vector3.forward;

        // 肩：青リングを厳しく制限
        if (name.Contains("shldr") ||
            name.Contains("shoulder") ||
            name.Contains("upperarm") ||
            name.Contains("upper_arm"))
        {
            if (isBlueRing)
                return shoulderBlueLimit;

            return shoulderLimit;
        }

        // 肘・前腕
        if (name.Contains("forearm") ||
            name.Contains("elbow"))
        {
            return isRedRing
                ? bendLimit
                : bendSideLimit;
        }

        // すね・膝：赤リングを厳しく制限
        if (name.Contains("shin") ||
            name.Contains("calf") ||
            name.Contains("knee"))
        {
            if (isRedRing)
                return kneeRedLimit;

            return bendSideLimit;
        }

        // 太もも・股関節：赤リングを厳しく制限
        if (name.Contains("thigh") ||
            name.Contains("upleg"))
        {
            if (isRedRing)
                return hipRedLimit;

            return thighSideLimit;
        }

        return shoulderLimit;
    }

    // ------------------------------
    // リング・移動ハンドルの選択
    // ------------------------------

    void TrySelect(Vector2 mousePosition)
    {
        selectedRing = null;
        selectedMoveHandle = null;
        draggingMoveArrow = false;

        // 移動矢印をクリックしたか確認
        if (moveGizmoVisible)
        {
            float nearestArrowDistance =
                moveArrowClickDistance;

            MoveArrow nearestArrow = null;

            foreach (MoveArrow arrow in moveArrows)
            {
                float distance =
                    GetMoveArrowScreenDistance(
                        arrow, mousePosition);

                if (distance < nearestArrowDistance)
                {
                    nearestArrowDistance = distance;
                    nearestArrow = arrow;
                }
            }

            if (nearestArrow != null)
            {
                selectedMoveScreenAxis =
                    nearestArrow.screenAxis;

                selectedMoveAxis =
                    nearestArrow.screenAxis == Vector3.right
                    ? targetCamera.transform.right
                    : targetCamera.transform.up;

                draggingMoveArrow = true;
                previousMousePosition = mousePosition;
                return;
            }
        }

        // 先にhipなどの移動ハンドルを探す
        Ray ray =
            targetCamera.ScreenPointToRay(mousePosition);

        RaycastHit[] hits = Physics.RaycastAll(
            ray,
            100f,
            Physics.DefaultRaycastLayers,
            QueryTriggerInteraction.Collide);

        System.Array.Sort(
            hits,
            (a, b) => a.distance.CompareTo(b.distance));

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
                moveGizmoHandle = handle;
                moveGizmoVisible = true;

                SetMoveArrowRenderers(true);
                UpdateMoveArrowPositions();

                Debug.Log(
                    "GK移動矢印を表示しました: " + handle.name);

                return;
            }
        }

        // 回転リングを探す
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
            BeginRingDrag(mousePosition);
        }
    }

    // ------------------------------
    // ドラッグ開始
    // ------------------------------

    void BeginRingDrag(Vector2 mousePosition)
    {
        selectedWorldAxis =
            selectedRing.bone.TransformDirection(
                selectedRing.localAxis).normalized;

        rotationPlane = new Plane(
            selectedWorldAxis,
            selectedRing.bone.position);

        float axisViewDot = Mathf.Abs(
            Vector3.Dot(
                targetCamera.transform.forward,
                selectedWorldAxis));

        useWorldPlane = axisViewDot >= 0.15f;

        Ray ray =
            targetCamera.ScreenPointToRay(mousePosition);

        if (useWorldPlane)
        {
            if (rotationPlane.Raycast(ray, out float enter))
            {
                previousWorldVector =
                    ray.GetPoint(enter)
                    - selectedRing.bone.position;

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

        if (!useWorldPlane)
        {
            SaveRingScreenPoints();

            previousRingAngle =
                FindClosestRingAngle(
                    mousePosition, 0f, false);
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
                    selectedRing.line.GetPosition(i));

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
            Ray ray =
                targetCamera.ScreenPointToRay(mousePosition);

            if (!rotationPlane.Raycast(ray, out float enter))
            {
                return;
            }

            Vector3 currentWorldVector =
                ray.GetPoint(enter)
                - selectedRing.bone.position;

            if (currentWorldVector.sqrMagnitude < 0.0001f ||
                previousWorldVector.sqrMagnitude < 0.0001f)
            {
                previousWorldVector = currentWorldVector;
                return;
            }

            angle = Vector3.SignedAngle(
                previousWorldVector,
                currentWorldVector,
                selectedWorldAxis);

            previousWorldVector = currentWorldVector;
        }
        else
        {
            float currentRingAngle =
                FindClosestRingAngle(
                    mousePosition,
                    previousRingAngle,
                    true);

            angle = Mathf.DeltaAngle(
                previousRingAngle,
                currentRingAngle);

            if (selectedRing.localAxis == Vector3.right)
            {
                angle = -angle;
            }

            previousRingAngle = currentRingAngle;
        }

        angle *= rotationSensitivity;

        float targetAngle = Mathf.Clamp(
            selectedRing.currentAngle + angle,
            selectedRing.minAngle,
            selectedRing.maxAngle);

        float allowedAngle =
            targetAngle - selectedRing.currentAngle;

        if (Mathf.Abs(allowedAngle) > 0.0001f)
        {
            selectedRing.bone.Rotate(
                selectedWorldAxis,
                allowedAngle,
                Space.World);

            selectedRing.currentAngle = targetAngle;
        }

        previousMousePosition = mousePosition;
    }

    // ------------------------------
    // リング上の角度を計算
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

        int segmentCount =
            dragRingScreenPoints.Length - 1;

        for (int i = 0; i < segmentCount; i++)
        {
            Vector2 a = dragRingScreenPoints[i];
            Vector2 b = dragRingScreenPoints[i + 1];

            Vector2 segment = b - a;
            float t = 0f;

            if (segment.sqrMagnitude > 0.001f)
            {
                t = Mathf.Clamp01(
                    Vector2.Dot(
                        mousePosition - a,
                        segment) / segment.sqrMagnitude);
            }

            Vector2 closest = a + segment * t;

            float distance =
                Vector2.Distance(mousePosition, closest);

            float parameter = i + t;

            float candidateAngle = Mathf.Repeat(
                parameter / segmentCount * 360f,
                360f);

            float score = distance;

            if (useContinuity)
            {
                float angleDifference = Mathf.Abs(
                    Mathf.DeltaAngle(
                        referenceAngle,
                        candidateAngle));

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
                    ring.line.GetPosition(i));

            Vector3 worldB =
                ring.line.transform.TransformPoint(
                    ring.line.GetPosition(i + 1));

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
                new Vector2(screenB.x, screenB.y));

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
            Vector2.Dot(
                point - a,
                segment) / segment.sqrMagnitude);

        return Vector2.Distance(
            point,
            a + segment * t);
    }

    // ------------------------------
    // hipによる胴体の移動
    // ------------------------------

    void MoveBody(Vector2 mousePosition)
    {
        if (playerRoot == null || !draggingMoveArrow)
        {
            return;
        }

        Vector2 mouseDelta =
            mousePosition - previousMousePosition;

        Vector2 screenAxis =
            selectedMoveScreenAxis == Vector3.right
            ? Vector2.right
            : Vector2.up;

        float amount =
            Vector2.Dot(mouseDelta, screenAxis)
            * moveSensitivity;

        playerRoot.position += selectedMoveAxis * amount;

        previousMousePosition = mousePosition;

        UpdateMoveArrowPositions();
    }
}