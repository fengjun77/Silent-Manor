using System.Collections;
using System.Collections.Generic;
using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class NPCManager : Singleton<NPCManager>
{
    [Header("全局唯一对话UI，只拖一次")]
    public UI_Dialogue dialogueUI;

    private List<NPC> allNpcs = new List<NPC>();
    public NPC CurrentTalkingNPC { get; private set; }

    private int dialogueIndex;
    private bool isTyping;
    private bool isInBranchDialogue; // 是否处于分支选择阶段
    // 新增：当前行是分支行，打字阶段就禁用跳过
    private bool waitingBranchPop;
    private Coroutine typeCoroutine;

    #region 注册注销
    public void RegisterNPC(NPC npc)
    {
        if (!allNpcs.Contains(npc))
            allNpcs.Add(npc);
    }

    public void UnRegisterNPC(NPC npc)
    {
        if (allNpcs.Contains(npc))
            allNpcs.Remove(npc);
        if (CurrentTalkingNPC == npc)
            CloseCurrentDialogue();
    }
    #endregion

    #region NPC交互入口
    public void OnNPCInteract(NPC npc)
    {
        if (CurrentTalkingNPC != null)
        {
            if (CurrentTalkingNPC != npc) return;
            NextLine();
            return;
        }
        OpenDialogue(npc);
    }
    #endregion

    #region 打开对话
    private void OpenDialogue(NPC npc)
    {
        CurrentTalkingNPC = npc;
        dialogueIndex = 0;
        isInBranchDialogue = false;

        // 给全局UI赋值NPC数据
        NPCDialogue data = npc.dialogueData;
        dialogueUI.txtName.text = data.npcName;
        dialogueUI.portraitImage.sprite = data.npcIcon;
        dialogueUI.txtContent.text = "";
        dialogueUI.ClearAllBranchBtns();
        dialogueUI.gameObject.SetActive(true);

        InputManager.CanPlayerMove = false;
        WaypointMover mover = npc.GetComponent<WaypointMover>();
        if (mover != null) mover.isLockedByDialogue = true;

        RunTypeLine();
    }
    #endregion

    #region 打字协程 + 分支检测
    void RunTypeLine()
    {
        if (typeCoroutine != null) StopCoroutine(typeCoroutine);
        typeCoroutine = StartCoroutine(TypeLine());
    }

    private IEnumerator TypeLine()
    {
        isTyping = true;
        NPC npc = CurrentTalkingNPC;
        NPCDialogue data = npc.dialogueData;
        DialogueLine currentLine = data.lines[dialogueIndex];

        dialogueUI.txtContent.SetText("");

        // 提前判断本行是否带分支，打字阶段锁定跳过
        waitingBranchPop = false;
        if (currentLine.branch != null && currentLine.branch.options != null && currentLine.branch.options.Length > 0)
        {
            waitingBranchPop = true;
        }

        string targetLine = currentLine.text;
        StringBuilder sb = new StringBuilder();

        for (int i = 0; i < targetLine.Length; i++)
        {
            char letter = targetLine[i];
            sb.Append(letter);
            dialogueUI.txtContent.text = sb.ToString();
            if (i == 0)
                SoundManager.Instance.PlayClip(data.voiceSound, data.voicePitch);
            yield return new WaitForSeconds(data.typingSpeed);
        }

        isTyping = false;
        waitingBranchPop = false;

        // 生成分支按钮
        CheckBranchOptions(npc, currentLine);

        // 判断读完直接结束对话
        if (currentLine.endDialogueAfterThisLine)
        {
            yield return new WaitForSeconds(currentLine.autoDelay);
            CloseCurrentDialogue();
            yield break;
        }

        // 非分支行且开启自动推进
        if (!isInBranchDialogue && currentLine.autoProgress)
        {
            yield return new WaitForSeconds(currentLine.autoDelay);
            NextLine();
        }
    }

    // 传入当前行，不再读取数组
void CheckBranchOptions(NPC npc, DialogueLine currentLine)
{
    NPCDialogue data = npc.dialogueData;
    dialogueUI.ClearAllBranchBtns();
    isInBranchDialogue = false;

    DialogueBranch br = currentLine.branch;
    if (br == null || br.options == null || br.options.Length == 0)
        return;

    isInBranchDialogue = true;
    dialogueUI.branchContainer.SetActive(true);

    foreach (var opt in br.options)
    {
        Button btn = Instantiate(dialogueUI.branchBtnPrefab, dialogueUI.branchContainer.transform);
        btn.gameObject.SetActive(true);
        TextMeshProUGUI txt = btn.GetComponentInChildren<TextMeshProUGUI>();
        if (txt != null) txt.text = opt.optionText;

        DialogueBranchOption cacheOpt = opt;
        btn.onClick.AddListener(() => OnBranchClick(cacheOpt.jumpToLineIndex));
    }
}

    // 分支点击跳转
    void OnBranchClick(int jumpIndex)
    {
        NPC npc = CurrentTalkingNPC;
        NPCDialogue data = npc.dialogueData;
        dialogueUI.ClearAllBranchBtns();
        isInBranchDialogue = false;

        // 跳转校验
        if (jumpIndex < 0 || jumpIndex >= data.lines.Count)
        {
            CloseCurrentDialogue();
            return;
        }
        dialogueIndex = jumpIndex;
        RunTypeLine();
    }
    #endregion

    #region 下一句（分支阶段禁用）
    public void NextLine()
    {
        NPC npc = CurrentTalkingNPC;
        NPCDialogue data = npc.dialogueData;
        DialogueLine currentLine = data.lines[dialogueIndex];

        if (npc == null || isInBranchDialogue) return;

        if (isInBranchDialogue || waitingBranchPop)
            return;

        if (currentLine.endDialogueAfterThisLine)
        {
            CloseCurrentDialogue();
            return;
        }

        if (isTyping)
        {
            if (typeCoroutine != null) StopCoroutine(typeCoroutine);
            dialogueUI.txtContent.text = npc.dialogueData.lines[dialogueIndex].text;
            isTyping = false;
            return;
        }

        dialogueIndex++;
        if (dialogueIndex < npc.dialogueData.lines.Count)
            RunTypeLine();
        else
            CloseCurrentDialogue();
    }
    #endregion

    #region 统一关闭对话
    public void CloseCurrentDialogue()
    {
        if (CurrentTalkingNPC == null) return;
        NPC npc = CurrentTalkingNPC;

        if (typeCoroutine != null)
        {
            StopCoroutine(typeCoroutine);
            typeCoroutine = null;
        }

        // 清空全局UI
        dialogueUI.txtContent.SetText("");
        dialogueUI.ClearAllBranchBtns();
        dialogueUI.gameObject.SetActive(false);

        InputManager.CanPlayerMove = true;
        WaypointMover mover = npc.GetComponent<WaypointMover>();
        if (mover != null) mover.isLockedByDialogue = false;

        CurrentTalkingNPC = null;
        dialogueIndex = 0;
        isTyping = false;
        isInBranchDialogue = false;
    }
    #endregion
}
