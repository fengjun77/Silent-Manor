using System.Collections;
using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class NPC : MonoBehaviour, IInteractable
{
    [Header("对话数据")]
    public NPCDialogue dialogueData;
    public GameObject dialoguePanel;
    public TextMeshProUGUI dialogueText, nameText;
    public Image portraitImage;

    [Header("交互")]
    public float iconHeightOffset = 1.3f;

    private float skipCoolDown = 0.12f;
    private float lastSkipTime;

    void Awake()
    {
        NPCManager.Instance.RegisterNPC(this);
    }

    void OnDestroy()
    {
        //NPCManager.Instance.UnRegisterNPC(this);
    }

    #region IInteractable
    public bool CanInteract()
    {
        return !GameManager.Instance.IsMenuOpen;
    }

    public void Interact()
    {
        if (Time.time - lastSkipTime < skipCoolDown) return;
        lastSkipTime = Time.time;

        if (dialogueData == null) return;

        // 全部交给管理器处理对话逻辑
        NPCManager.Instance.OnNPCInteract(this);
    }

    public Vector3 GetIconSpawnPos()
    {
        return transform.position + Vector3.up * iconHeightOffset;
    }
    #endregion
}
