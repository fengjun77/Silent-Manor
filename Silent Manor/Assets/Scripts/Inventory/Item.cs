using UnityEngine;
using UnityEngine.EventSystems;

public class Item : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    public ItemData itemData;

    private CanvasGroup _canvasGroup;
    private int _originSlotIndex;
    private Transform _originParent;

    void Awake()
    {
        _canvasGroup = GetComponent<CanvasGroup>();
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        UI_Slot originSlot = transform.parent.GetComponent<UI_Slot>();
        _originSlotIndex = originSlot.slotIndex;
        _originParent = transform.parent;

        transform.SetParent(transform.root);
        _canvasGroup.blocksRaycasts = false;
        _canvasGroup.alpha = 0.6f;
    }

    public void OnDrag(PointerEventData eventData)
    {
        transform.position = eventData.position;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        _canvasGroup.blocksRaycasts = true;
        _canvasGroup.alpha = 1f;

        UI_Slot dropSlot = eventData.pointerEnter?.GetComponent<UI_Slot>();
        if (dropSlot == null && eventData.pointerEnter != null)
        {
            dropSlot = eventData.pointerEnter.GetComponentInParent<UI_Slot>();
        }

        if (dropSlot != null)
        {
            int targetIdx = dropSlot.slotIndex;
            // 如果拖回原来自身格子：直接归位，不交换
            if (targetIdx == _originSlotIndex)
            {
                ResetItemToOrigin();
            }
            else
            {
                // 不同格子才执行交换，交换后UI自动重建，不用手动归位
                InventoryManager.Instance.InventoryData.SwapSlot(_originSlotIndex, targetIdx);
            }
        }
        // 情况2：没有落在任何格子，归位
        else
        {
            ResetItemToOrigin();
        }
    }

    // 统一归位逻辑，复用
    private void ResetItemToOrigin()
    {
        transform.SetParent(_originParent);
        RectTransform rt = GetComponent<RectTransform>();
        rt.anchoredPosition = Vector2.zero;
    }
}
