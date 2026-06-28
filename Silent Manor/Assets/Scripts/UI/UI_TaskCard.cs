using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UI_TaskCard : MonoBehaviour
{
    [Header("任务基础信息")]
    public TMP_Text txtTaskName;
    public TMP_Text txtTaskDetail;

    [Header("任务目标清单容器")]
    public Transform objectiveContent;
    public UI_TaskObjectiveItem objectiveItemPrefab;

    [Header("放弃按钮")]
    public Button btnAbandon;

    private TaskSO bindTask;
    private PlayerTaskProgress bindProgress;
    private UI_TaskPanel parentPanel;

    // 缓存生成的目标条目
    private List<UI_TaskObjectiveItem> spawnObjectives = new List<UI_TaskObjectiveItem>();

    /// 绑定任务数据，刷新整张卡片所有内容
    public void BindTaskData(TaskSO taskData, PlayerTaskProgress progress, UI_TaskPanel panel)
    {
        bindTask = taskData;
        bindProgress = progress;
        parentPanel = panel;

        // 1. 基础名称、详情
        txtTaskName.text = taskData.taskName;
        txtTaskDetail.text = taskData.taskDetail;

        // 2. 清空旧目标条目
        ClearAllObjectiveItems();

        // 3. 动态生成每条任务目标
        foreach (var obj in taskData.objectives)
        {
            UI_TaskObjectiveItem item = Instantiate(objectiveItemPrefab, objectiveContent);
            spawnObjectives.Add(item);

            if (obj.objType == ObjectiveType.ItemSubmit)
            {
                // 提交道具：实时读取背包数量判断是否充足
                int ownCount = InventoryManager.Instance.GetItemCount(obj.targetId);
                bool hasEnough = ownCount >= obj.needCount;
                item.SetSubmitItemData(obj.descText, hasEnough);
            }
            else
            {
                // 收集/击杀任务：读取本地进度
                int curNum = bindProgress.GetCurrentProgress(obj.targetId);
                bool finish = curNum >= obj.needCount;
                item.SetData(obj.descText, curNum, obj.needCount, finish);
            }
        }

        // 4. 放弃按钮：仅配置允许放弃的任务显示
        btnAbandon.gameObject.SetActive(taskData.canRetakeAfterAbandon);
        btnAbandon.onClick.RemoveAllListeners();
        btnAbandon.onClick.AddListener(OnClickAbandonTask);
    }

    /// 清空清单所有子条目
    void ClearAllObjectiveItems()
    {
        foreach (var item in spawnObjectives)
            Destroy(item.gameObject);
        spawnObjectives.Clear();
    }

    /// 点击放弃任务
    void OnClickAbandonTask()
    {
        parentPanel.AbandonTaskById(bindTask.taskId);
    }
}
