using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UI_TaskObjectiveItem : MonoBehaviour
{
    [Header("组件绑定")]
    public TMP_Text txtDesc;
    public TMP_Text txtProgress;

    /// 收集/击杀目标渲染
    public void SetData(string desc, int current, int need, bool finished)
    {
        txtDesc.text = desc;
        txtProgress.text = $"{current}/{need}";
    }

    /// 道具提交目标渲染（仅交付时校验背包）
    public void SetSubmitItemData(string desc, bool enough)
    {
        txtDesc.text = desc;
        txtProgress.text = "交付NPC时校验背包";
    }
}
