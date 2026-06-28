using System;

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

    // 拾取道具：道具ID，拾取数量
    public static Action<int, int> OnItemPickUp;
    // 怪物击杀：怪物ID
    public static Action<int> OnEnemyKilled;
}

public static class TaskEvent
{
    // 接取任务，参数 taskId
    public static Action<string> OnTaskTake;
    // 任意任务进度更新，无参，统一刷新全部UI
    public static Action OnTaskProgressUpdate;
    // 单个任务全部目标完成，参数 taskId
    public static Action<string> OnTaskAllObjectiveComplete;
    // 任务提交完成，参数 taskId
    public static Action<string> OnTaskFinish;
    // 玩家放弃任务，参数 taskId
    public static Action<string> OnTaskAbandon;
}
