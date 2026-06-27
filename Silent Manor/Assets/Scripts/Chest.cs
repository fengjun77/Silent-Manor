using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class TreasureDropItem
{
    [Header("掉落物品")]
    public ItemData itemData;
    [Header("物品权重（数值越大越容易出）")]
    public int weight = 10;
    [Header("数量区间：最小~最大")]
    public int minCount = 1;
    public int maxCount = 3;
}

// 宝箱类型
public enum ChestType
{
    Refreshable,
    PermanentOnce
}

public class Chest : MonoBehaviour
{
    [Header("交互设置")]
    public float interactRange = 2f;
    public GameObject tipIconPrefab;
    public float iconFloatSpeed = 1f;
    public float iconFloatOffset = 0.3f;
    public float destroyDelay = 2f;

    [Header("宝箱类型配置")]
    public ChestType chestType;
    [Tooltip("永久一次性宝箱必须填写全局唯一ID，每个箱子不能重复")]
    public string chestUniqueId;

    [Header("刷新型宝箱专属：区域怪物检测")]
    public List<GameObject> areaMonsters; // 当前区域所有怪物物体

    [Header("掉落物品池（权重越大越容易抽到）")]
    public List<TreasureDropItem> dropPool = new List<TreasureDropItem>();

    private bool isOpened = false;
    private GameObject tipIconObj;
    private Transform playerTrans;
    private Vector3 iconOriginPos;
    private SpriteRenderer sr;
    public Sprite openSprite;

    void Awake()
    {
        sr = GetComponent<SpriteRenderer>();
    }

    void Start()
    {
        // 预先找玩家
        GameObject player = GameObject.FindWithTag("Player");
        if (player != null)
            playerTrans = player.transform;
        
        // 加载时根据类型初始化宝箱状态
        Invoke(nameof(InitChestState), 0.1f);
    }

    /// 根据存档/类型初始化宝箱
    void InitChestState()
    {
        // 永久一次性宝箱：读取存档，已开启直接销毁
        if (chestType == ChestType.PermanentOnce)
        {
            if (string.IsNullOrEmpty(chestUniqueId))
            {
                Debug.LogError($"永久宝箱 {gameObject.name} 未填写唯一ID！");
                return;
            }

            if (SaveManager.Instance.IsChestOpened(chestUniqueId))
            {
                Destroy(gameObject);
            }
        }
        // 刷新型宝箱：每次加载强制重置为未开启
        else if (chestType == ChestType.Refreshable)
        {
            isOpened = false;
        }
    }

    void Update()
    {
        if (isOpened || playerTrans == null) return;

        float distance = Vector3.Distance(transform.position, playerTrans.position);
        bool inRange = distance <= interactRange;

        // 刷新型宝箱额外判断：怪物是否全部清除，有怪物无法交互
        bool canInteract = true;
        if (chestType == ChestType.Refreshable)
        {
            canInteract = CheckAllMonsterDead();
        }

        // 玩家在范围内，生成/显示头顶图标
        if (inRange && canInteract)
        {
            if (tipIconObj == null)
            {
                SpawnTipIcon();
            }
            // 上下浮动动画
            float floatY = Mathf.Sin(Time.time * iconFloatSpeed) * iconFloatOffset;
            Vector3 targetWorldPos = iconOriginPos;
            targetWorldPos.y += floatY;
            tipIconObj.transform.position = targetWorldPos;

            // 按F开启宝箱
            if (Input.GetKeyDown(KeyCode.F))
            {
                OpenBox();
            }
        }
        else
        {
            // 离开范围销毁图标
            if (tipIconObj != null)
            {
                Destroy(tipIconObj);
                tipIconObj = null;
            }
        }
    }

    // 检测区域所有怪物是否全部死亡
    bool CheckAllMonsterDead()
    {
        if (areaMonsters == null || areaMonsters.Count == 0)
            return true;
        foreach (var monster in areaMonsters)
        {
            if (monster != null)
                return false;
        }
        return true;
    }

    // 生成头顶浮动图标
    void SpawnTipIcon()
    {
        if (tipIconPrefab == null) return;
        iconOriginPos = transform.position + Vector3.up * 1.2f;
        tipIconObj = Instantiate(tipIconPrefab, iconOriginPos, Quaternion.identity, transform);
    }

    void OpenBox()
    {
        isOpened = true;
        sr.sprite = openSprite;

        SoundManager.Instance.Play("Chest", false);

        // 销毁头顶提示图标
        if (tipIconObj != null)
        {
            Destroy(tipIconObj);
            tipIconObj = null;
        }

        // 1. 根据权重随机选一个物品
        TreasureDropItem drop = GetRandomDropByWeight();
        if (drop == null || drop.itemData == null)
        {
            Debug.LogWarning("宝箱无有效掉落物品！");
            Destroy(gameObject, destroyDelay);
            return;
        }

        // 2. 随机数量
        int randomCount = Random.Range(drop.minCount, drop.maxCount + 1);

        // 物品直接进背包
        int remain = InventoryManager.Instance.AddItem(drop.itemData, randomCount);
        int getNum = randomCount - remain;

        if (getNum > 0)
        {
            TipManager.Instance.ShowGetItemTip(drop.itemData, getNum);
        }
        if (remain > 0)
        {
            Debug.Log($"背包已满，剩余{remain}个无法拾取");
        }

        // 永久一次性宝箱：存入存档已开启列表
        if (chestType == ChestType.PermanentOnce)
        {
            SaveManager.Instance.AddOpenedChestId(chestUniqueId);
        }

        // 2秒后销毁宝箱
        Destroy(gameObject, destroyDelay);
    }

    /// <summary>按权重随机抽取掉落，权重越大概率越高</summary>
    private TreasureDropItem GetRandomDropByWeight()
    {
        // 计算总权重
        int totalWeight = 0;
        foreach (var item in dropPool)
        {
            if (item.itemData != null)
                totalWeight += item.weight;
        }

        if (totalWeight <= 0) return null;

        // 随机一个0~总权重的数字
        int randomValue = Random.Range(1, totalWeight + 1);
        int currentWeight = 0;

        foreach (var item in dropPool)
        {
            if (item.itemData == null) continue;
            currentWeight += item.weight;
            if (randomValue <= currentWeight)
            {
                return item;
            }
        }
        return null;
    }
}
