using UnityEngine;
using System.Collections.Generic;
public class DrawCardEffect : Effect
{
    [SerializeField] private int drawAmount;
    public override GameAction GetGameAction(List<CombatantView> targets)
    {
        DrawCardsGA drawCardsGA = new(drawAmount);
        return drawCardsGA;
    }
}
