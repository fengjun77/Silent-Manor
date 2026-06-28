using UnityEngine;

public class TaskProgressListener : MonoBehaviour
{
    private TaskProgressManager progressMgr;
    private TaskManager taskMgr;

    void Awake()
    {
        progressMgr = TaskProgressManager.Instance;
        taskMgr = TaskManager.Instance;

        // 绑定全局拾取事件
        EventCenter.OnItemPickUp += OnPickItem;
        // 绑定全局击杀事件
        EventCenter.OnEnemyKilled += OnKillEnemy;
    }

    void OnDestroy()
    {
        // 注销防止内存泄漏
        EventCenter.OnItemPickUp -= OnPickItem;
        EventCenter.OnEnemyKilled -= OnKillEnemy;
    }

    // 拾取道具触发
    void OnPickItem(int itemId, int count)
    {
        // 更新收集类任务进度
        progressMgr.AddObjectiveProgress(itemId, count);

        // 遍历所有进行中任务，判断是否全部完成
        foreach (var progress in progressMgr.allTaskProgress)
        {
            if (progress.taskState != TaskState.Running) continue;
            taskMgr.AutoCheckTaskAllObjectiveComplete(progress.taskId);
        }
    }

    // 击杀怪物触发
    void OnKillEnemy(int enemyId)
    {
        // 更新击杀类任务进度
        progressMgr.AddObjectiveProgress(enemyId, 1);

        // 遍历所有进行中任务，判断是否全部完成
        foreach (var progress in progressMgr.allTaskProgress)
        {
            if (progress.taskState != TaskState.Running) continue;
            taskMgr.AutoCheckTaskAllObjectiveComplete(progress.taskId);
        }
    }
}
