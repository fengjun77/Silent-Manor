using System.Collections.Generic;
using UnityEngine;

public class UI_Map : MonoBehaviour
{
    public List<MapConfig> allMapPanels;
    [Header("游戏初始场景默认地图ID")]
    public string defaultMapId;

    void Awake()
    {
        // 启动全部隐藏
        foreach (var cfg in allMapPanels)
        {
            if (cfg.mapPanel) cfg.mapPanel.SetActive(false);
        }
    }

    // 打开地图面板时调用，只激活对应地图UI，其余隐藏
    public void RefreshMapPanel()
    {
        foreach (var cfg in allMapPanels)
        {
            if (cfg.mapPanel) cfg.mapPanel.SetActive(false);
        }

        string currentMapId = MapManager.Instance.GetCurrentMapId();
        var target = allMapPanels.Find(c => c.mapId == currentMapId);
        if (target != null && target.mapPanel)
        {
            target.mapPanel.SetActive(true);
        }
        else
        {
            Debug.LogWarning($"未配置地图ID：{currentMapId} 对应的UI面板");
        }
    }
}
