using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class UI_MapTranstionFade : MonoBehaviour
{
    public Image fadeImage;
    public float fadeDuration = .5f;

    private float currentAlpha;
    private Coroutine currentFadeCoroutine;

    void Start()
    {
        fadeImage.gameObject.SetActive(false);
    }  

    private void SetAlpha(float value)
    {
        Color color = fadeImage.color;
        color.a = Mathf.Clamp01(value);
        fadeImage.color = color;
        currentAlpha = color.a;
    }

    public void FadeIn()
    {
        if(currentFadeCoroutine != null) 
            StopCoroutine(currentFadeCoroutine);
        fadeImage.gameObject.SetActive(true);
        currentFadeCoroutine = StartCoroutine(FadeProcess(1,0));
    }

    public void FadeOut()
    {
        fadeImage.gameObject.SetActive(true);
        if(currentFadeCoroutine != null) 
            StopCoroutine(currentFadeCoroutine);
        currentFadeCoroutine = StartCoroutine(FadeProcess(0,1));
    }

    // 核心渐变协程
    private IEnumerator FadeProcess(float startAlpha, float targetAlpha)
    {
        float timer = 0f;
        SetAlpha(startAlpha);

        while (timer < fadeDuration)
        {
            timer += Time.deltaTime;
            float t = timer / fadeDuration;
            // 平滑插值透明度
            float newAlpha = Mathf.Lerp(startAlpha, targetAlpha, t);
            SetAlpha(newAlpha);
            yield return null;
        }
        // 强制锁定最终值，防止浮点误差
        SetAlpha(targetAlpha);
        currentFadeCoroutine = null;

        if(targetAlpha == 0) fadeImage.gameObject.SetActive(false);
    }
}
