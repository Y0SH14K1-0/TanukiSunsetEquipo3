using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class StatusEffectSystem : MonoBehaviour
{
    private List<StatusEffect> currentStatusEffects;

    public delegate void OnStatusEffectHasChangedDelegate(StatusEffect.EffectType effect, bool status);
    public OnStatusEffectHasChangedDelegate OnStatusEffectHasChanged;

    private void Start()
    {
        Initialize();
    }

    private void Initialize()
    {
        if (currentStatusEffects == null) currentStatusEffects = new List<StatusEffect>();
    }

    public void AddTemporaryStatusEffect(StatusEffect.EffectType effect, float duration)
    {
        Initialize();
        StatusEffect newStatusEffect = new StatusEffect(StatusEffect.StatusType.TEMP, effect, duration);
        if (OnStatusEffectHasChanged != null && !HasStatusEffect(effect)) OnStatusEffectHasChanged(effect, true);
        currentStatusEffects.Add(newStatusEffect);
    }

    private void Update()
    {
        List<StatusEffect> statusEffectToRemove = new List<StatusEffect>();

        foreach (StatusEffect statusEffect in currentStatusEffects)
        {
            if (statusEffect.status == StatusEffect.StatusType.TEMP)
            {
                statusEffect.duration -= Time.deltaTime;
                if (statusEffect.duration <= 0f)
                {
                    statusEffectToRemove.Add(statusEffect);
                }
            }
        }

        foreach (StatusEffect statusEffect in statusEffectToRemove)
        {
            currentStatusEffects.Remove(statusEffect);
            if (OnStatusEffectHasChanged != null && !HasStatusEffect(statusEffect.effect)) OnStatusEffectHasChanged(statusEffect.effect, false);
        }
    }

    public bool HasStatusEffect(StatusEffect.EffectType effect)
    {
        List<StatusEffect> validStatusEffects = currentStatusEffects.FindAll(x => x.effect == effect);
        return validStatusEffects.Count > 0;
    }

    public float GetRemainingStatusEffect(StatusEffect.EffectType effect)
    {
        List<StatusEffect> validStatusEffects = currentStatusEffects.FindAll(x => x.effect == effect);
        return (from x in validStatusEffects select x.duration).Sum();
    }

    public float GetTotalStatusEffect(StatusEffect.EffectType effect)
    {
        List<StatusEffect> validStatusEffects = currentStatusEffects.FindAll(x => x.effect == effect);
        return (from x in validStatusEffects select x.initialDuration).Sum();
    }

    public class StatusEffect
    {
        public StatusEffect(StatusType _status, EffectType _effect, float _duration)
        {
            status = _status;
            effect = _effect;
            duration = _duration;
            initialDuration = _duration;
        }

        public StatusType status;
        public EffectType effect;
        public float duration;
        public float initialDuration;

        public enum EffectType
        {
            POISON,
        }

        public enum StatusType
        {
            TEMP,
        }
    }
}
