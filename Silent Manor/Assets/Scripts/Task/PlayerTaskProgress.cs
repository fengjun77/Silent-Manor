using System.Collections.Generic;

[System.Serializable]
public class PlayerTaskProgress
{
    // 绑定对应任务配置ID
    public string taskId;
    // 当前任务整体状态
    public TaskState taskState;

    // 实时进度字典：目标唯一ID - 当前完成数量
    // 仅 CollectGet / KillEnemy 类型会存入数值
    public Dictionary<int, int> objectiveProgress = new Dictionary<int, int>();

    /// 初始化一条新接取的任务进度
    public void InitNewProgress(TaskSO taskSo)
    {
        taskId = taskSo.taskId;
        taskState = TaskState.Running;
        objectiveProgress.Clear();

        // 所有收集、击杀目标初始进度0，提交道具类型不初始化（交付时实时读背包）
        foreach (var obj in taskSo.objectives)
        {
            if (obj.objType == ObjectiveType.CollectGet || obj.objType == ObjectiveType.KillEnemy)
            {
                if (!objectiveProgress.ContainsKey(obj.targetId))
                {
                    objectiveProgress.Add(obj.targetId, 0);
                }
            }
        }
    }

    /// 更新单个目标进度
    public void UpdateObjectiveProgress(int targetId, int addCount)
    {
        if (!objectiveProgress.ContainsKey(targetId)) return;
        objectiveProgress[targetId] += addCount;
    }

    /// 获取目标当前进度，无记录返回0
    public int GetCurrentProgress(int targetId)
    {
        if (objectiveProgress.TryGetValue(targetId, out int val))
            return val;
        return 0;
    }
}
