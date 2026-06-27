using UnityEngine;

[CreateAssetMenu(menuName = "Data/NPCDialogue", fileName = "NewDialogueData")]
public class NPCDialogue : ScriptableObject
{
    public string npcName;
    public Sprite npcIcon;
    public string[] dialogueLines;
    public bool[] autoProgressLines; // 此行是否自动推进
    public float autoProgressDelay; //此行自动推进时间

    public float typingSpeed = 0.05f; //文字出现速度
    public AudioClip voiceSound;
    public float voicePitch = 1f;
}
