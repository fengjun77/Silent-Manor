using UnityEngine;

public class UI : Singleton<UI>
{
    public UI_MapTranstionFade mapTranstion {get; private set; }
    public UI_Menu menu { get; private set; }

    protected override void Awake()
    {
        base.Awake();
        mapTranstion = GetComponentInChildren<UI_MapTranstionFade>();
        menu = GetComponentInChildren<UI_Menu>();
    }

    void Start()
    {
        menu.gameObject.SetActive(false);
    }

    void Update()
    {
        if(InputManager.WasTabClicked)
        {
            bool isActive = menu.gameObject.activeSelf;
            // menu.gameObject.SetActive(!isActive);
           
            // InputManager.canPlayerMove = !isActive;

            // 条件：对话激活 → 不执行打开菜单
            if (DialogueIsActive()) return;

            if (isActive)
                CloseMenu();
            else
                OpenMenu();
        }

        if(InputManager.WasSpaceClicked && DialogueIsActive())
        {
            NPCManager.Instance.NextLine();
        }
    }

    // 判断是否存在激活对话（全局简易判断，也可单例对话管理器）
    private bool DialogueIsActive()
    {
        // 当前存在正在对话的NPC = 对话激活
        return NPCManager.Instance.CurrentTalkingNPC != null;
    }

    public void OpenMenu()
    {
        menu.gameObject.SetActive(true);
        GameManager.Instance.IsMenuOpen = true;
        GameManager.Instance.AddPauseLayer();
    }

    public void CloseMenu()
    {
        menu.gameObject.SetActive(false);
        GameManager.Instance.IsMenuOpen = false;
        GameManager.Instance.RemovePauseLayer();
    }

}
