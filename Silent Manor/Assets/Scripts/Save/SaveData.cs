using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class SaveData
{
    public Vector3 playerPos;
    public string mapBoundName;
    public InventorySaveData inventoryData;
    public string currentMapId;

    public float sfxVolume = 1f;

    // 永久一次性宝箱：存储所有已开启宝箱唯一ID
    public List<string> openedPermanentChestIds = new List<string>();
}
