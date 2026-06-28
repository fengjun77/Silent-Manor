
using UnityEngine;

public class NPC : MonoBehaviour, IInteractable
{
    public string npcId; // 和TaskSO.ownerNpcId保持一致

    // 原基础对话（会被代码动态替换）
    public NPCDialogue dialogueData;

    [Header("任务分状态独立对话资源")]
    public NPCDialogue dialogue_Normal;        // 无任务，普通闲聊
    public NPCDialogue dialogue_CanTakeTask;   // 有任务可接取
    public NPCDialogue dialogue_TaskRunning;   // 任务进行中
    public NPCDialogue dialogue_WaitSubmit;    // 任务完成可交付

    [Header("交互")]
    public float iconHeightOffset = 1.3f;

    private float skipCoolDown = 0.12f;
    private float lastSkipTime;

    void Awake()
    {
        NPCManager.Instance.RegisterNPC(this);
    }

    #region IInteractable
    public bool CanInteract()
    {
        // 补充多条件校验
        if (GameManager.Instance == null) return false;
        if (GameManager.Instance.IsMenuOpen) return false;
        if (NPCManager.Instance != null && NPCManager.Instance.CurrentTalkingNPC != null) return false;
        return true;
    }

    public void Interact()
    {
        if (Time.time - lastSkipTime < skipCoolDown) return;
        lastSkipTime = Time.time;
        // 增加空指针校验
        if (NPCManager.Instance == null || dialogueData == null) 
        {
            Debug.LogError($"NPC {npcId} 对话数据为空或NPCManager未初始化");
            return;
        }
        NPCManager.Instance.OnNPCInteract(this);
    }

    public Vector3 GetIconSpawnPos()
    {
        return transform.position + Vector3.up * iconHeightOffset;
    }
    #endregion
}
