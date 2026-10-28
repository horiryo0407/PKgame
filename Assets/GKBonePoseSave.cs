
using UnityEngine;
using UnityEngine.InputSystem;

public class GKBonePoseSave : MonoBehaviour
{
    [Header("保存する関節")]
    public Transform hip;
    public Transform rShldrBend;
    public Transform lShldrBend;
    public Transform rForearmBend;
    public Transform lForearmBend;
    public Transform lThighBend;
    public Transform lShin;
    public Transform rThighBend;
    public Transform rShin;

    [Header("設定")]
    public bool loadPoseOnStart = true;

    private const string Prefix = "GKPose_";

    void Start()
    {
        if (loadPoseOnStart)
        {
            LoadPose();
        }
    }

    void Update()
    {
        if (Keyboard.current != null &&
            Keyboard.current.pKey.wasPressedThisFrame)
        {
            SavePose();
        }
    }

    public void SavePose()
    {
        SaveBone("hip", hip);
        SaveBone("rShldrBend", rShldrBend);
        SaveBone("lShldrBend", lShldrBend);
        SaveBone("rForearmBend", rForearmBend);
        SaveBone("lForearmBend", lForearmBend);
        SaveBone("lThighBend", lThighBend);
        SaveBone("lShin", lShin);
        SaveBone("rThighBend", rThighBend);
        SaveBone("rShin", rShin);

        PlayerPrefs.Save();

        Debug.Log("GKのポーズを保存しました！");
    }

    public void LoadPose()
    {
        LoadBone("hip", hip);
        LoadBone("rShldrBend", rShldrBend);
        LoadBone("lShldrBend", lShldrBend);
        LoadBone("rForearmBend", rForearmBend);
        LoadBone("lForearmBend", lForearmBend);
        LoadBone("lThighBend", lThighBend);
        LoadBone("lShin", lShin);
        LoadBone("rThighBend", rThighBend);
        LoadBone("rShin", rShin);

        Debug.Log("GKのポーズを読み込みました！");
    }

    void SaveBone(string boneName, Transform bone)
    {
        if (bone == null)
        {
            Debug.LogWarning(
                boneName + " がInspectorに設定されていません"
            );
            return;
        }

        Quaternion rot = bone.localRotation;
        string key = Prefix + boneName;

        PlayerPrefs.SetFloat(key + "_x", rot.x);
        PlayerPrefs.SetFloat(key + "_y", rot.y);
        PlayerPrefs.SetFloat(key + "_z", rot.z);
        PlayerPrefs.SetFloat(key + "_w", rot.w);
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
