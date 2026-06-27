using UnityEngine;

[System.Serializable]
public class MapConfig
{
    public string mapId;        // 地图唯一标识（城镇/森林）
    public GameObject mapPanel;  // 对应静态UI面板
}

public class MapManager : Singleton<MapManager>
{
    public string defaultMapId;

    private string _currentMapId;

    protected override void Awake()
    {
        base.Awake();

        _currentMapId = defaultMapId;
    }

    // 地图切换时调用，更新当前地图ID
    public void SetCurrentMap(string mapId)
    {
        if (string.IsNullOrEmpty(mapId)) return;
        _currentMapId = mapId;
        Debug.Log("切换当前地图：" + mapId);
    }

    /// 获取当前地图ID（存档用）
    public string GetCurrentMapId()
    {
        return _currentMapId;
    }

    /// 读档恢复地图ID
    public void RestoreMapId(string saveMapId)
    {
        if (!string.IsNullOrEmpty(saveMapId))
        {
            _currentMapId = saveMapId;
        }
        else
        {
            _currentMapId = defaultMapId;
            Debug.LogWarning("存档无地图ID，使用初始默认地图");
        }
    }
}
