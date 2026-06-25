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
            menu.gameObject.SetActive(!menu.gameObject.activeSelf);
            InputManager.canPlayerMove = !menu.gameObject.activeSelf;
        }
    }

}
