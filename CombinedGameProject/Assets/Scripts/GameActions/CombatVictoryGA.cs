// CombatVictoryGA.cs
using System.Collections.Generic;
using UnityEngine;

public class CombatVictoryGA : GameAction
{
    // Optional rewards (tweak to your game’s needs)
    public int Gold { get; }
    public int HealAmount { get; }
    public List<CardData> CardRewardPool { get; }   // pool to choose from
    public int PickCardCount { get; }               // how many to pick

    public CombatVictoryGA(int gold = 0, int healAmount = 0,
                           List<CardData> cardRewardPool = null, int pickCardCount = 0)
    {
        Gold = Mathf.Max(0, gold);
        HealAmount = Mathf.Max(0, healAmount);
        CardRewardPool = cardRewardPool != null ? new List<CardData>(cardRewardPool) : null;
        PickCardCount = Mathf.Max(0, pickCardCount);
    }
}
