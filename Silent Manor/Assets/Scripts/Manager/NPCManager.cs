using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

public class NPCManager : Singleton<NPCManager>
{
    // 所有NPC缓存
    private List<NPC> allNpcs = new List<NPC>();

    // 当前正在对话的NPC
    public NPC CurrentTalkingNPC { get; private set; }

    // 对话状态缓存
    private int dialogueIndex;
    private bool isTyping;
    private Coroutine typeCoroutine;

    #region NPC注册注销
    public void RegisterNPC(NPC npc)
    {
        if (!allNpcs.Contains(npc))
            allNpcs.Add(npc);
    }

    public void UnRegisterNPC(NPC npc)
    {
        if (allNpcs.Contains(npc))
            allNpcs.Remove(npc);

        // 如果销毁的NPC正是当前对话对象，强制关闭对话
        if (CurrentTalkingNPC == npc)
            CloseCurrentDialogue();
    }
    #endregion

    #region 交互入口 NPC.Interact 调用
    public void OnNPCInteract(NPC npc)
    {
        // 已有对话在进行 → 跳过打字/下一句
        if (CurrentTalkingNPC != null)
        {
            if (CurrentTalkingNPC != npc) return; // 不能操作其他NPC对话
            NextLine();
            return;
        }

        // 开启新对话
        OpenDialogue(npc);
    }
    #endregion

    #region 开启对话
    private void OpenDialogue(NPC npc)
    {
        CurrentTalkingNPC = npc;
        dialogueIndex = 0;

        // UI赋值
        npc.nameText.SetText(npc.dialogueData.npcName);
        npc.portraitImage.sprite = npc.dialogueData.npcIcon;
        npc.dialoguePanel.SetActive(true);

        // 游戏逻辑暂停（禁止玩家移动）
        InputManager.CanPlayerMove = false;

        // 锁定NPC巡逻移动
        WaypointMover mover = npc.GetComponent<WaypointMover>();
        if (mover != null) mover.isLockedByDialogue = true;

        // 启动打字
        RunTypeLine();
    }
    #endregion

    #region 打字协程
    void RunTypeLine()
    {
        if (typeCoroutine != null) StopCoroutine(typeCoroutine);
        typeCoroutine = StartCoroutine(TypeLine());
    }

    private IEnumerator TypeLine()
    {
        isTyping = true;
        NPC npc = CurrentTalkingNPC;
        npc.dialogueText.SetText("");
        string targetLine = npc.dialogueData.dialogueLines[dialogueIndex];
        StringBuilder sb = new StringBuilder();

        for (int i = 0; i < targetLine.Length; i++)
        {
            char letter = targetLine[i];
            sb.Append(letter);
            npc.dialogueText.text = sb.ToString();

            if (i == 0)
            {
                SoundManager.Instance.PlayClip(npc.dialogueData.voiceSound, npc.dialogueData.voicePitch);
            }
            yield return new WaitForSeconds(npc.dialogueData.typingSpeed);
        }

        isTyping = false;

        // 自动跳转延时
        if (dialogueIndex < npc.dialogueData.autoProgressLines.Length && npc.dialogueData.autoProgressLines[dialogueIndex])
        {
            yield return new WaitForSeconds(npc.dialogueData.autoProgressDelay);
            NextLine();
        }
    }
    #endregion

    #region 下一句 / 跳过打字
    public void NextLine()
    {
        NPC npc = CurrentTalkingNPC;
        if (npc == null) return;

        // 正在打字：直接显示完整文字
        if (isTyping)
        {
            if (typeCoroutine != null) StopCoroutine(typeCoroutine);
            npc.dialogueText.SetText(npc.dialogueData.dialogueLines[dialogueIndex]);
            isTyping = false;
            return;
        }

        // 切换下一段
        dialogueIndex++;
        if (dialogueIndex < npc.dialogueData.dialogueLines.Length)
        {
            RunTypeLine();
        }
        else
        {
            CloseCurrentDialogue();
        }
    }
    #endregion

    #region 统一关闭对话（UI按钮直接调用这个）
    public void CloseCurrentDialogue()
    {
        if (CurrentTalkingNPC == null) return;

        NPC npc = CurrentTalkingNPC;

        // 停止打字协程
        if (typeCoroutine != null)
        {
            StopCoroutine(typeCoroutine);
            typeCoroutine = null;
        }

        // 清空UI
        npc.dialogueText.SetText("");
        npc.dialoguePanel.SetActive(false);

        // 解除游戏逻辑暂停，玩家恢复移动
        InputManager.CanPlayerMove = true;

        // 解锁NPC巡逻移动
        WaypointMover mover = npc.GetComponent<WaypointMover>();
        if (mover != null) mover.isLockedByDialogue = false;

        // 清空当前对话标记
        CurrentTalkingNPC = null;
        dialogueIndex = 0;
        isTyping = false;
    }
    #endregion
}
