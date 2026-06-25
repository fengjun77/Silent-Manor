using UnityEngine;

[CreateAssetMenu(menuName = "Data/Item", fileName = "Item")]
public class ItemData : ScriptableObject
{
    [Header("基础配置")]
    public int itemId;               // 唯一ID，不可重复
    public string itemName;
    public GameObject itemBasePrefab;// 物品通用UI预制体（挂载Item、CanvasGroup、Image）

    [Header("堆叠配置")]
    public int maxStackCount = 99;   // 最大堆叠数量
    public Sprite icon;
}
