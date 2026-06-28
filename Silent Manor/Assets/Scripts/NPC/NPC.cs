
using UnityEngine;

public class NPC : MonoBehaviour, IInteractable
{
    [Header("仅对话数据，无需绑定任何UI")]
    public NPCDialogue dialogueData;

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
        return !GameManager.Instance.IsMenuOpen;
    }

    public void Interact()
    {
        if (Time.time - lastSkipTime < skipCoolDown) return;
        lastSkipTime = Time.time;
        if (dialogueData == null) return;
        NPCManager.Instance.OnNPCInteract(this);
    }

    public Vector3 GetIconSpawnPos()
    {
        return transform.position + Vector3.up * iconHeightOffset;
    }
    #endregion
}
