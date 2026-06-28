using UnityEngine;
using System.Collections.Generic;

public class TaskManager : Singleton<TaskManager>
{
    [Header("全局任务资源库，拖入场景唯一TaskLibrary")]
    public TaskLibrary taskLibrary;

    private TaskProgressManager progressMgr => TaskProgressManager.Instance;

    #region 外部通用查询接口：获取NPC当前任务状态
    /// <summary>
    /// 获取NPC当前交互对应的任务状态
    /// 返回4种状态：无任务可交互 / 可接取新任务 / 进行中未完成 / 待交付
    /// </summary>
    public NpcTaskStatus GetNpcCurrentTaskStatus(string npcId)
    {
        List<TaskSO> npcAllTask = taskLibrary.GetNpcAllTasks(npcId);
        if (npcAllTask.Count == 0)
            return NpcTaskStatus.NoAnyTask;

        // 1. 查找该NPC正在进行/待交付的任务
        foreach (var task in npcAllTask)
        {
            var progress = progressMgr.GetTaskProgress(task.taskId);
            if (progress == null) continue;

            if (progress.taskState == TaskState.Running)
                return NpcTaskStatus.TaskRunning;
            if (progress.taskState == TaskState.WaitSubmit)
                return NpcTaskStatus.WaitSubmit;
        }

        // 2. 没有进行中/待交付任务，寻找下一个可接取任务（前置已完成）
        foreach (var task in npcAllTask)
        {
            // 已有进度并且任务已完成，跳过
            var progress = progressMgr.GetTaskProgress(task.taskId);
            if (progress != null && progress.taskState == TaskState.Finished)
                continue;

            // 无前置任务 或者 前置任务已完成
            bool preTaskFinish = string.IsNullOrEmpty(task.preTaskId) || IsTaskFinished(task.preTaskId);
            if (preTaskFinish)
                return NpcTaskStatus.CanTakeNewTask;
        }

        // 全部任务做完
        return NpcTaskStatus.NoAnyTask;
    }

    /// 判断某任务是否已经交付完成
    public bool IsTaskFinished(string taskId)
    {
        var progress = progressMgr.GetTaskProgress(taskId);
        return progress != null && progress.taskState == TaskState.Finished;
    }

    /// 根据NPCID获取当前可接取的任务
    public TaskSO GetNpcAvailableTakeTask(string npcId)
    {
        List<TaskSO> npcAllTask = taskLibrary.GetNpcAllTasks(npcId);
        foreach (var task in npcAllTask)
        {
            var progress = progressMgr.GetTaskProgress(task.taskId);
            if (progress != null && progress.taskState == TaskState.Finished)
                continue;

            bool preOk = string.IsNullOrEmpty(task.preTaskId) || IsTaskFinished(task.preTaskId);
            if (preOk)
                return task;
        }
        return null;
    }

    /// 获取NPC当前进行中/待交付任务
    public TaskSO GetNpcCurrentActiveTask(string npcId)
    {
        List<TaskSO> npcAllTask = taskLibrary.GetNpcAllTasks(npcId);
        foreach (var task in npcAllTask)
        {
            var progress = progressMgr.GetTaskProgress(task.taskId);
            if (progress == null) continue;
            if (progress.taskState == TaskState.Running || progress.taskState == TaskState.WaitSubmit)
                return task;
        }
        return null;
    }
    #endregion

    #region 任务接取逻辑（对话【接受任务】分支调用）
    public bool TryTakeTask(string taskId)
    {
        TaskSO task = taskLibrary.GetTaskById(taskId);
        if (task == null) return false;

        // 已有进度拦截
        if (progressMgr.HasTaskProgress(taskId))
        {
            Debug.LogWarning($"任务 {task.taskName} 已接取");
            return false;
        }

        // 前置任务校验
        if (!string.IsNullOrEmpty(task.preTaskId) && !IsTaskFinished(task.preTaskId))
        {
            Debug.LogWarning($"未完成前置任务 {task.preTaskId}，无法接取");
            return false;
        }

        // 创建进度，触发事件刷新UI
        progressMgr.CreateNewTaskProgress(task);
        return true;
    }
    #endregion

