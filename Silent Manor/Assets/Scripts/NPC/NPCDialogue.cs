using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class DialogueLine
{
    [Header("对话文本")]
    public string text;

    [Header("自动推进")]
    public bool autoProgress = false;
    public float autoDelay = 2f;

    [Header("读完本行直接关闭对话")]
    public bool endDialogueAfterThisLine = false;

    [Header("分支选项（不需要则留空）")]
    public DialogueBranch branch;
}

[System.Serializable]
public class DialogueBranch
{
    public DialogueBranchOption[] options;
}

[System.Serializable]
public class DialogueBranchOption
{
    public string optionText;
    public int jumpToLineIndex;
}

[CreateAssetMenu(menuName = "Data/NPCDialogue", fileName = "NewDialogueData")]
public class NPCDialogue : ScriptableObject
{
    [Header("NPC基础信息")]
    public string npcName;
    public Sprite npcIcon;

    [Header("全局打字/语音")]
    public float typingSpeed = 0.05f;
    public AudioClip voiceSound;
    public float voicePitch = 1f;

    [Header("对话行列表（一行一条，自动对齐配置）")]
    public List<DialogueLine> lines = new List<DialogueLine>();
}


