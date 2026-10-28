
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
            LoadPose();
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
        // GK本体のワールド位置
        Vector3 pos = transform.position;
        PlayerPrefs.SetFloat(Prefix + "RootPos_x", pos.x);
        PlayerPrefs.SetFloat(Prefix + "RootPos_y", pos.y);
        PlayerPrefs.SetFloat(Prefix + "RootPos_z", pos.z);

        // hipのローカル位置
        if (hip != null)
        {
            Vector3 hipPos = hip.localPosition;
            PlayerPrefs.SetFloat(Prefix + "HipPos_x", hipPos.x);
            PlayerPrefs.SetFloat(Prefix + "HipPos_y", hipPos.y);
            PlayerPrefs.SetFloat(Prefix + "HipPos_z", hipPos.z);
        }

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
        Debug.Log("GKの位置とポーズを保存しました！");
    }

    public void LoadPose()
    {
        if (PlayerPrefs.HasKey(Prefix + "RootPos_x"))
        {
            transform.position = new Vector3(
                PlayerPrefs.GetFloat(Prefix + "RootPos_x"),
                PlayerPrefs.GetFloat(Prefix + "RootPos_y"),
                PlayerPrefs.GetFloat(Prefix + "RootPos_z")
            );
        }

        if (hip != null &&
            PlayerPrefs.HasKey(Prefix + "HipPos_x"))
        {
            hip.localPosition = new Vector3(
                PlayerPrefs.GetFloat(Prefix + "HipPos_x"),
                PlayerPrefs.GetFloat(Prefix + "HipPos_y"),
                PlayerPrefs.GetFloat(Prefix + "HipPos_z")
            );
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

        Debug.Log("GKの位置とポーズを読み込みました！");
    }

    void SaveBone(string boneName, Transform bone)
    {
        if (bone == null)
        {
            Debug.LogWarning(boneName + " が設定されていません");
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
            return;

        string key = Prefix + boneName;

        if (!PlayerPrefs.HasKey(key + "_w"))
            return;

        bone.localRotation = new Quaternion(
            PlayerPrefs.GetFloat(key + "_x"),
            PlayerPrefs.GetFloat(key + "_y"),
            PlayerPrefs.GetFloat(key + "_z"),
            PlayerPrefs.GetFloat(key + "_w")
        );
    }
}
