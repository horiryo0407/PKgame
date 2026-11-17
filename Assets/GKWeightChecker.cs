
using UnityEngine;

public class GKWeightChecker : MonoBehaviour
{
    public SkinnedMeshRenderer original;
    public SkinnedMeshRenderer body;

    [ContextMenu("BindPoseを比較")]
    public void CompareBindPoses()
    {
        if (original == null || body == null)
        {
            Debug.LogError("OriginalとBodyを設定してください。");
            return;
        }

        Mesh originalMesh = original.sharedMesh;
        Mesh bodyMesh = body.sharedMesh;

        if (originalMesh == null || bodyMesh == null)
        {
            Debug.LogError("Meshが設定されていません。");
            return;
        }

        Matrix4x4[] originalPoses = originalMesh.bindposes;
        Matrix4x4[] bodyPoses = bodyMesh.bindposes;

        Transform[] originalBones = original.bones;
        Transform[] bodyBones = body.bones;

        Debug.Log("===== BindPose比較開始 =====");
        Debug.Log("元BASE BindPose数：" + originalPoses.Length);
        Debug.Log("Body BindPose数：" + bodyPoses.Length);
        Debug.Log("元BASE Bones数：" + originalBones.Length);
        Debug.Log("Body Bones数：" + bodyBones.Length);

        int count = Mathf.Min(
            originalPoses.Length,
            bodyPoses.Length,
            originalBones.Length,
            bodyBones.Length
        );

        int differentCount = 0;

        for (int i = 0; i < count; i++)
        {
            bool sameBone = originalBones[i] == bodyBones[i];

            bool samePose = MatrixApproximatelyEqual(
                originalPoses[i],
                bodyPoses[i]
            );

            if (!sameBone || !samePose)
            {
                differentCount++;

                Debug.LogWarning(
                    "Index " + i +
                    " / 元BASE Bone：" + originalBones[i].name +
                    " / Body Bone：" + bodyBones[i].name +
                    " / Bone一致：" + sameBone +
                    " / BindPose一致：" + samePose
                );
            }
        }

        Debug.Log(
            "比較した数：" + count +
            " / 不一致数：" + differentCount
        );

        Debug.Log("===== BindPose比較終了 =====");
    }

    private bool MatrixApproximatelyEqual(
        Matrix4x4 a,
        Matrix4x4 b)
    {
        for (int i = 0; i < 16; i++)
        {
            if (Mathf.Abs(a[i] - b[i]) > 0.0001f)
                return false;
        }

        return true;
    }
}
