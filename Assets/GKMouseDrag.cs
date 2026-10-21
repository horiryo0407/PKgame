
using UnityEngine;
using UnityEngine.InputSystem;

public class GKMouseBoneDrag : MonoBehaviour
{
    public Camera targetCamera;
    public float sensitivity = 0.5f;

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
            return;

        Vector2 mousePosition = Mouse.current.position.ReadValue();

        if (Mouse.current.leftButton.wasPressedThisFrame)
        {
            Ray ray = targetCamera.ScreenPointToRay(mousePosition);

            if (Physics.Raycast(
                ray, out RaycastHit hit, 100f,
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

        if (Mouse.current.leftButton.isPressed &&
            selectedBone != null)
        {
            Vector2 delta = mousePosition - previousMousePosition;

            Transform bone = selectedBone.transform;

            bone.Rotate(
                Vector3.up,
                -delta.x * sensitivity,
                Space.Self
            );

            bone.Rotate(
                Vector3.right,
                delta.y * sensitivity,
                Space.Self
            );

            previousMousePosition = mousePosition;
        }

        if (Mouse.current.leftButton.wasReleasedThisFrame)
        {
            selectedBone = null;
        }
    }
}
