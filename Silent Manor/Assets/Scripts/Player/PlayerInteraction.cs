using UnityEngine;

public class PlayerInteraction : MonoBehaviour
{
    [Header("交互检测范围")]
    public float checkRadius = 1f;
    [Header("交互提示图标预制体")]
    public GameObject interactTipPrefab;
    [Header("图标浮动参数")]
    public float floatSpeed = 6f;
    public float floatAmplitude = 0.1f;

    // 当前生效的交互目标 & 图标实例
    private IInteractable currentTarget;
    private GameObject activeTipIcon;

    void Update()
    {
        // 游戏暂停直接停止所有交互逻辑
        if (GameManager.Instance.IsGamePaused)
        {
            ClearTipIcon();
            currentTarget = null;
            return;
        }

        // 1. 查找范围内最近、可交互物体
        FindNearestInteractable();

        // 2. 根据目标刷新提示图标
        RefreshTipIcon();

        // 3. F按键执行交互
        if (InputManager.WasInteracted && currentTarget != null)
        {
            currentTarget.Interact();
        }
    }

    /// 查找半径内满足CanInteract的最近物体
    void FindNearestInteractable()
    {
        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, checkRadius);
        IInteractable nearestTarget = null;
        float minDistance = float.MaxValue;

        foreach (var col in hits)
        {
            IInteractable interact = col.GetComponent<IInteractable>();
            if (interact == null) continue;
            // 当前不允许交互直接跳过
            if (!interact.CanInteract()) continue;

            float dist = Vector2.Distance(transform.position, col.transform.position);
            if (dist < minDistance)
            {
                minDistance = dist;
                nearestTarget = interact;
            }
        }

        currentTarget = nearestTarget;
    }

    /// 刷新图标：有目标则生成/跟随浮动，无目标销毁
    void RefreshTipIcon()
    {
        if (currentTarget == null)
        {
            ClearTipIcon();
            return;
        }

        // 没有图标则实例化
        if (activeTipIcon == null)
        {
            Vector3 spawnPos = currentTarget.GetIconSpawnPos();
            activeTipIcon = Instantiate(interactTipPrefab, spawnPos, Quaternion.identity, null);
        }

        // 持续更新图标位置+上下浮动
        Vector3 basePos = currentTarget.GetIconSpawnPos();
        float offsetY = Mathf.Sin(Time.time * floatSpeed) * floatAmplitude;
        activeTipIcon.transform.position = new Vector3(basePos.x, basePos.y + offsetY, basePos.z);
    }

    /// 销毁当前交互图标
    void ClearTipIcon()
    {
        if (activeTipIcon != null)
        {
            Destroy(activeTipIcon);
            activeTipIcon = null;
        }
    }
}
