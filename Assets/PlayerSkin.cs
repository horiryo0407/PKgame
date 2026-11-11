using UnityEngine;

public class PlayerSkin : MonoBehaviour
{
    [SerializeField] private Renderer[] targetRenderers; // 色を変える
    [SerializeField] private int materialIndex = 0;      // ユニフォームの番号
    [SerializeField] private Material player1Material;   // 1P用
    [SerializeField] private Material player2Material;   // 2P用

    public void Apply(int playerNumber)
    {
        Material mat = playerNumber == 1 ? player1Material : player2Material;

        foreach (Renderer r in targetRenderers)
        {
            Material[] mats = r.sharedMaterials;
            if (materialIndex < mats.Length)
            {
                mats[materialIndex] = mat;
                r.sharedMaterials = mats;
            }
        }
    }
}