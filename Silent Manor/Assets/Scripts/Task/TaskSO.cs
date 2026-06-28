using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class TaskRewardItem
{
    [Header("道具ID")]
    public int itemId;
    [Header("奖励数量")]
    public int count;
}

[CreateAssetMenu(menuName = "Task/Task Data", fileName = "NewTask")]
public class TaskSO : ScriptableObject
{
    [Header("基础标识")]
    public string taskId;
    public string taskName;
    [TextArea(3, 8)] public string taskDetail;

    [Header("归属NPC")]
    public string ownerNpcId;

    [Header("任务链控制")]
    public string preTaskId; // 前置任务ID，为空代表无前置
    public string nextTaskId; // 完成后解锁的下一环任务ID

    [Header("任务目标列表")]
    public List<TaskObjective> objectives = new List<TaskObjective>();

    [Header("任务奖励")]
    public List<TaskRewardItem> rewardItems = new List<TaskRewardItem>();
    public int rewardGold;

    [Header("是否可重复接取（放弃后重新接）")]
    public bool canRetakeAfterAbandon;
}