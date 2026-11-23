using System.Collections;
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

    private Coroutine poseCopyCoroutine;

    void Awake()
    {
        initialRootPosition = transform.position;
        initialRootRotation = transform.rotation;

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

        Debug.Log(gameObject.name + "の初期ポーズを記録しました！");
    }

    public void ResetToInitialPose()
    {
        if (poseCopyCoroutine != null)
        {
            StopCoroutine(poseCopyCoroutine);
            poseCopyCoroutine = null;
        }

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

        Debug.Log(gameObject.name + "を初期ポーズに戻しました！");
    }

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

        if (hip != null && PlayerPrefs.HasKey(Prefix + "HipPos_x"))
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

        Debug.Log(gameObject.name + "が保存ポーズを読み込みました！");
    }

    // 即時コピー（既存機能を残す）
    public void CopyPoseFrom(GKBonePoseSave source)
    {
        if (source == null)
        {
            Debug.LogError("コピー元のPose Saveがありません！");
            return;
        }

        if (hip != null && source.hip != null)
        {
            hip.localPosition = source.hip.localPosition;
            hip.localRotation = source.hip.localRotation;
        }

        CopyBone(source.rShldrBend, rShldrBend);
        CopyBone(source.lShldrBend, lShldrBend);
        CopyBone(source.rForearmBend, rForearmBend);
        CopyBone(source.lForearmBend, lForearmBend);
        CopyBone(source.lThighBend, lThighBend);
        CopyBone(source.lShin, lShin);
        CopyBone(source.rThighBend, rThighBend);
        CopyBone(source.rShin, rShin);

        Debug.Log("Didaのポーズを" + gameObject.name + "にコピーしました！");
    }

    // 指定時間をかけて滑らかにコピー
    public void SmoothCopyPoseFrom(
        GKBonePoseSave source,
        float duration)
    {
        if (source == null)
        {
            Debug.LogError("コピー元のPose Saveがありません！");
            return;
        }

        if (poseCopyCoroutine != null)
        {
            StopCoroutine(poseCopyCoroutine);
        }

        poseCopyCoroutine =
            StartCoroutine(SmoothCopyCoroutine(source, duration));
    }

    private IEnumerator SmoothCopyCoroutine(
        GKBonePoseSave source,
        float duration)
    {
        Transform[] targetBones = GetBones();
        Transform[] sourceBones = source.GetBones();

        Quaternion[] startRotations =
            new Quaternion[targetBones.Length];

        Quaternion[] targetRotations =
            new Quaternion[targetBones.Length];

        for (int i = 0; i < targetBones.Length; i++)
        {
            if (targetBones[i] != null && sourceBones[i] != null)
            {
                startRotations[i] = targetBones[i].localRotation;
                targetRotations[i] = sourceBones[i].localRotation;
            }
        }

        Vector3 startHipPosition =
            hip != null ? hip.localPosition : Vector3.zero;

        Vector3 targetHipPosition =
            source.hip != null
                ? source.hip.localPosition
                : startHipPosition;

        float elapsed = 0f;
        duration = Mathf.Max(0.01f, duration);

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;

            float t = Mathf.Clamp01(elapsed / duration);
            float smoothT = Mathf.SmoothStep(0f, 1f, t);

            if (hip != null && source.hip != null)
            {
                hip.localPosition = Vector3.Lerp(
                    startHipPosition,
                    targetHipPosition,
                    smoothT
                );
            }

            for (int i = 0; i < targetBones.Length; i++)
            {
                if (targetBones[i] == null ||
                    sourceBones[i] == null)
                {
                    continue;
                }

                targetBones[i].localRotation =
                    Quaternion.Slerp(
                        startRotations[i],
                        targetRotations[i],
                        smoothT
                    );
            }

            yield return null;
        }

        if (hip != null && source.hip != null)
            hip.localPosition = targetHipPosition;

        for (int i = 0; i < targetBones.Length; i++)
        {
            if (targetBones[i] != null &&
                sourceBones[i] != null)
            {
                targetBones[i].localRotation =
                    targetRotations[i];
            }
        }

        poseCopyCoroutine = null;

        Debug.Log(
            "Didaのポーズを" + gameObject.name +
            "へ滑らかに反映しました！"
        );
    }

    private Transform[] GetBones()
    {
        return new Transform[]
        {
            hip,
            rShldrBend,
            lShldrBend,
            rForearmBend,
            lForearmBend,
            lThighBend,
            lShin,
            rThighBend,
            rShin
        };
    }

    private void CopyBone(Transform source, Transform target)
    {
        if (source == null || target == null)
            return;

        target.localRotation = source.localRotation;
    }

    private void SaveBone(string boneName, Transform bone)
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

    private void LoadBone(string boneName, Transform bone)
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