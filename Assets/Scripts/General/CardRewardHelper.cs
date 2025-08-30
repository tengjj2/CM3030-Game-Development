using UnityEngine;
using System.Linq;

public static class CardRewardHelper
{
    public static CardData[] GetRandomRewardOptions(int count)
    {
        // Load all CardData assets in your project under a folder called "Resources/Cards"
        var allCards = Resources.LoadAll<CardData>("Cards");
        if (allCards == null || allCards.Length == 0) return new CardData[0];

        // Shuffle and take N
        return allCards.OrderBy(x => Random.value).Take(count).ToArray();
    }
}