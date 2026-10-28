
using UnityEngine;
using UnityEngine.InputSystem;

public class GKPoseOnSpace : MonoBehaviour
{
    [Header("保存したポーズを適用する骨")]
    public Transform hip;
    public Transform rShldrBend;
    public Transform lShldrBend;
    public Transform rForearmBend;
    public Transform lForearmBend;
    public Transform lThighBend;
    public Transform lShin;
    public Transform rThighBend;
    public Transform rShin;

    private const string Prefix = "GKPose_";

    private bool poseApplied = false;

    void Update()
    {
        if (Keyboard.current != null &&
            Keyboard.current.spaceKey.wasPressedThisFrame &&
            !poseApplied)
        {
            ApplySavedPose();
        }
    }

    void ApplySavedPose()
    {
        bool hasSavedPose = PlayerPrefs.HasKey(
            Prefix + "hip_w"
        );

        if (!hasSavedPose)
        {
            Debug.LogWarning("保存したGKのポーズがありません！");
            return;
        }

        LoadBone("hip", hip);
        LoadBone("rShldrBend", rShldrBend);
        LoadBone("lShldrBend", lShldrBend);
        LoadBone("rForearmBend", rForearmBend);
        LoadBone("lForearmBend", lForearmBend);
        LoadBone("lThighBend", lThighBend);
        LoadBone("lShin", lShin);
        LoadBone("rThighBend", rThighBend);
        LoadBone("rShin", rShin);

        poseApplied = true;

        Debug.Log("保存したGKのポーズに切り替えました！");
    }

    void LoadBone(string boneName, Transform bone)
    {
        if (bone == null)
        {
            Debug.LogWarning(
                boneName + " がInspectorに設定されていません"
            );
            return;
        }

        string key = Prefix + boneName;

        if (!PlayerPrefs.HasKey(key + "_w"))
        {
            Debug.LogWarning(
                boneName + " の保存データがありません"
            );
            return;
        }

        Quaternion rot = new Quaternion(
            PlayerPrefs.GetFloat(key + "_x"),
            PlayerPrefs.GetFloat(key + "_y"),
            PlayerPrefs.GetFloat(key + "_z"),
            PlayerPrefs.GetFloat(key + "_w")
        );

        bone.localRotation = rot;
    }
}
