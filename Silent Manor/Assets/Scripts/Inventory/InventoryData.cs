using System;
using UnityEngine;

// 单个格子数据：存储物品ID+堆叠数量
[Serializable]
public struct SlotData
{
    public int itemId;
    public int stackCount;

    public SlotData(int id = -1, int count = 0)
    {
        itemId = id;
        stackCount = count;
    }

    // 判断格子是否为空
    public bool IsEmpty() => itemId == -1 || stackCount <= 0;
}

/// <summary>纯数据背包，所有修改都会触发更新事件</summary>
public class InventoryData
{
    private SlotData[] _slots;
    public int SlotTotal => _slots.Length;

    public InventoryData(int slotAmount)
    {
        _slots = new SlotData[slotAmount];
        for (int i = 0; i < slotAmount; i++)
        {
            _slots[i] = new SlotData(-1, 0);
        }
    }

    /// <summary>
    /// 获取目标位置格子信息
    /// </summary>
    /// <param name="index"></param>
    /// <returns></returns>
    public SlotData GetSlot(int index)
    {
        if (!CheckIndexValid(index)) return new SlotData(-1, 0);
        return _slots[index];
    }

    /// <summary>
    /// 设置目标位置格子信息
    /// </summary>
    /// <param name="index"></param>
    /// <param name="data"></param>
    public void SetSlot(int index, SlotData data)
    {
        if (!CheckIndexValid(index)) return;
        _slots[index] = data;
        TriggerUpdate();
    }

    /// <summary>
    /// 互换格子信息
    /// </summary>
    /// <param name="slotA"></param>
    /// <param name="slotB"></param>
    public void SwapSlot(int slotA, int slotB)
    {
        if (!CheckIndexValid(slotA) || !CheckIndexValid(slotB) || slotA == slotB)
            return;

        SlotData temp = _slots[slotA];
        _slots[slotA] = _slots[slotB];
        _slots[slotB] = temp;

        TriggerUpdate();
    }

    /// <summary>
    /// 清除格子信息
    /// </summary>
    /// <param name="index"></param>
    public void ClearSlot(int index)
    {
        if (!CheckIndexValid(index)) return;
        _slots[index] = new SlotData(-1, 0);
        TriggerUpdate();
    }

    /// <summary>
    /// 清空所有格子信息
    /// </summary>
    public void ClearAllSlots()
    {
        for (int i = 0; i < _slots.Length; i++)
        {
            _slots[i] = new SlotData(-1, 0);
        }
        TriggerUpdate();
    }

    public int AddItem(ItemData item, int addCount)
    {
        if(item == null || addCount <= 0) return 0;

        int remain = addCount;
        int maxStack = item.maxStackCount;
        int targetId = item.itemId;

        //第一遍遍历，优先给已有物品进行堆叠
        for(int i = 0; i < SlotTotal; i++)
        {
            if(remain <= 0) break;

            //获取当前格子信息
            SlotData slot = GetSlot(i);
            if(slot.itemId != targetId) continue;
            if(slot.stackCount >= maxStack) continue;

            int canAdd = maxStack - slot.stackCount;
            int put = Math.Min(canAdd, remain);

            SlotData newSlot = slot;
            newSlot.stackCount += put;

            remain -= put;
        }

        // 第二步：还有剩余，找空格子新建堆叠
        for (int i = 0; i < SlotTotal; i++)
        {
            if (remain <= 0) break;

            SlotData slot = GetSlot(i);
            if (!slot.IsEmpty()) continue;

            int put = Math.Min(maxStack, remain);
            SetSlot(i, new SlotData(targetId, put));
            remain -= put;
        }

        // 剩余大于0代表背包装满，没放进去
        return remain;
    }

    // 获取第一个空格索引
    public int GetFirstEmptySlotIndex()
    {
        for (int i = 0; i < SlotTotal; i++)
        {
            if (GetSlot(i).IsEmpty())
                return i;
        }
        return -1;
    }

    // 统一调用全局事件中心，不再内部事件
    private void TriggerUpdate()
    {
        EventCenter.CallInventoryChangedEvent();
    }

    /// <summary>
    /// 检查所有合法性
    /// </summary>
    /// <param name="idx"></param>
    /// <returns></returns>
    private bool CheckIndexValid(int idx)
    {
        return idx >= 0 && idx < _slots.Length;
    }
}
