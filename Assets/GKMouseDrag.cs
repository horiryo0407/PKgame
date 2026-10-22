
using UnityEngine;
using UnityEngine.InputSystem;

public class GKMouseBoneDrag : MonoBehaviour
{
    public Camera targetCamera;

    [Header("腕・肘の回転")]
    public float sensitivity = 0.001f;
    public float maxRotationPerFrame = 1.5f;

    [Header("胴体の左右移動")]
    public Transform playerRoot;
    public float moveSensitivity = 0.01f;

    private GKBoneHandle selectedHandle;
    private Vector2 previousMousePosition;
    private float playerStartY;
    private float playerStartZ;

    void Start()
    {
        if (targetCamera == null)
        {
            targetCamera = Camera.main;
        }

        if (playerRoot != null)
        {
            playerStartY = playerRoot.position.y;
            playerStartZ = playerRoot.position.z;
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

        // クリックした部位を選択
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
                selectedHandle =
                    hit.collider.GetComponent<GKBoneHandle>();

                if (selectedHandle != null &&
                    selectedHandle.handleType !=
                    GKBoneHandle.HandleType.None)
                {
                    previousMousePosition = mousePosition;
                }
                else
                {
                    selectedHandle = null;
                }
            }
        }

        // 選択中の部位を操作
        if (Mouse.current.leftButton.isPressed &&
            selectedHandle != null)
        {
            Vector2 delta =
                mousePosition - previousMousePosition;

            if (selectedHandle.handleType ==
                GKBoneHandle.HandleType.Rotate)
            {
                RotateBone(selectedHandle.transform, delta);
            }
            else if (selectedHandle.handleType ==
                     GKBoneHandle.HandleType.MoveSideways)
            {
                MovePlayer(delta);
            }

            previousMousePosition = mousePosition;
        }

        // マウスを離したら選択解除
        if (Mouse.current.leftButton.wasReleasedThisFrame)
        {
            selectedHandle = null;
        }
    }

    void RotateBone(Transform bone, Vector2 delta)
    {
        if (delta.sqrMagnitude <= 0.01f)
        {
            return;
        }

        Vector3 boneDirection;

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
            return;
        }

        boneDirection.Normalize();

        Vector3 dragDirection =
            targetCamera.transform.right * delta.x
            + targetCamera.transform.up * delta.y;

        if (dragDirection.sqrMagnitude <= 0.0001f)
        {
            return;
        }

        float rotationAmount =
            Mathf.Min(
                dragDirection.magnitude * sensitivity,
                maxRotationPerFrame
            );

        Vector3 rotationAxis =
            Vector3.Cross(
                boneDirection,
                dragDirection.normalized
            );

        if (rotationAxis.sqrMagnitude > 0.0001f)
        {
            bone.Rotate(
                rotationAxis.normalized,
                rotationAmount,
                Space.World
            );
        }
    }

    void MovePlayer(Vector2 delta)
    {
        if (playerRoot == null)
        {
            return;
        }

        // 画面の左右方向だけを使う
        Vector3 right =
            targetCamera.transform.right;

        right.y = 0f;
        right.Normalize();

        Vector3 movement =
            right * delta.x * moveSensitivity;

        Vector3 newPosition =
            playerRoot.position + movement;

        // 高さと奥行きを固定
        newPosition.y = playerStartY;
        newPosition.z = playerStartZ;

        playerRoot.position = newPosition;
    }
}
