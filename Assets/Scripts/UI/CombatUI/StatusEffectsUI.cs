using System.Collections.Generic;
using UnityEngine;

public class StatusEffectsUI : MonoBehaviour
{
    [SerializeField] private StatusEffectUI statusEffectUIPrefab;

    [Header("Sprites for Status Effects")]
    [SerializeField] private Sprite blockSprite;
    [SerializeField] private Sprite healSprite;
    [SerializeField] private Sprite strengthSprite;
    [SerializeField] private Sprite defenceSprite;
    [SerializeField] private Sprite weakenSprite;
    [SerializeField] private Sprite frailSprite;
    [SerializeField] private Sprite barrierSprite;
    [SerializeField] private Sprite thornsSprite;
    [SerializeField] private Sprite burnSprite;
    [SerializeField] private Sprite poisonSprite;
    [SerializeField] private Sprite confuseSprite;
    [SerializeField] private Sprite exhaustSprite;
    [SerializeField] private Sprite discardSprite;
    [SerializeField] private Sprite energyGainSprite;
    [SerializeField] private Sprite energyLossSprite;

    private Dictionary<StatusEffectType, StatusEffectUI> statusEffectUIs = new();

    public void UpdateStatusEffecctUI(StatusEffectType statusEffectType, int stackCount)
    {
        // --- Clear if 0 stacks ---
        if (stackCount <= 0)
        {
            if (statusEffectUIs.TryGetValue(statusEffectType, out var existingUI) && existingUI != null)
            {
                statusEffectUIs.Remove(statusEffectType);
                Destroy(existingUI.gameObject);
            }
            return;
        }

        // --- Create if missing ---
        if (!statusEffectUIs.TryGetValue(statusEffectType, out var statusEffectUI) || statusEffectUI == null)
        {
            statusEffectUI = Instantiate(statusEffectUIPrefab, transform);
            statusEffectUIs[statusEffectType] = statusEffectUI;
        }

        // --- Update sprite + stack count ---
        Sprite sprite = GetSpriteByType(statusEffectType);
        statusEffectUI.Set(sprite, stackCount);
    }

    private Sprite GetSpriteByType(StatusEffectType statusEffectType)
    {
        return statusEffectType switch
        {
            StatusEffectType.BLOCK       => blockSprite,
            StatusEffectType.HEAL        => healSprite,
            StatusEffectType.STRENGTH    => strengthSprite,
            StatusEffectType.DEFENCE     => defenceSprite,
            StatusEffectType.WEAKEN      => weakenSprite,
            StatusEffectType.FRAIL       => frailSprite,
            StatusEffectType.BARRIER     => barrierSprite,
            StatusEffectType.THORNS      => thornsSprite,
            StatusEffectType.BURN        => burnSprite,
            StatusEffectType.POISON      => poisonSprite,
            StatusEffectType.CONFUSE     => confuseSprite,
            StatusEffectType.EXHAUST     => exhaustSprite,
            StatusEffectType.DISCARD     => discardSprite,
            StatusEffectType.ENERGYGAIN  => energyGainSprite,
            StatusEffectType.ENERGYLOSS  => energyLossSprite,
            _ => null,
        };
    }

    /// <summary>
    /// Utility: Clears all status effect icons safely.
    /// </summary>
    public void ClearAll()
    {
        foreach (var kvp in statusEffectUIs)
        {
            if (kvp.Value != null)
                Destroy(kvp.Value.gameObject);
        }
        statusEffectUIs.Clear();
    }
}
