using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UI_Dialogue : MonoBehaviour
{
    [Header("基础对话UI")]
    public Image portraitImage;
    public TextMeshProUGUI txtName;
    public TextMeshProUGUI txtContent;
    public Button btnClose;

    [Header("分支对话UI")]
    public GameObject branchContainer;
    public Button branchBtnPrefab;

    void Awake()
    {
        // 初始隐藏面板和分支
        gameObject.SetActive(false);
        branchContainer.SetActive(false);
        btnClose.onClick.AddListener(NPCManager.Instance.CloseCurrentDialogue);
    }

    // 清空所有动态生成的分支按钮
    public void ClearAllBranchBtns()
    {
        foreach (Transform child in branchContainer.transform)
        {
            if (child.gameObject != branchBtnPrefab)
                Destroy(child.gameObject);
        }
        branchContainer.SetActive(false);
    }
}
