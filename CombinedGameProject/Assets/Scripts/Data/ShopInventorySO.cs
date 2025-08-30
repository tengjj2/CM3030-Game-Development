using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Shop/Inventory")]
public class ShopInventorySO : ScriptableObject
{
    public List<ShopEntry> Items = new();
}

[System.Serializable]
public class ShopEntry
{
    public ShopItemType type = ShopItemType.Card;
    public int price = 50;

    public CardData card;       // used if type == Card
    public PerkData perk;       // used if type == Perk
    public int healAmount = 10; // used if type == Heal (flat heal)

    public string DisplayName =>
        type switch
        {
            ShopItemType.Card => card ? card.name : "Card",
            ShopItemType.Perk => perk ? perk.name : "Perk",
            ShopItemType.Heal => $"Heal +{healAmount}",
            _ => "Item"
        };
}

public enum ShopItemType { Card, Perk, Heal }