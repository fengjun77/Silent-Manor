using Unity.VisualScripting;
using UnityEngine;

[System.Serializable]
public class DefaultInventoryItem
{
    public ItemData itemData;
    [Min(1)] public int stackCount = 1;
}

public class InventoryManager : Singleton<InventoryManager>
{
    [Header("背包配置")]
    public int inventorySlotCount = 24;
    public ItemData[] allItemDatabase;
    public UI_Inventory inventoryUI;

    public InventoryData InventoryData { get; private set; }

    public DefaultInventoryItem[] defaultInitItems; //初始物品表

    protected override void Awake()
    {
        base.Awake();
        InventoryData = new InventoryData(inventorySlotCount);
    }

    // 导出背包数据给SaveManager存档使用
    public InventorySaveData ExportInventorySaveData()
    {
        InventorySaveData save = new InventorySaveData();
        save.totalSlotCount = InventoryData.SlotTotal;
        save.slots = new SavedSlot[save.totalSlotCount];

        for (int i = 0; i < save.totalSlotCount; i++)
        {
            SlotData slot = InventoryData.GetSlot(i);
            save.slots[i] = new SavedSlot()
            {
                itemId = slot.itemId,
                stackCount = slot.stackCount
            };
        }
        return save;
    }

    // 读档时导入背包数据
    public void LoadFromSaveData(InventorySaveData saveData)
    {
        // 容错：存档格子数和当前背包不一致时截断
        int limit = Mathf.Min(saveData.slots.Length, InventoryData.SlotTotal);

        for (int i = 0; i < saveData.slots.Length; i++)
        {
            SavedSlot s = saveData.slots[i];
            InventoryData.SetSlot(i, new SlotData(s.itemId, s.stackCount));
        }
    }

    /// 加载默认物品，调用AddItem自动堆叠
    public void SpawnDefaultInventoryItems()
    {
        // 先清空背包，防止残留空数据干扰
        InventoryData.ClearAllSlots();

        foreach (var cfg in defaultInitItems)
        {
            if (cfg.itemData == null || cfg.stackCount <= 0) continue;
            // 自动堆叠、自动分配空格
            AddItem(cfg.itemData, cfg.stackCount);
        }
    }

    // 根据ID查找物品SO
    public ItemData GetItemById(int itemId)
    {
        foreach (var item in allItemDatabase)
        {
            if (item.itemId == itemId) return item;
        }
        return null;
    }

    // 自动添加物品，自动堆叠+自动空格
    // return 剩余未放入数量
    public int AddItem(ItemData item, int count)
    {
        return InventoryData.AddItem(item, count);
    }

    // 直接指定格子设置物品（用于初始化/手动放置）
    public void SetItemToSlot(int slotIndex, ItemData item, int stackNum)
    {
        if (item == null || stackNum <= 0) return;
        InventoryData.SetSlot(slotIndex, new SlotData(item.itemId, stackNum));
    }
}
