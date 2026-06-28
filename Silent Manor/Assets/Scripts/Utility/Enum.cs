
public enum TaskState
{
    // 未接取（仅存在SO，玩家无进度）
    UnTake,
    // 进行中
    Running,
    // 全部目标完成，等待找NPC交付
    WaitSubmit,
    // 已交付完成
    Finished,
    // 玩家主动放弃
    Abandon
}

public enum ObjectiveType
{
    // 拾取道具，实时累加进度
    CollectGet,
    // 交付道具，仅提交时校验背包数量，不存进度
    ItemSubmit,
    // 击杀怪物，实时累加进度
    KillEnemy
}
