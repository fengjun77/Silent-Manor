using System.IO;
using System;
using Unity.Cinemachine;
using UnityEngine;
using System.Collections.Generic;

[Serializable]
public class InventorySaveData
{
    public int totalSlotCount;
    public SavedSlot[] slots;
}

[Serializable]
public struct SavedSlot
{
    public int itemId;
    public int stackCount;
}

public class SaveManager : Singleton<SaveManager>
{
    private SaveData saveData;

    protected override void Awake()
    {
        base.Awake();
        
        // 移除自动存档绑定，背包更新不再自动存盘
        LoadGame();

    }

    void Start()
    {
        
    }

    /// <summary>手动执行全量存档，需要时主动调用（切换场景/保存按钮/退出游戏）</summary>
    public void SaveGame()
    {
        string path = GetSaveFullPath();

        // 玩家、地图数据
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
            saveData.playerPos = player.transform.position;

        CinemachineConfiner2D confiner = FindFirstObjectByType<CinemachineConfiner2D>();
        if (confiner != null)
            saveData.mapBoundName = confiner.BoundingShape2D.gameObject.name;

        saveData.currentMapId = MapManager.Instance.GetCurrentMapId();

        saveData.sfxVolume = SoundManager.Instance.GetCurrentSfxVolume();

        //背包数据（对接你的InventoryManager获取最新背包数据）
        saveData.inventoryData = InventoryManager.Instance.ExportInventorySaveData();

        // 写入本地JSON
        string json = JsonUtility.ToJson(saveData, prettyPrint: true);
        File.WriteAllText(path, json);

        // 广播存档完成事件
        EventCenter.CallGameSavedEvent();
        Debug.Log("手动存档完成");
        Debug.Log(path);
    }

    /// <summary>读取存档，游戏启动自动执行</summary>
    public void LoadGame()
    {
        string path = GetSaveFullPath();
        if (!File.Exists(path))
        {
            // 无存档，初始化全新存档对象
            saveData = new SaveData();
            saveData.openedPermanentChestIds = new List<string>();

            InventoryManager.Instance.SpawnDefaultInventoryItems();
            return;
        }

        string json = File.ReadAllText(path);
        saveData = JsonUtility.FromJson<SaveData>(json);

        // 恢复玩家位置
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
            player.transform.position = saveData.playerPos;

        // 恢复地图边界
        CinemachineConfiner2D confiner = FindFirstObjectByType<CinemachineConfiner2D>();
        if (confiner != null)
        {
            GameObject boundObj = GameObject.Find(saveData.mapBoundName);
            if (boundObj != null)
            {
                confiner.BoundingShape2D = boundObj.GetComponent<BoxCollider2D>();
            }
        }

        MapManager.Instance.RestoreMapId(saveData.currentMapId);

        SoundManager.Instance.LoadSfxVolume(saveData.sfxVolume);

        //恢复背包数据
        InventoryManager.Instance.LoadFromSaveData(saveData.inventoryData);

        // 广播读档完成，刷新背包UI
        EventCenter.CallGameLoadedEvent();
        Debug.Log("读档完成");
    }

    public bool HasSaveFile()
    {
        return File.Exists(GetSaveFullPath());
    }

    // 统一获取完整存档路径
    public string GetSaveFullPath()
    {
        return Path.Combine(Application.persistentDataPath, "saveData.json");
    }

    [ContextMenu("清除全部存档数据")]
    public void ClearSaveData()
    {
        string path = GetSaveFullPath();
        try
        {
            if (File.Exists(path))
            {
                File.Delete(path);
                Debug.Log($"成功删除存档文件：{path}");
            }
            else
            {
                Debug.Log($"存档文件不存在：{path}");
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"删除存档失败，文件被占用/权限不足：{e.Message}");
            return;
        }

        // 仅游戏运行时清空内存背包
        if (Application.isPlaying)
        {
            if (InventoryManager.Instance != null)
            {
                InventoryManager.Instance.InventoryData.ClearAllSlots();
                Debug.Log("运行中：内存背包数据已清空");
            }
        }
        else
        {
            Debug.Log("编辑器模式：仅删除本地存档，不操作内存");
        }
    }

    // 添加已开启永久宝箱ID
    public void AddOpenedChestId(string chestId)
    {
        // 内存里直接操作全局存档对象
        if (!saveData.openedPermanentChestIds.Contains(chestId))
        {
            saveData.openedPermanentChestIds.Add(chestId);
        }
        SaveGame();
    }

    // 查询宝箱是否已经开启
    public bool IsChestOpened(string chestId)
    {
        if (saveData == null) return false;
        return saveData.openedPermanentChestIds.Contains(chestId);
    }
}
