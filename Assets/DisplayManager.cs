
using UnityEngine;

public class DisplayManager : MonoBehaviour
{
    private void Start()
    {
        // 2台目のモニターが接続されているか確認
        if (Display.displays.Length > 1)
        {
            // 2台目のモニターを有効化
            Display.displays[1].Activate();

            Debug.Log("Display 2 を有効化しました！");
        }
        else
        {
            Debug.LogWarning("2台目のモニターが見つかりません！");
        }
    }
}
