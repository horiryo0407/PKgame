using UnityEngine;

public class TurnSkinApplier : MonoBehaviour
{
    void Start()
    {
        TurnSkinManager skin = FindAnyObjectByType<TurnSkinManager>();
        if (skin != null)
        {
            skin.ApplyTurn(SceneTransition.CurrentPlayer + 1);
        }
    }
}