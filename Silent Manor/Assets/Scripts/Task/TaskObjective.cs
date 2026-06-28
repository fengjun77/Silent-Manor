using UnityEngine;

[System.Serializable]
public class TaskObjective
{
    [Header("目标描述（UI清单显示文本）")]
    public string descText;

    [Header("目标类型")]
    public ObjectiveType objType;

    [Header("对应道具/怪物唯一ID")]
    public int targetId;

    [Header("需要达成的数量")]
    public int needCount;
}