    #region 任务交付校验&完成逻辑（对话【提交任务】分支调用）
    /// 校验当前任务是否满足交付条件
    public bool CheckTaskCanSubmit(TaskSO task)
    {
        var progress = progressMgr.GetTaskProgress(task.taskId);
        if (progress == null || progress.taskState != TaskState.WaitSubmit)
            return false;

        // 逐条校验所有目标
        foreach (var obj in task.objectives)
        {
            switch (obj.objType)
            {
                case ObjectiveType.CollectGet:
                case ObjectiveType.KillEnemy:
                    int cur = progress.GetCurrentProgress(obj.targetId);
                    if (cur < obj.needCount)
                        return false;
                    break;
                case ObjectiveType.ItemSubmit:
                    // 此处预留：调用背包管理器获取道具数量对比，后面补背包对接代码
                    int bagCount = InventoryManager.Instance.GetItemCount(obj.targetId);
                    if (bagCount < obj.needCount)
                        return false;
                    break;
            }
        }
        return true;
    }

    /// 执行提交任务，发放奖励，切换任务状态
    public void SubmitTask(string taskId)
    {
        TaskSO task = taskLibrary.GetTaskById(taskId);
        if (task == null) return;
        if (!CheckTaskCanSubmit(task)) return;

        // 1. 扣除提交类道具
        foreach (var obj in task.objectives)
        {
            if (obj.objType == ObjectiveType.ItemSubmit)
            {
                InventoryManager.Instance.ConsumeItem(obj.targetId, obj.needCount);
            }
        }

        // 2. 发放奖励
        InventoryManager.Instance.AddGold(task.rewardGold);
        foreach (var reward in task.rewardItems)
        {
            InventoryManager.Instance.AddItemById(reward.itemId, reward.count);
        }

        // 3. 标记任务完成
        progressMgr.SetTaskState(taskId, TaskState.Finished);
        // 清理已完成任务进度缓存
        PlayerTaskProgress finishProgress = progressMgr.GetTaskProgress(taskId);
        if (finishProgress != null)
        {
            progressMgr.allTaskProgress.Remove(finishProgress);
        }
    }
    #endregion

    #region 放弃任务（UI按钮调用）
    public void AbandonTask(string taskId)
    {
        TaskSO task = taskLibrary.GetTaskById(taskId);
        var progress = progressMgr.GetTaskProgress(taskId);
        if (task == null || progress == null) return;

        // 不可重复接取的任务禁止放弃
        if (!task.canRetakeAfterAbandon)
        {
            Debug.LogWarning($"任务 {task.taskName} 不允许放弃");
            return;
        }

        progressMgr.SetTaskState(taskId, TaskState.Abandon);
        // 放弃后直接移除进度，NPC恢复可接取状态
        progressMgr.allTaskProgress.Remove(progress);
    }
    #endregion

    #region 内部自动检测：判断任务是否全部目标完成，切换WaitSubmit状态
    /// 外部拾取/击杀事件后调用，自动判断是否完成所有目标
    public void AutoCheckTaskAllObjectiveComplete(string taskId)
    {
        TaskSO task = taskLibrary.GetTaskById(taskId);
        var progress = progressMgr.GetTaskProgress(taskId);
        if (task == null || progress == null || progress.taskState != TaskState.Running)
            return;

        bool allComplete = true;
        foreach (var obj in task.objectives)
        {
            // 提交道具类型不参与实时判断，仅交付时校验
            if (obj.objType == ObjectiveType.ItemSubmit)
                continue;

            int cur = progress.GetCurrentProgress(obj.targetId);
            if (cur < obj.needCount)
            {
                allComplete = false;
                break;
            }
        }

        if (allComplete)
        {
            progressMgr.SetTaskState(taskId, TaskState.WaitSubmit);
        }
    }
    #endregion
}

// 新增NPC任务交互状态枚举，新建 TaskNpcStatus.cs
public enum NpcTaskStatus
{
    // NPC无任何任务
    NoAnyTask,
    // 存在未接取的新任务
    CanTakeNewTask,
    // 任务进行中，未完成
    TaskRunning,
    // 任务全部目标完成，等待交付
    WaitSubmit
}
