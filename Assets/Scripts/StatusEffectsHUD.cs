using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using static StatusEffectSystem.StatusEffect;

public class StatusEffectsHUD : MonoBehaviour
{
    public StatusEffectIcon poisonIcon;
    public CanvasGroup poisonScreenSplashCanvasGroup;
    private float time01;
    public AnimationCurve splashCurve;

    private void Start()
    {
        GameManager.Instance.statusEffectSystem.OnStatusEffectHasChanged += OnStatusEffectHasChanged;
    }

    void OnStatusEffectHasChanged(EffectType type, bool value)
    {
        if (type == EffectType.POISON) poisonIcon.SetEnabled(value);
    }
    
    private void Update()
    {
        
        if (poisonIcon.IsEnabled())
        {
            float remainingTime = GameManager.Instance.statusEffectSystem.GetRemainingStatusEffect(EffectType.POISON);
            float totalTime = GameManager.Instance.statusEffectSystem.GetTotalStatusEffect(EffectType.POISON);
            time01 = Mathf.Clamp01(remainingTime / totalTime);
            poisonIcon.UpdateCircleFillAmount(time01);
            poisonScreenSplashCanvasGroup.alpha = splashCurve.Evaluate(time01);
        }
        else
        {
            if (time01 != 0f)
            {
                time01 = 0f;
                poisonScreenSplashCanvasGroup.alpha = time01;
            }
        }
    }
}
