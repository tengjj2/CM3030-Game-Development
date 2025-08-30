using System.Collections.Generic;
using DG.Tweening;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class CombatantView : MonoBehaviour
{
    [SerializeField] private TMP_Text healthText;
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private Image healthBarFill;
    [SerializeField] private StatusEffectsUI statusEffectsUI;

    public int MaxHealth { get; private set; }
    public int CurrentHealth { get; set; }

    private readonly Dictionary<StatusEffectType, int> statusEffects = new();

    // ---------- Lifecycle / setup ----------
    protected void SetupBase(int health, Sprite image)
    {
        MaxHealth = CurrentHealth = health;
        spriteRenderer.sprite = image;
        RefreshHealthUI();
    }

    // ---------- Public helpers ----------
    /// <summary>
    /// Convenience wrapper to enqueue a GainBlockGA for this unit.
    /// Keeps block *math* inside BlockSystem/performer, not this view.
    /// </summary>
    public void GainBlock(int baseAmount, CombatantView caster = null)
    {
        // default: self-cast block
        caster ??= this;
        ActionSystem.Instance.AddReaction(new ApplyBlockGA(this, caster, baseAmount));
    }

    /// <summary>
    /// Direct HP setter for systems that already decided the final HP.
    /// Preserves view responsibility for UI updates/animation.
    /// </summary>
    public void SetHealth(int newValue)
    {
        CurrentHealth = Mathf.Clamp(newValue, 0, MaxHealth);
        transform.DOShakePosition(0.2f, 0.5f);
        RefreshHealthUI();
    }

    // ---------- Damage application (consumes BLOCK, then HP) ----------
    public void Damage(int damageAmount)
    {
        int remainingDamage = Mathf.Max(0, damageAmount);

        // --- BARRIER: consume 1 stack to negate this instance entirely ---
        int barrier = GetStatusEffectStacks(StatusEffectType.BARRIER);
        if (barrier > 0)
        {
            RemoveStatusEffect(StatusEffectType.BARRIER, 1);
            // optional: tiny “parry” shake or flash
            transform.DOShakePosition(0.15f, 0.2f);
            return; // damage fully negated
        }

        int currentBlock = GetStatusEffectStacks(StatusEffectType.BLOCK);
        if (currentBlock > 0)
        {
            if (currentBlock >= remainingDamage) // block fully absorbs
            {
                RemoveStatusEffect(StatusEffectType.BLOCK, remainingDamage);
                remainingDamage = 0;
            }
            else // partial absorb
            {
                RemoveStatusEffect(StatusEffectType.BLOCK, currentBlock);
                remainingDamage -= currentBlock;
            }
        }

        if (remainingDamage > 0)
        {
            CurrentHealth -= remainingDamage;
            if (CurrentHealth < 0) CurrentHealth = 0;
        }

        transform.DOShakePosition(0.2f, 0.5f);
        RefreshHealthUI();
    }

    // ---------- Status effects ----------
    public void AddStatusEffect(StatusEffectType type, int stackCount)
    {
        if (stackCount <= 0) return;

        if (statusEffects.ContainsKey(type))
            statusEffects[type] += stackCount;
        else
            statusEffects.Add(type, stackCount);

        statusEffectsUI.UpdateStatusEffecctUI(type, GetStatusEffectStacks(type));
    }

    public void RemoveStatusEffect(StatusEffectType type, int stackCount)
    {
        if (stackCount <= 0) return;

        if (statusEffects.ContainsKey(type))
        {
            statusEffects[type] -= stackCount;
            if (statusEffects[type] <= 0)
                statusEffects.Remove(type);

            statusEffectsUI.UpdateStatusEffecctUI(type, GetStatusEffectStacks(type));
        }
    }

    public void Heal(int amount)
    {
        if (amount <= 0) return;
        CurrentHealth = Mathf.Min(MaxHealth, CurrentHealth + amount);

        // Optional feedback
        transform.DOPunchScale(Vector3.one * 0.05f, 0.15f, 8, 0.8f);

        // Update health bar/text as you already do
        if (healthBarFill != null)
        {
            float pct = (float)CurrentHealth / MaxHealth;
            healthBarFill.fillAmount = pct;
        }
        RefreshHealthUI();
    }

    public int GetStatusEffectStacks(StatusEffectType type)
    {
        return statusEffects.TryGetValue(type, out var stacks) ? stacks : 0;
    }

    // ---------- UI ----------
    public void RefreshHealthUI()
    {
        if (healthText != null)
            healthText.text = CurrentHealth + " / " + MaxHealth;

        if (healthBarFill != null && MaxHealth > 0)
            healthBarFill.fillAmount = (float)CurrentHealth / MaxHealth;
    }

    public void IncreaseMaxHealth(int amount, bool healBySameAmount = true)
    {
        if (amount == 0) { RefreshHealthUI(); return; }

        int newMax = Mathf.Max(1, MaxHealth + amount);
        MaxHealth = newMax;

        if (healBySameAmount)
            CurrentHealth = Mathf.Min(MaxHealth, CurrentHealth + amount);
        else
            CurrentHealth = Mathf.Min(CurrentHealth, MaxHealth);

        RefreshHealthUI();
    }
    
    public void ClearAllStatusEffects()
    {
        // remove every status that currently has stacks
        foreach (StatusEffectType t in System.Enum.GetValues(typeof(StatusEffectType)))
        {
            int stacks = GetStatusEffectStacks(t);
            if (stacks > 0)
                RemoveStatusEffect(t, stacks);
        }

    }
}