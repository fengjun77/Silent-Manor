using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UI_Inventory : MonoBehaviour
{
    [Header("格子生成配置")]
    public Transform slotParent;
    public GameObject slotPrefab;
    private UI_Slot[] _slotUIs; // 所有生成了的格子

    void Start()
    {
        int totalSlots = InventoryManager.Instance.inventorySlotCount;
        _slotUIs = new UI_Slot[totalSlots];

        for (int i = 0; i < totalSlots; i++)
        {
            GameObject slotObj = Instantiate(slotPrefab, slotParent);
            UI_Slot slot = slotObj.GetComponent<UI_Slot>();
            slot.slotIndex = i;
            _slotUIs[i] = slot;
        }

        RefreshAllInventoryUI();
    }

    void OnEnable()
    {
        // 监听全局背包变更事件，自动刷新UI
        EventCenter.OnInventoryChanged += RefreshAllInventoryUI;
        // 监听读档完成事件，防止读档不刷新
        EventCenter.OnGameLoaded += RefreshAllInventoryUI;
    }


    /// <summary>
    /// 刷新所有背包UI
    /// </summary>
    public void RefreshAllInventoryUI()
    {
        InventoryData inv = InventoryManager.Instance.InventoryData;
        for (int i = 0; i < _slotUIs.Length; i++)
        {
            RefreshSingleSlot(_slotUIs[i], inv.GetSlot(i));
        }
    }

    /// <summary>
    /// 刷新当前格子
    /// </summary>
    /// <param name="slot"></param>
    /// <param name="slotData"></param>
    void RefreshSingleSlot(UI_Slot slot, SlotData slotData)
    {
        if (slot.currentItemObj != null)
        {
            Destroy(slot.currentItemObj);
            slot.currentItemObj = null;
        }

        TextMeshProUGUI stackText = slot.transform.Find("Count")?.GetComponent<TextMeshProUGUI>();
        if (stackText != null)
        {
            stackText.text = string.Empty;
        }

        if (slotData.IsEmpty()) return;

        //获得对于ID物品信息
        ItemData itemData = InventoryManager.Instance.GetItemById(slotData.itemId);
        if (itemData == null) return;

        //生成并校准位置
        GameObject itemObj = Instantiate(itemData.itemBasePrefab, slot.transform);
        RectTransform rect = itemObj.GetComponent<RectTransform>();
        rect.anchoredPosition = Vector2.zero;

        Image iconImg = itemObj.GetComponent<Image>();
        if (iconImg != null) iconImg.sprite = itemData.icon;

        Item itemComp = itemObj.GetComponent<Item>();
        if (itemComp != null) itemComp.itemData = itemData;

        slot.currentItemObj = itemObj;

        if (stackText != null && slotData.stackCount > 1)
        {
            stackText.text = slotData.stackCount.ToString();
        }
    }

    void OnDisable()
    {
        // 取消事件订阅，防止内存泄漏
        EventCenter.OnInventoryChanged -= RefreshAllInventoryUI;
        EventCenter.OnGameLoaded -= RefreshAllInventoryUI;
    }
}
