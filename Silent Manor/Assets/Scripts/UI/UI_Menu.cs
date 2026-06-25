using UnityEngine;
using UnityEngine.UI;

public class UI_Menu : MonoBehaviour
{
    public Image[] tabs;
    public GameObject[] pages;

    void Start()
    {
        ActivateTab(0);
    }

    public void ActivateTab(int tabNum)
    {
        for(int i = 0; i < tabs.Length; i++)
        {
            tabs[i].color = Color.gray;
            pages[i].SetActive(false);
        }

        pages[tabNum].SetActive(true);
        tabs[tabNum].color = Color.white;
    }
}
