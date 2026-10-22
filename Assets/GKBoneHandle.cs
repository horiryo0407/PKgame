
using UnityEngine;

public class GKBoneHandle : MonoBehaviour
{
    public enum HandleType
    {
        None,       // 操作しない
        Rotate,     // 腕・肘などを回転
        MoveSideways // 胴体を左右移動
    }

    [Header("操作タイプ")]
    public HandleType handleType = HandleType.None;
}
