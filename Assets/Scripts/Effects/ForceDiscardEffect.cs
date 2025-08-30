// ForceDiscardEffect.cs
using System.Collections.Generic;
using UnityEngine;

public class ForceDiscardEffect : Effect
{
    [Min(1)] public int Count = 1;

    public override GameAction GetGameAction(List<CombatantView> _, CombatantView __)
    {
        return new ForceDiscardGA(Count);
    }
}