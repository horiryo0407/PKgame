
using UnityEngine;

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

    private const string Prefix = "GKPose_";

    // 初期状態の保存用
    private Vector3 initialRootPosition;
    private Quaternion initialRootRotation;
    private Vector3 initialHipPosition;

    private Quaternion initialHipRotation;
    private Quaternion initialRShldrRotation;
    private Quaternion initialLShldrRotation;
    private Quaternion initialRForearmRotation;
    private Quaternion initialLForearmRotation;
    private Quaternion initialLThighRotation;
    private Quaternion initialLShinRotation;
    private Quaternion initialRThighRotation;
    private Quaternion initialRShinRotation;

    void Awake()
    {
        // ゲーム開始時の位置を記録
        initialRootPosition = transform.position;
        initialRootRotation = transform.rotation;

        // 関節の初期状態を記録
        if (hip != null)
        {
            initialHipPosition = hip.localPosition;
            initialHipRotation = hip.localRotation;
        }

        if (rShldrBend != null)
            initialRShldrRotation = rShldrBend.localRotation;

        if (lShldrBend != null)
            initialLShldrRotation = lShldrBend.localRotation;

        if (rForearmBend != null)
            initialRForearmRotation = rForearmBend.localRotation;

        if (lForearmBend != null)
            initialLForearmRotation = lForearmBend.localRotation;

        if (lThighBend != null)
            initialLThighRotation = lThighBend.localRotation;

        if (lShin != null)
            initialLShinRotation = lShin.localRotation;

        if (rThighBend != null)
            initialRThighRotation = rThighBend.localRotation;

        if (rShin != null)
            initialRShinRotation = rShin.localRotation;

        Debug.Log("Didaの初期位置・ポーズを記録しました！");
    }

    // 初期状態に戻す
    public void ResetToInitialPose()
    {
        transform.position = initialRootPosition;
        transform.rotation = initialRootRotation;

        if (hip != null)
        {
            hip.localPosition = initialHipPosition;
            hip.localRotation = initialHipRotation;
        }

        if (rShldrBend != null)
            rShldrBend.localRotation = initialRShldrRotation;

        if (lShldrBend != null)
            lShldrBend.localRotation = initialLShldrRotation;

        if (rForearmBend != null)
            rForearmBend.localRotation = initialRForearmRotation;

        if (lForearmBend != null)
            lForearmBend.localRotation = initialLForearmRotation;

        if (lThighBend != null)
            lThighBend.localRotation = initialLThighRotation;

        if (lShin != null)
            lShin.localRotation = initialLShinRotation;

        if (rThighBend != null)
            rThighBend.localRotation = initialRThighRotation;

        if (rShin != null)
            rShin.localRotation = initialRShinRotation;

        Debug.Log("Didaを初期位置・初期ポーズに戻しました！");
    }

    // GKの位置とポーズを保存
    public void SavePose()
    {
        Debug.Log("SavePose実行：" + gameObject.name);
        Vector3 pos = transform.position;

        PlayerPrefs.SetFloat(Prefix + "RootPos_x", pos.x);
        PlayerPrefs.SetFloat(Prefix + "RootPos_y", pos.y);
        PlayerPrefs.SetFloat(Prefix + "RootPos_z", pos.z);

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

        Debug.Log("GKポーズ保存実行：" + Time.frameCount);
    }

    // 保存したポーズを読み込む
    // loadRootPosition=falseなら本体の位置は変更しない
    public void LoadPose(bool loadRootPosition = true)
    {
        // 本体の位置は必要な場合だけ読み込む
        if (loadRootPosition &&
            PlayerPrefs.HasKey(Prefix + "RootPos_x"))
        {
            transform.position = new Vector3(
                PlayerPrefs.GetFloat(Prefix + "RootPos_x"),
                PlayerPrefs.GetFloat(Prefix + "RootPos_y"),
                PlayerPrefs.GetFloat(Prefix + "RootPos_z")
            );
        }

        // 腰の位置
        if (hip != null && PlayerPrefs.HasKey(Prefix + "HipPos_x"))
        {
            hip.localPosition = new Vector3(
                PlayerPrefs.GetFloat(Prefix + "HipPos_x"),
                PlayerPrefs.GetFloat(Prefix + "HipPos_y"),
                PlayerPrefs.GetFloat(Prefix + "HipPos_z")
            );
        }

        // 関節の回転
        LoadBone("hip", hip);
        LoadBone("rShldrBend", rShldrBend);
        LoadBone("lShldrBend", lShldrBend);
        LoadBone("rForearmBend", rForearmBend);
        LoadBone("lForearmBend", lForearmBend);
        LoadBone("lThighBend", lThighBend);
        LoadBone("lShin", lShin);
        LoadBone("rThighBend", rThighBend);
        LoadBone("rShin", rShin);

        Debug.Log("保存したGKポーズを読み込みました！");
    }

    void SaveBone(string boneName, Transform bone)
    {
        if (bone == null)
            return;

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
