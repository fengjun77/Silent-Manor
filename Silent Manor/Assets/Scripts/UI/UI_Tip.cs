using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UI_Tip : MonoBehaviour
{
    public Image iconImg;
    public TextMeshProUGUI textTMP;
    public float showTime = 2f;
    public float moveUpSpeed = 50f;

    private float timer;
    private CanvasGroup cg;

    void Awake()
    {
        cg = GetComponent<CanvasGroup>();
        if (cg == null)
            cg = gameObject.AddComponent<CanvasGroup>();
    }

    /// 初始化弹窗内容
    public void Setup(ItemData item, int count)
    {
        iconImg.sprite = item.icon;
        textTMP.text = $"获得 {item.itemName} x{count}";
        timer = showTime;
        cg.alpha = 1;
    }

    void Update()
    {
        transform.Translate(Vector2.up * moveUpSpeed * Time.deltaTime);
        timer -= Time.deltaTime;
        if (timer < 0.5f)
        {
            cg.alpha -= Time.deltaTime * 2;
        }
        if (timer <= 0)
        {
            Destroy(gameObject);
        }
    }
}
