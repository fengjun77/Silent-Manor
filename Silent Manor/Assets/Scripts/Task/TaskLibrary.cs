using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Task/Task Library", fileName = "TaskLibrary")]
public class TaskLibrary : ScriptableObject
{
    [Header("所有任务配置集合")]
    public List<TaskSO> allTasks = new List<TaskSO>();

    // 根据任务ID查找任务SO
    public TaskSO GetTaskById(string taskId)
    {
        foreach (var task in allTasks)
        {
            if (task.taskId == taskId)
                return task;
        }
        Debug.LogWarning($"未找到ID为 {taskId} 的任务");
        return null;
    }

    // 获取指定NPC的全部任务（按任务链先后排序）
    public List<TaskSO> GetNpcAllTasks(string npcId)
    {
        List<TaskSO> result = new List<TaskSO>();
        foreach (var task in allTasks)
        {
            if (task.ownerNpcId == npcId)
                result.Add(task);
        }
        return result;
    }
}
