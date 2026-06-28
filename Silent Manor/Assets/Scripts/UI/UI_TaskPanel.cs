using System.Collections.Generic;
using UnityEngine;

public class UI_TaskPanel : MonoBehaviour
{
    [Header("滚动容器：存放所有任务卡片")]
    public Transform scrollContent;
    [Header("任务完整卡片预制体")]
    public UI_TaskCard taskCardPrefab;

    private TaskManager taskMgr => TaskManager.Instance;
    private TaskProgressManager progressMgr => TaskProgressManager.Instance;
    private TaskLibrary taskLib;

    // 缓存已生成的所有任务卡片
    private List<UI_TaskCard> allTaskCards = new List<UI_TaskCard>();

    void OnEnable()
    {
        taskLib = taskMgr.taskLibrary;
        // 带taskId的事件用lambda丢弃参数
        TaskEvent.OnTaskTake += (_) => RefreshAllTaskCards();
        TaskEvent.OnTaskAllObjectiveComplete += (_) => RefreshAllTaskCards();
        TaskEvent.OnTaskFinish += (_) => RefreshAllTaskCards();
        TaskEvent.OnTaskAbandon += (_) => RefreshAllTaskCards();
        // 无参进度事件直接绑定
        TaskEvent.OnTaskProgressUpdate += RefreshAllTaskCards;
    }

    void OnDisable()
    {
        TaskEvent.OnTaskTake -= (_) => RefreshAllTaskCards();
        TaskEvent.OnTaskAllObjectiveComplete -= (_) => RefreshAllTaskCards();
        TaskEvent.OnTaskFinish -= (_) => RefreshAllTaskCards();
        TaskEvent.OnTaskAbandon -= (_) => RefreshAllTaskCards();
        TaskEvent.OnTaskProgressUpdate -= RefreshAllTaskCards;
    }

    #region 面板开关（外部调用）
    public void OpenPanel()
    {
        gameObject.SetActive(true);
        RefreshAllTaskCards();
    }

    public void ClosePanel()
    {
        gameObject.SetActive(false);
    }
    #endregion

    #region 核心刷新逻辑
    /// 清空并重新生成全部任务卡片
    public void RefreshAllTaskCards()
    {
        // 销毁旧卡片
        foreach (var card in allTaskCards)
            Destroy(card.gameObject);
        allTaskCards.Clear();

        // 遍历玩家所有进行中/待交付任务
        foreach (var progress in progressMgr.allTaskProgress)
        {
            if (progress.taskState != TaskState.Running && progress.taskState != TaskState.WaitSubmit)
                continue;

            TaskSO taskCfg = taskLib.GetTaskById(progress.taskId);
            if (taskCfg == null) continue;

            // 生成一张完整任务卡片
            UI_TaskCard newCard = Instantiate(taskCardPrefab, scrollContent);
            newCard.BindTaskData(taskCfg, progress, this);
            allTaskCards.Add(newCard);
        }
    }
    #endregion

    #region 外部接口：放弃任务
    public void AbandonTaskById(string taskId)
    {
        taskMgr.AbandonTask(taskId);
    }
    #endregion
}
