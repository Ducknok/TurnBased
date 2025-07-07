using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIFade : Singleton<UIFade>
{
    [SerializeField] private Image fadeScreen;
    [SerializeField] private float fadeSpeed;

    private IEnumerator fadeCoroutine;

    public void FadeToBlack()
    {
        if(this.fadeCoroutine != null)
        {
            StopCoroutine(this.fadeCoroutine);
        }
        this.fadeCoroutine = this.FadeRoutine(1);
        StartCoroutine(this.fadeCoroutine);
    }
    public void FadeToClear()
    {
        if(this.fadeCoroutine != null)
        {
            StopCoroutine(this.fadeCoroutine);
        }
        this.fadeCoroutine = FadeRoutine(0);
        StartCoroutine(this.fadeCoroutine);
    }
    private IEnumerator FadeRoutine(float targetAlpha)
    {
         while(!Mathf.Approximately(fadeScreen.color.a, targetAlpha))
        {
            float alpha = Mathf.MoveTowards(fadeScreen.color.a, targetAlpha, this.fadeSpeed * Time.deltaTime);
            this.fadeScreen.color = new Color(this.fadeScreen.color.r, this.fadeScreen.color.g, this.fadeScreen.color.b, alpha);
            yield return null;
        }
    }
}
