using System.Threading;
using UnityEngine;
using UnityEngine.UI;

public class TipManager : Singleton<TipManager>
{
    [Header("提示弹窗配置")]
    public Transform tipParent; // 左下角UI父物体
    public GameObject itemTipPrefab;

    /// 弹出获得物品提示
    public void ShowGetItemTip(ItemData item, int count)
    {
        if (item == null || count <= 0) return;
        GameObject tipObj = Instantiate(itemTipPrefab, tipParent);
        UI_Tip tip = tipObj.GetComponent<UI_Tip>();
        tip.Setup(item, count);
    }
}
