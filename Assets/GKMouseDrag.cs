
using UnityEngine;
using UnityEngine.InputSystem;

public class GKMouseBoneDrag : MonoBehaviour
{
    public Camera targetCamera;
    public float sensitivity = 0.15f;

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

        Vector2 mousePosition = Mouse.current.position.ReadValue();

        // ボーンをクリックして選択
        if (Mouse.current.leftButton.wasPressedThisFrame)
        {
            Ray ray = targetCamera.ScreenPointToRay(mousePosition);

            if (Physics.Raycast(
                ray,
                out RaycastHit hit,
                100f,
                Physics.DefaultRaycastLayers,
                QueryTriggerInteraction.Collide))
            {
                selectedBone = hit.collider.GetComponent<GKBoneHandle>();

                if (selectedBone != null)
                {
                    previousMousePosition = mousePosition;
                }
            }
        }

        // ドラッグ中にボーンを回転
        if (Mouse.current.leftButton.isPressed && selectedBone != null)
        {
            Vector2 delta = mousePosition - previousMousePosition;

            if (delta.sqrMagnitude > 0.01f)
            {
                Transform bone = selectedBone.transform;

                Vector3 boneDirection = bone.childCount > 0
                    ? bone.GetChild(0).position - bone.position
                    : bone.up;

                if (boneDirection.sqrMagnitude > 0.0001f)
                {
                    boneDirection.Normalize();

                    // マウスの移動方向をカメラ基準で取得
                    Vector3 dragDirection =
                        targetCamera.transform.right * delta.x
                        + targetCamera.transform.up * delta.y;

                    Vector3 rotationAxis =
                        Vector3.Cross(boneDirection, dragDirection);

                    if (rotationAxis.sqrMagnitude > 0.0001f)
                    {
                        rotationAxis.Normalize();

                        float angle = delta.magnitude * sensitivity;

                        bone.Rotate(
                            rotationAxis,
                            angle,
                            Space.World
                        );
                    }
                }
            }

            previousMousePosition = mousePosition;
        }

        // マウスを離したら選択解除
        if (Mouse.current.leftButton.wasReleasedThisFrame)
        {
            selectedBone = null;
        }
    }
}
