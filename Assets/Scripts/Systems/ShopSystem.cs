using UnityEngine;
using System;

public class ShopSystem : Singleton<ShopSystem>
{
    // Hook this up to a Shop UI later. For now, quick open/close API.
    public void Open(ShopInventorySO inv, Action onClose)
    {
        if (inv == null || inv.Items == null || inv.Items.Count == 0)
        {
            Debug.Log("[Shop] Empty inventory. Closing.");
            onClose?.Invoke();
            return;
        }

        Debug.Log($"[Shop] Open with {inv.Items.Count} items. Gold={CurrencySystem.Instance.Gold}");
        // TODO: Show UI list. For now: auto-buy first item if affordable; then close.
        var item = inv.Items[0];
        TryBuy(item);
        onClose?.Invoke();
    }

    public bool TryBuy(ShopEntry entry)
    {
        if (entry == null) return false;
        /*
        if (!CurrencySystem.Instance.CanAfford(entry.price))
        {
            Debug.Log("[Shop] Not enough gold.");
            return false;
        }*/

        if (!CurrencySystem.Instance.Spend(entry.price)) return false;

        switch (entry.type)
        {
            case ShopItemType.Card:
                if (entry.card != null)
                {
                    // Add to persistent deck (PlayerData.Deck is used by CardSystem.Setup at start)
                    //PlayerSystem.Instance.PlayerView.PlayerDeck_AddCard(entry.card);
                    Debug.Log($"[Shop] Bought card: {entry.card.name}");
                }
                break;

            case ShopItemType.Perk:
                if (entry.perk != null)
                {
                    PerkSystem.Instance.AddPerk(new Perk(entry.perk));
                    Debug.Log($"[Shop] Bought perk: {entry.perk.name}");
                }
                break;

            case ShopItemType.Heal:
                {
                    var pv = PlayerSystem.Instance.PlayerView;
                    if (pv != null)
                    {
                        // Use your HealGA if present, otherwise directly:
                        pv.Heal(entry.healAmount); // add this helper on PlayerView/CombatantView if needed
                        Debug.Log($"[Shop] Healed +{entry.healAmount}");
                    }
                }
                break;
        }

        return true;
    }
}