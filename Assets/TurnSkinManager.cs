
using UnityEngine;

public class TurnSkinManager : MonoBehaviour
{
    [Header("キッカー")]
    [SerializeField] private PlayerSkin shooterSkin;

    [Header("GK本体")]
    [SerializeField] private PlayerSkin keeperSkin;

    [Header("キッカー画面側のGK")]
    [SerializeField] private PlayerSkin kkKeeperSkin;

    public void ApplyTurn(int shooterPlayerNumber)
    {
        int keeperPlayerNumber = shooterPlayerNumber == 1 ? 2 : 1;

        // キッカーの色
        if (shooterSkin != null)
            shooterSkin.Apply(shooterPlayerNumber);

        // GK本体の色
        if (keeperSkin != null)
            keeperSkin.Apply(keeperPlayerNumber);

        // キッカー画面側のGKの色
        if (kkKeeperSkin != null)
            kkKeeperSkin.Apply(keeperPlayerNumber);
    }

    [ContextMenu("Test Shooter=1P")]
    private void TestP1()
    {
        ApplyTurn(1);
    }

    [ContextMenu("Test Shooter=2P")]
    private void TestP2()
    {
        ApplyTurn(2);
    }
}
