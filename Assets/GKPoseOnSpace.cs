
using System.Collections;
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

    [Header("動きの設定")]
    public float moveDuration = 0.5f;

    private const string Prefix = "GKPose_";
    private bool poseApplied = false;

    void Update()
    {
        if (poseApplied || Keyboard.current == null)
            return;

        if (Keyboard.current.spaceKey.wasPressedThisFrame)
        {
            if (PlayerPrefs.HasKey(Prefix + "RootPos_x") &&
                PlayerPrefs.HasKey(Prefix + "hip_w"))
            {
                StartCoroutine(MoveToSavedPose());
            }
            else
            {
                Debug.LogWarning(
                    "保存データがありません。GKPoseSettingでPキーを押してください。"
                );
            }
        }
    }

    IEnumerator MoveToSavedPose()
    {
        poseApplied = true;

        Vector3 startPosition = transform.position;

        Vector3 targetPosition = new Vector3(
            PlayerPrefs.GetFloat(Prefix + "RootPos_x"),
            PlayerPrefs.GetFloat(Prefix + "RootPos_y"),
            PlayerPrefs.GetFloat(Prefix + "RootPos_z")
        );

        Vector3 startHipPosition = hip != null
            ? hip.localPosition
            : Vector3.zero;

        Vector3 targetHipPosition = new Vector3(
            PlayerPrefs.GetFloat(Prefix + "HipPos_x"),
            PlayerPrefs.GetFloat(Prefix + "HipPos_y"),
            PlayerPrefs.GetFloat(Prefix + "HipPos_z")
        );

        Transform[] bones = GetBones();
        Quaternion[] startRotations = GetRotations(bones);
        Quaternion[] targetRotations = GetSavedRotations(bones);

        float elapsed = 0f;
        float duration = Mathf.Max(0.01f, moveDuration);

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;

            float t = Mathf.Clamp01(elapsed / duration);
            float smoothT = Mathf.SmoothStep(0f, 1f, t);

            // GK本体の位置
            transform.position = Vector3.Lerp(
                startPosition, targetPosition, smoothT
            );

            // hipの位置
            if (hip != null &&
                PlayerPrefs.HasKey(Prefix + "HipPos_x"))
            {
                hip.localPosition = Vector3.Lerp(
                    startHipPosition, targetHipPosition, smoothT
                );
            }

            // 関節の回転
            for (int i = 0; i < bones.Length; i++)
            {
                if (bones[i] != null)
                {
                    bones[i].localRotation = Quaternion.Slerp(
                        startRotations[i],
                        targetRotations[i],
                        smoothT
                    );
                }
            }

            yield return null;
        }

        // 最後に保存した値へ正確に合わせる
        transform.position = targetPosition;

        if (hip != null &&
            PlayerPrefs.HasKey(Prefix + "HipPos_x"))
        {
            hip.localPosition = targetHipPosition;
        }

        for (int i = 0; i < bones.Length; i++)
        {
            if (bones[i] != null)
                bones[i].localRotation = targetRotations[i];
        }

        Debug.Log("GKが保存した位置とポーズに移動しました！");
    }

    Transform[] GetBones()
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

    Quaternion[] GetRotations(Transform[] bones)
    {
        Quaternion[] rotations = new Quaternion[bones.Length];

        for (int i = 0; i < bones.Length; i++)
        {
            rotations[i] = bones[i] != null
                ? bones[i].localRotation
                : Quaternion.identity;
        }

        return rotations;
    }

    Quaternion[] GetSavedRotations(Transform[] bones)
    {
        Quaternion[] rotations = new Quaternion[bones.Length];

        for (int i = 0; i < bones.Length; i++)
        {
            rotations[i] = bones[i] != null
                ? GetSavedRotation(bones[i].name)
                : Quaternion.identity;
        }

        return rotations;
    }

    Quaternion GetSavedRotation(string boneName)
    {
        string key = Prefix + boneName;

        if (!PlayerPrefs.HasKey(key + "_w"))
            return Quaternion.identity;

        return new Quaternion(
            PlayerPrefs.GetFloat(key + "_x"),
            PlayerPrefs.GetFloat(key + "_y"),
            PlayerPrefs.GetFloat(key + "_z"),
            PlayerPrefs.GetFloat(key + "_w")
        );
    }
}
