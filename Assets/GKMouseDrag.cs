
using UnityEngine;
using UnityEngine.InputSystem;

public class GKMouseBoneDrag : MonoBehaviour
{
    public Camera targetCamera;

    [Header("マウス操作")]
    public float sensitivity = 0.001f;
    public float maxRotationPerFrame = 1.5f;

    private GKBoneHandle selectedBone;
    private Vector2 previousMousePosition;

    void Start()
    {
        if (targetCamera == null)
        {
            targetCamera = Camera.main;
        }
    }

    void Update()
    {
        if (Mouse.current == null || targetCamera == null)
        {
            return;
        }

        Vector2 mousePosition =
            Mouse.current.position.ReadValue();

        // ボーンをクリックしてつかむ
        if (Mouse.current.leftButton.wasPressedThisFrame)
        {
            Ray ray =
                targetCamera.ScreenPointToRay(mousePosition);

            if (Physics.Raycast(
                ray,
                out RaycastHit hit,
                100f,
                Physics.DefaultRaycastLayers,
                QueryTriggerInteraction.Collide))
            {
                selectedBone =
                    hit.collider.GetComponent<GKBoneHandle>();

                if (selectedBone != null)
                {
                    previousMousePosition = mousePosition;
                }
            }
        }

        // マウスを動かして前腕を回転させる
        if (Mouse.current.leftButton.isPressed &&
            selectedBone != null)
        {
            Vector2 delta =
                mousePosition - previousMousePosition;

            if (delta.sqrMagnitude > 0.01f)
            {
                Transform bone = selectedBone.transform;

                Vector3 boneDirection;

                // 前腕の向きを取得
                if (bone.childCount > 0)
                {
                    boneDirection =
                        bone.GetChild(0).position - bone.position;
                }
                else if (bone.parent != null)
                {
                    boneDirection =
                        bone.position - bone.parent.position;
                }
                else
                {
                    previousMousePosition = mousePosition;
                    return;
                }

                boneDirection.Normalize();

                // マウスの移動方向をカメラ基準で計算
                Vector3 dragDirection =
                    targetCamera.transform.right * delta.x
                    + targetCamera.transform.up * delta.y;

                // 1回の移動で回転する角度を制限
                float rotationAmount =
                    Mathf.Min(
                        dragDirection.magnitude * sensitivity,
                        maxRotationPerFrame
                    );

                if (dragDirection.sqrMagnitude > 0.0001f)
                {
                    Vector3 rotationAxis =
                        Vector3.Cross(
                            boneDirection,
                            dragDirection.normalized
                        );

                    if (rotationAxis.sqrMagnitude > 0.0001f)
                    {
                        rotationAxis.Normalize();

                        bone.Rotate(
                            rotationAxis,
                            rotationAmount,
                            Space.World
                        );
                    }
                }
            }

            previousMousePosition = mousePosition;
        }

        // マウスを離したら操作終了
        if (Mouse.current.leftButton.wasReleasedThisFrame)
        {
            selectedBone = null;
        }
    }
}
