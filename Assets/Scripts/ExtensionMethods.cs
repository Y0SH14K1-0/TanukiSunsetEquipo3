using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class ExtensionMethods
{
    private static System.Random rng = new System.Random();

    public static void Shuffle<T>(this IList<T> list)
    {
        int n = list.Count;
        while (n > 1)
        {
            n--;
            int k = rng.Next(n + 1);
            T value = list[k];
            list[k] = list[n];
            list[n] = value;
        }
    }
    public static void FadeTo(this CanvasGroup canvasGroup, MonoBehaviour mono, float targetAlpha, float duration, Action callback = null, bool scaledTime = true)
    {
        mono.StartCoroutine(C_FadeTo(canvasGroup, targetAlpha, duration, callback, scaledTime));
    }

    public static void SetInteractable(this CanvasGroup canvasGroup, bool value)
    {
        if (canvasGroup == null) return;
        canvasGroup.interactable = value;
        canvasGroup.blocksRaycasts = value;
    }

    static IEnumerator C_FadeTo(CanvasGroup canvasGroup, float targetAlpha, float duration, Action callback = null, bool scaledTime = true)
    {
        float startAlpha = canvasGroup.alpha;
        float increment = scaledTime ? Time.deltaTime : Time.fixedUnscaledDeltaTime;
        for (float f = 0f; f <= 1f; f += increment / duration)
        {
            canvasGroup.alpha = Mathf.Lerp(startAlpha, targetAlpha, Mathf.SmoothStep(0f, 1f, f));
            yield return null;
        }
        canvasGroup.alpha = targetAlpha;
        if (callback != null) callback();
    }
}
