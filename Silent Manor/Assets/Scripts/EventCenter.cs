using System;
using UnityEngine;

public static class EventCenter
{
    public static event Action OnInventoryChanged;

    public static void CallInventoryChangedEvent()
    {
        OnInventoryChanged?.Invoke();
    }
    
    // 存档完成事件
    public static event Action OnGameSaved;
    public static void CallGameSavedEvent()
    {
        OnGameSaved?.Invoke();
    }

    // 读档完成事件
    public static event Action OnGameLoaded;
    public static void CallGameLoadedEvent()
    {
        OnGameLoaded?.Invoke();
    }
}
