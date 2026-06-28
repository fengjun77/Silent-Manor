using System.Collections.Generic;
using UnityEngine;

public class TaskProgressManager : Singleton<TaskProgressManager>
{
    // 玩家当前所有任务进度（进行中/放弃/已完成）
    public List<PlayerTaskProgress> allTaskProgress = new List<PlayerTaskProgress>();

    #region 基础查询接口
    /// 通过任务ID获取进度实例
    public PlayerTaskProgress GetTaskProgress(string taskId)
    {
        foreach (var progress in allTaskProgress)
        {
            if (progress.taskId == taskId)
                return progress;
        }
        return null;
    }

    /// 判断玩家是否拥有该任务进度
    public bool HasTaskProgress(string taskId)
    {
        return GetTaskProgress(taskId) != null;
    }
    #endregion

    #region 任务接取 - 创建进度
    public void CreateNewTaskProgress(TaskSO taskSo)
    {
        // 重复接取拦截
        if (HasTaskProgress(taskSo.taskId))
        {
            Debug.LogWarning($"已拥有任务 {taskSo.taskName}，无需重复接取");
            return;
        }

        PlayerTaskProgress newProgress = new PlayerTaskProgress();
        newProgress.InitNewProgress(taskSo);
        allTaskProgress.Add(newProgress);

        // 广播任务新增事件（后续UI、TaskManager监听）
        TaskEvent.OnTaskTake?.Invoke(taskSo.taskId);
    }
    #endregion

    #region 更新击杀/收集目标进度
    public void AddObjectiveProgress(int targetId, int addNum = 1)
    {
        // 遍历所有进行中任务，匹配目标ID累加进度
        foreach (var progress in allTaskProgress)
        {
            if (progress.taskState != TaskState.Running) continue;
            progress.UpdateObjectiveProgress(targetId, addNum);
        }
        // 广播进度更新事件
        TaskEvent.OnTaskProgressUpdate?.Invoke();
    }
    #endregion

    #region 修改任务状态
    public void SetTaskState(string taskId, TaskState newState)
    {
        var progress = GetTaskProgress(taskId);
        if (progress == null) return;

        progress.taskState = newState;

        if (newState == TaskState.WaitSubmit)
        {
            TaskEvent.OnTaskAllObjectiveComplete?.Invoke(taskId);
        }
        else if (newState == TaskState.Finished)
        {
            TaskEvent.OnTaskFinish?.Invoke(taskId);
        }
        else if (newState == TaskState.Abandon)
        {
            TaskEvent.OnTaskAbandon?.Invoke(taskId);
        }
    }
    #endregion

    #region 存档预留（序列化读写占位，后续扩展本地存储）
    public void SaveProgress()
    {
        // 后续补充Json/PlayerPrefs存档逻辑
    }

    public void LoadProgress()
    {
        // 读档时清空旧数据再加载
        allTaskProgress.Clear();
        // 后续补充读取逻辑
    }
    #endregion
}
