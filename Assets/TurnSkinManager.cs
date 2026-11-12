using UnityEngine;

public class TurnSkinManager : MonoBehaviour
{
    [SerializeField] private PlayerSkin shooterSkin;
    [SerializeField] private PlayerSkin keeperSkin;

    // shooterPlayerNumber　今のシューターが1Pなら1になるし、2Pなら2になる
    public void ApplyTurn(int shooterPlayerNumber)
    {
        int keeperPlayerNumber = shooterPlayerNumber == 1 ? 2 : 1;
        if (shooterSkin != null) shooterSkin.Apply(shooterPlayerNumber);
        if (keeperSkin != null) keeperSkin.Apply(keeperPlayerNumber);
    }

    [ContextMenu("Test Shooter=1P")]
    private void TestP1() { ApplyTurn(1); }

    [ContextMenu("Test Shooter=2P")]
    private void TestP2() { ApplyTurn(2); }
}


